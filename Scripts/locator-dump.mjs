#!/usr/bin/env node
// One-shot accessibility-tree dump for UI-locator authoring (Rules/ui-testing.md → Locators).
// No npm dependencies: Node's built-in WebSocket/fetch + the Playwright-managed Chromium.

import { spawn } from 'node:child_process'
import { existsSync, mkdtempSync, readdirSync, readFileSync, rmSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { join } from 'node:path'

const DEFAULT_STATE_PATH = join('.auth', 'github-ui-state.json')
const DEFAULT_TIMEOUT_MS = 15000
const CHROMIUM_STARTUP_TIMEOUT_MS = 20000
const COMMAND_TIMEOUT_MS = 30000
const POLL_INTERVAL_MS = 100
const VIEWPORT = '--window-size=1400,900'

const INTERESTING_ROLES = new Set([
  'button', 'link', 'textbox', 'searchbox', 'combobox', 'checkbox', 'radio',
  'switch', 'tab', 'menuitem', 'menuitemcheckbox', 'menuitemradio', 'option',
  'heading', 'slider', 'spinbutton', 'treeitem', 'summary'
])

const USAGE = `Usage:
  node Scripts/locator-dump.mjs <url> [options]

Dumps role + accessible name of interactive elements (for GetByRole/GetByLabel locators).

Options:
  --state <path>   storageState JSON with cookies
                   (default: ${DEFAULT_STATE_PATH} when it exists)
  --anonymous      do not load any state
  --waitfor <text> wait until body innerText contains <text> (case-insensitive)
  --timeout <ms>   navigation/text timeout (default: ${DEFAULT_TIMEOUT_MS})
  --headed         show the browser window instead of headless
  -h, --help       show this help

Examples:
  node Scripts/locator-dump.mjs https://github.com/Gredja/AiTest/issues/new --waitfor "Create new issue"
  node Scripts/locator-dump.mjs https://github.com/Gredja/AiTest --anonymous`

function fail(message) {
  throw new Error(message)
}

function sleep(milliseconds) {
  return new Promise(resolve => setTimeout(resolve, milliseconds))
}

function parseArguments(argumentList) {
  const options = {
    url: null,
    state: null,
    anonymous: false,
    waitFor: null,
    timeoutMs: DEFAULT_TIMEOUT_MS,
    headed: false,
    help: false
  }
  for (let index = 0; index < argumentList.length; index += 1) {
    const argument = argumentList[index]
    if (argument === '-h' || argument === '--help') {
      options.help = true
    } else if (argument === '--state') {
      options.state = argumentList[++index]
      if (!options.state) fail('--state requires a path')
    } else if (argument === '--anonymous') {
      options.anonymous = true
    } else if (argument === '--waitfor') {
      options.waitFor = argumentList[++index]
      if (!options.waitFor) fail('--waitfor requires a text')
    } else if (argument === '--timeout') {
      options.timeoutMs = Number(argumentList[++index])
      if (!Number.isFinite(options.timeoutMs) || options.timeoutMs <= 0) {
        fail('--timeout requires a positive number of ms')
      }
    } else if (argument === '--headed') {
      options.headed = true
    } else if (argument.startsWith('--')) {
      fail(`unknown option: ${argument}`)
    } else if (options.url) {
      fail(`unexpected argument: ${argument}`)
    } else {
      options.url = argument
    }
  }
  return options
}

function resolveStatePath(options) {
  if (options.anonymous) return null
  if (options.state) {
    if (!existsSync(options.state)) fail(`state file not found: ${options.state}`)
    return options.state
  }
  return existsSync(DEFAULT_STATE_PATH) ? DEFAULT_STATE_PATH : null
}

function findChromiumExecutable() {
  const override = process.env.CHROMIUM_PATH
  if (override) {
    if (existsSync(override)) return override
    fail(`CHROMIUM_PATH does not exist: ${override}`)
  }
  const msPlaywrightRoot = process.env.LOCALAPPDATA
    ? join(process.env.LOCALAPPDATA, 'ms-playwright')
    : null
  if (!msPlaywrightRoot || !existsSync(msPlaywrightRoot)) {
    fail('ms-playwright browsers not found — run Scripts/install-playwright.ps1 or set CHROMIUM_PATH')
  }
  const executableName = process.platform === 'win32' ? 'chrome.exe' : 'chrome'
  const chromiumDirectories = readdirSync(msPlaywrightRoot)
    .map(directory => ({ directory, match: /^chromium-(\d+)$/.exec(directory) }))
    .filter(entry => entry.match)
    .sort((left, right) => Number(right.match[1]) - Number(left.match[1]))
  for (const entry of chromiumDirectories) {
    for (const platformDirectory of ['chrome-win64', 'chrome-win', 'chrome-linux']) {
      const candidate = join(msPlaywrightRoot, entry.directory, platformDirectory, executableName)
      if (existsSync(candidate)) return candidate
    }
  }
  fail(`no chromium executable under ${msPlaywrightRoot} — run Scripts/install-playwright.ps1`)
}

function loadStorageStateCookies(statePath) {
  const state = JSON.parse(readFileSync(statePath, 'utf8'))
  return (state.cookies ?? []).map(cookie => ({
    name: cookie.name,
    value: cookie.value,
    domain: cookie.domain,
    path: cookie.path ?? '/',
    expires: cookie.expires ?? -1,
    httpOnly: cookie.httpOnly ?? false,
    secure: cookie.secure ?? false,
    sameSite: cookie.sameSite ?? 'Lax'
  }))
}

function launchChromium(executablePath, profileDirectory, headed) {
  const argumentList = [
    '--remote-debugging-port=0',
    `--user-data-dir=${profileDirectory}`,
    '--no-first-run',
    '--no-default-browser-check',
    '--disable-extensions',
    '--disable-sync',
    '--disable-translate',
    '--disable-background-networking',
    '--metrics-recording-only',
    '--mute-audio',
    VIEWPORT
  ]
  if (!headed) argumentList.push('--headless=new')
  const child = spawn(executablePath, argumentList, { stdio: ['ignore', 'ignore', 'pipe'] })
  let stderrLog = ''
  child.stderr.on('data', chunk => {
    stderrLog = `${stderrLog}${chunk.toString()}`.slice(-4000)
  })
  return { child, stderrLog: () => stderrLog }
}

async function waitForDevToolsPort(profileDirectory, browser) {
  const portFile = join(profileDirectory, 'DevToolsActivePort')
  const deadline = Date.now() + CHROMIUM_STARTUP_TIMEOUT_MS
  while (Date.now() < deadline) {
    if (browser.child.exitCode !== null) {
      fail(`chromium exited with code ${browser.child.exitCode}:\n${browser.stderrLog()}`)
    }
    if (existsSync(portFile)) {
      const [firstLine] = readFileSync(portFile, 'utf8').split('\n')
      const port = Number.parseInt(firstLine, 10)
      if (Number.isInteger(port) && port > 0) return port
    }
    await sleep(POLL_INTERVAL_MS)
  }
  fail(`DevToolsActivePort did not appear within ${CHROMIUM_STARTUP_TIMEOUT_MS}ms:\n${browser.stderrLog()}`)
}

async function fetchBrowserWebSocketUrl(port) {
  const deadline = Date.now() + CHROMIUM_STARTUP_TIMEOUT_MS
  while (Date.now() < deadline) {
    try {
      const response = await fetch(`http://127.0.0.1:${port}/json/version`)
      if (response.ok) {
        const version = await response.json()
        if (version.webSocketDebuggerUrl) return version.webSocketDebuggerUrl
      }
    } catch {
      // DevTools HTTP endpoint is not listening yet
    }
    await sleep(POLL_INTERVAL_MS)
  }
  fail(`DevTools HTTP endpoint on port ${port} did not answer within ${CHROMIUM_STARTUP_TIMEOUT_MS}ms`)
}

function openWebSocket(url) {
  return new Promise((resolve, reject) => {
    const webSocket = new WebSocket(url)
    webSocket.addEventListener('open', () => resolve(webSocket), { once: true })
    webSocket.addEventListener('error', () => reject(new Error(`cannot connect to ${url}`)), { once: true })
  })
}

function createCdpClient(webSocket) {
  let nextId = 0
  const pendingCommands = new Map()
  const eventHandlers = new Set()

  const rejectAll = reason => {
    for (const pending of pendingCommands.values()) {
      clearTimeout(pending.timer)
      pending.reject(new Error(reason))
    }
    pendingCommands.clear()
  }

  webSocket.addEventListener('message', event => {
    const message = JSON.parse(event.data)
    if (typeof message.id === 'number') {
      const pending = pendingCommands.get(message.id)
      if (!pending) return
      pendingCommands.delete(message.id)
      clearTimeout(pending.timer)
      if (message.error) pending.reject(new Error(`${pending.method}: ${message.error.message}`))
      else pending.resolve(message.result)
      return
    }
    if (message.method) {
      for (const handler of eventHandlers) handler(message)
    }
  })
  webSocket.addEventListener('close', () => rejectAll('browser connection closed'))
  webSocket.addEventListener('error', () => rejectAll('browser connection error'))

  return {
    send(method, params = {}, sessionId) {
      nextId += 1
      const id = nextId
      const payload = sessionId ? { id, method, params, sessionId } : { id, method, params }
      return new Promise((resolve, reject) => {
        const timer = setTimeout(() => {
          pendingCommands.delete(id)
          reject(new Error(`${method}: command timed out after ${COMMAND_TIMEOUT_MS}ms`))
        }, COMMAND_TIMEOUT_MS)
        pendingCommands.set(id, { resolve, reject, method, timer })
        webSocket.send(JSON.stringify(payload))
      })
    },
    subscribe(handler) {
      eventHandlers.add(handler)
      return () => eventHandlers.delete(handler)
    },
    rejectAll
  }
}

function waitForPageLoad(client, sessionId, timeoutMs) {
  let timer = null
  let unsubscribe = null
  const result = new Promise((resolve, reject) => {
    timer = setTimeout(() => {
      unsubscribe?.()
      reject(new Error(`navigation did not fire a load event within ${timeoutMs}ms`))
    }, timeoutMs)
    unsubscribe = client.subscribe(message => {
      if (message.method !== 'Page.loadEventFired' || message.sessionId !== sessionId) return
      clearTimeout(timer)
      unsubscribe()
      resolve()
    })
  })
  result.catch(() => {})
  const cancel = () => {
    clearTimeout(timer)
    unsubscribe?.()
  }
  return { result, cancel }
}

async function waitForSignal(client, sessionId, expression, description, timeoutMs) {
  const deadline = Date.now() + timeoutMs
  let lastError = null
  while (Date.now() < deadline) {
    try {
      const evaluation = await client.send('Runtime.evaluate', { expression, returnByValue: true }, sessionId)
      if (evaluation.result?.value === true) return
      lastError = null
    } catch (error) {
      lastError = error
    }
    await sleep(POLL_INTERVAL_MS)
  }
  const detail = lastError ? ` (last error: ${lastError.message})` : ''
  fail(`timed out after ${timeoutMs}ms waiting for ${description}${detail}`)
}

function containsTextExpression(text) {
  return `document.body ? document.body.innerText.toLowerCase().includes(${JSON.stringify(text.toLowerCase())}) : false`
}

function axProperty(node, propertyName) {
  const property = node.properties?.find(entry => entry.name === propertyName)
  return property?.value?.value
}

function formatElement(node) {
  const role = node.role?.value
  if (!role || !INTERESTING_ROLES.has(role)) return null
  if (node.ignored === true) return null
  if (axProperty(node, 'hidden') === true) return null
  const name = (node.name?.value ?? '').replace(/\s+/g, ' ').trim()
  if (!name) return null

  const details = []
  const level = axProperty(node, 'level')
  if (level !== undefined && level !== null) details.push(`level=${level}`)
  if (axProperty(node, 'disabled') === true) details.push('disabled')
  const checked = axProperty(node, 'checked')
  if (checked === true || checked === 'mixed') details.push(`checked=${checked}`)
  if (axProperty(node, 'selected') === true) details.push('selected')
  if (axProperty(node, 'expanded') === true) details.push('expanded')

  const detailText = details.length > 0 ? ` (${details.join(', ')})` : ''
  return `- [${role}] ${JSON.stringify(name)}${detailText}`
}

async function closeBrowser(client) {
  if (!client) return
  try {
    await client.send('Browser.close')
  } catch {
    client.rejectAll('browser closed')
  }
}

function waitForChildExit(child, timeoutMs) {
  if (child.exitCode !== null || child.signalCode !== null) return Promise.resolve()
  return new Promise(resolve => {
    const timer = setTimeout(resolve, timeoutMs)
    child.once('exit', () => {
      clearTimeout(timer)
      resolve()
    })
  })
}

async function stopChromium(browser) {
  const child = browser.child
  if (child.exitCode !== null || child.signalCode !== null) return
  const exitPromise = waitForChildExit(child, 5000)
  const forceKill = setTimeout(() => child.kill(), 3000)
  await exitPromise
  clearTimeout(forceKill)
}

function removeProfileDirectory(profileDirectory) {
  try {
    rmSync(profileDirectory, { recursive: true, force: true, maxRetries: 5, retryDelay: 300 })
  } catch (error) {
    console.error(`profile cleanup failed (leftover temp dir is harmless): ${error.message}`)
  }
}

async function main() {
  const options = parseArguments(process.argv.slice(2))
  if (options.help || !options.url) {
    console.log(USAGE)
    if (!options.help) process.exitCode = 1
    return
  }
  if (typeof WebSocket === 'undefined') {
    fail('built-in WebSocket is missing — Node 22+ is required')
  }

  const statePath = resolveStatePath(options)
  const executablePath = findChromiumExecutable()
  const profileDirectory = mkdtempSync(join(tmpdir(), 'locator-dump-'))
  console.error(`chromium: ${executablePath}`)
  console.error(statePath ? `state: ${statePath}` : 'state: anonymous')

  const browser = launchChromium(executablePath, profileDirectory, options.headed)
  let client = null
  try {
    const port = await waitForDevToolsPort(profileDirectory, browser)
    const browserWebSocketUrl = await fetchBrowserWebSocketUrl(port)
    const webSocket = await openWebSocket(browserWebSocketUrl)
    client = createCdpClient(webSocket)

    const { targetId } = await client.send('Target.createTarget', { url: 'about:blank' })
    const { sessionId } = await client.send('Target.attachToTarget', { targetId, flatten: true })
    await client.send('Page.enable', {}, sessionId)

    if (statePath) {
      const cookies = loadStorageStateCookies(statePath)
      if (cookies.length > 0) await client.send('Network.setCookies', { cookies }, sessionId)
    }

    console.error(`navigating: ${options.url}`)
    const pageLoad = waitForPageLoad(client, sessionId, options.timeoutMs)
    const navigation = await client.send('Page.navigate', { url: options.url }, sessionId)
    if (navigation.errorText) {
      pageLoad.cancel()
      fail(`navigation failed: ${navigation.errorText}`)
    }
    await pageLoad.result
    if (options.waitFor) {
      await waitForSignal(
        client,
        sessionId,
        containsTextExpression(options.waitFor),
        `text "${options.waitFor}"`,
        options.timeoutMs)
    }

    const { nodes } = await client.send('Accessibility.getFullAXTree', {}, sessionId)
    const lines = nodes.map(formatElement).filter(line => line !== null)
    process.stdout.write(`${lines.join('\n')}\n`)
    console.error(`dumped ${lines.length} elements`)
  } finally {
    await closeBrowser(client)
    await stopChromium(browser)
    removeProfileDirectory(profileDirectory)
  }
}

main().catch(error => {
  console.error(error instanceof Error ? error.message : String(error))
  process.exitCode = 1
})
