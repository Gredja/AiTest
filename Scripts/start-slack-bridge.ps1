param(
    [Parameter(Mandatory = $true)]
    [string]$SessionId
)

# Raises the Slack -> session bridge for the current MiMoCode session:
# secrets (process memory) -> mimo serve (fixed port) -> slack-inbox poller.
# Best-effort by design: prints status, never throws to the caller.

$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $PSScriptRoot

if ($SessionId -notmatch '^ses_') {
    Write-Output "slack-bridge: usage: start-slack-bridge.ps1 <sessionID>"
    exit 2
}

& (Join-Path $PSScriptRoot 'sync-secrets.ps1') | Out-Null

$config = Get-Content (Join-Path $root 'Scripts\mcp-slack\config.json') -Raw | ConvertFrom-Json
$port = [int]$config.servePort
$serveLog = Join-Path $env:TEMP 'mimo-serve.log'
$serveErrorLog = Join-Path $env:TEMP 'mimo-serve.err.log'
$inboxLog = Join-Path $env:TEMP 'slack-inbox.err.log'
$inboxOutLog = Join-Path $env:TEMP 'slack-inbox.out.log'

$serveListener = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
if ($serveListener) {
    Write-Output "slack-bridge: serve already listening on $port"
}
else {
    $mimoCommand = @'
mimo serve --port PORT
'@.Replace('PORT', $port)
    Start-Process -FilePath 'powershell.exe' `
        -ArgumentList '-NoProfile', '-NonInteractive', '-Command', $mimoCommand `
        -WindowStyle Hidden `
        -RedirectStandardOutput $serveLog `
        -RedirectStandardError $serveErrorLog
    Start-Sleep -Seconds 3
    $serveListener = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($serveListener) {
        Write-Output "slack-bridge: serve started on $port"
    }
    else {
        Write-Output "slack-bridge: serve FAILED to listen on $port (see $serveErrorLog)"
    }
}

$toolsDll = Join-Path $root 'Scripts\Tools\bin\Debug\net11.0\Tools.dll'
$inboxProcess = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -match 'slack-inbox' -and $_.CommandLine -match [regex]::Escape($SessionId) }

if ($inboxProcess) {
    Write-Output "slack-bridge: slack-inbox already running (pid $($inboxProcess.ProcessId))"
}
elseif (-not (Test-Path $toolsDll)) {
    Write-Output "slack-bridge: $toolsDll not found — run 'dotnet build Scripts/Tools' once first"
    exit 1
}
else {
    Start-Process -FilePath 'dotnet' `
        -ArgumentList "`"$toolsDll`"", 'slack-inbox', $SessionId `
        -WindowStyle Hidden `
        -RedirectStandardOutput $inboxOutLog `
        -RedirectStandardError $inboxLog
    Start-Sleep -Seconds 2
    $inboxProcess = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -match 'slack-inbox' -and $_.CommandLine -match [regex]::Escape($SessionId) }
    if ($inboxProcess) {
        Write-Output "slack-bridge: slack-inbox started (pid $($inboxProcess.ProcessId)) → session $SessionId"
    }
    else {
        Write-Output "slack-bridge: slack-inbox FAILED to start (see $inboxLog)"
        exit 1
    }
}

exit 0
