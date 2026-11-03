import { createInterface } from 'node:readline';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const SERVER_NAME = 'slack-local';
const SERVER_VERSION = '1.0.0';
const SLACK_API = 'https://slack.com/api';

const TOOLS = [
  {
    name: 'post_message',
    description:
      'Post a message to a Slack channel the bot is a member of. ' +
      'Omit channel to use the SLACK_CHANNEL default from the repository .env.',
    inputSchema: {
      type: 'object',
      properties: {
        channel: {
          type: 'string',
          description: 'Channel ID (C...) or name (#general); defaults to SLACK_CHANNEL',
        },
        text: { type: 'string', description: 'Message text; mrkdwn is supported' },
      },
      required: ['text'],
    },
  },
  {
    name: 'list_channels',
    description: 'List Slack channels visible to the bot (id and name).',
    inputSchema: {
      type: 'object',
      properties: {
        limit: {
          type: 'integer',
          description: 'Maximum number of channels to return (1-200)',
          minimum: 1,
          maximum: 200,
        },
      },
      required: [],
    },
  },
];

function readEnvFile() {
  const root = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
  const values = {};
  try {
    for (const line of readFileSync(join(root, '.env'), 'utf8').split(/\r?\n/)) {
      const match = line.match(/^([A-Z0-9_]+)=(.*)$/);
      if (match) values[match[1]] = match[2].trim();
    }
  } catch {
    // missing .env is reported by the callers that need a value
  }
  return values;
}

const env = readEnvFile();

function send(payload) {
  process.stdout.write(`${JSON.stringify(payload)}\n`);
}

function reply(id, result) {
  send({ jsonrpc: '2.0', id, result });
}

function replyError(id, code, message) {
  send({ jsonrpc: '2.0', id, error: { code, message } });
}

function toolText(text, isError = false) {
  const result = { content: [{ type: 'text', text }] };
  if (isError) result.isError = true;
  return result;
}

async function slackFetch(path, options = {}) {
  const token = env.SLACK_BOT_TOKEN;
  if (!token) {
    throw new Error('SLACK_BOT_TOKEN is missing in the repository .env — add it and restart the session');
  }
  const response = await fetch(`${SLACK_API}${path}`, {
    ...options,
    headers: {
      Authorization: `Bearer ${token}`,
      ...(options.body ? { 'Content-Type': 'application/json; charset=utf-8' } : {}),
      ...(options.headers ?? {}),
    },
  });
  const payload = await response.json();
  if (!response.ok || payload.ok === false) {
    const slackError = payload.error ?? `HTTP ${response.status}`;
    const needed = payload.needed ?? payload.details?.needed;
    const provided = payload.provided ?? payload.details?.provided;
    const details = needed ? ` (needed: ${needed}; provided: ${provided ?? 'none'})` : '';
    throw new Error(`Slack API ${path} failed: ${slackError}${details}`);
  }
  return payload;
}

async function callTool(name, args) {
  if (name === 'post_message') {
    const channel = args.channel ?? env.SLACK_CHANNEL;
    if (!channel) {
      throw new Error('No channel given and no SLACK_CHANNEL default in .env');
    }
    const payload = await slackFetch('/chat.postMessage', {
      method: 'POST',
      body: JSON.stringify({ channel, text: args.text }),
    });
    return toolText(`Message posted to ${payload.channel} at ts ${payload.ts}`);
  }

  if (name === 'list_channels') {
    const limit = args.limit ?? 50;
    const payload = await slackFetch(
      `/conversations.list?types=public_channel&exclude_archived=true&limit=${limit}`,
    );
    const lines = payload.channels.map((channel) => `${channel.id}\t${channel.name}`);
    return toolText(lines.length > 0 ? lines.join('\n') : 'No channels visible to the bot');
  }

  throw new Error(`Unknown tool: ${name}`);
}

async function handle(message) {
  const { id, method, params } = message;

  if (id === undefined) return;

  if (method === 'initialize') {
    reply(id, {
      protocolVersion: params?.protocolVersion ?? '2024-11-05',
      capabilities: { tools: {} },
      serverInfo: { name: SERVER_NAME, version: SERVER_VERSION },
    });
    return;
  }

  if (method === 'ping') {
    reply(id, {});
    return;
  }

  if (method === 'tools/list') {
    reply(id, { tools: TOOLS });
    return;
  }

  if (method === 'tools/call') {
    try {
      reply(id, await callTool(params?.name, params?.arguments ?? {}));
    } catch (error) {
      reply(id, toolText(String(error.message ?? error), true));
    }
    return;
  }

  replyError(id, -32601, `Method not found: ${method}`);
}

process.stderr.write(`${SERVER_NAME} ${SERVER_VERSION} listening on stdio\n`);

const input = createInterface({ input: process.stdin, terminal: false });
input.on('line', (line) => {
  const trimmed = line.trim();
  if (!trimmed) return;
  let message;
  try {
    message = JSON.parse(trimmed);
  } catch {
    replyError(null, -32700, 'Parse error');
    return;
  }
  handle(message).catch((error) => {
    if (message.id !== undefined) replyError(message.id, -32603, String(error));
  });
});
