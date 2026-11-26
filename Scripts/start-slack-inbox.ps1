# Starts the Slack → live-session pipeline: secrets, mimo serve (port from config),
# slack-inbox poller (auto newest-main-session mode). Idempotent — reuses a running
# listener/poller, so Task Scheduler at logon and a manual run are the same path.
# Usage: Scripts/start-slack-inbox.ps1   (no parameters; logs go to %TEMP%)
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    & (Join-Path $repoRoot 'Scripts\sync-secrets.ps1') | Out-Null

    $config = Get-Content (Join-Path $repoRoot 'Scripts\mcp-slack\config.json') -Raw | ConvertFrom-Json
    $port = [int]$config.servePort

    if (-not (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)) {
        $mimo = Get-Command 'mimo.cmd' -CommandType Application -ErrorAction SilentlyContinue
        if ($null -eq $mimo) { throw 'mimo.cmd not found on PATH — install the MiMoCode CLI' }
        Start-Process -FilePath $mimo.Source `
            -ArgumentList 'serve', '--port', "$port" `
            -WorkingDirectory $repoRoot `
            -WindowStyle Hidden `
            -RedirectStandardOutput (Join-Path $env:TEMP 'mimo-serve.log') `
            -RedirectStandardError (Join-Path $env:TEMP 'mimo-serve.err')

        $deadline = (Get-Date).AddSeconds(30)
        while ((Get-Date) -lt $deadline -and
            -not (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)) {
            Start-Sleep -Milliseconds 500
        }
        if (-not (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)) {
            throw "mimo serve did not listen on port $port within 30s — see %TEMP%\mimo-serve.err"
        }
    }

    $poller = Get-CimInstance Win32_Process -Filter "Name='Tools.exe'" |
        Where-Object { $_.CommandLine -match 'slack-inbox' }
    if ($null -eq $poller) {
        # Dedicated output dir: bin\Debug may be locked by the live serve-slack MCP process
        $buildOutput = Join-Path $env:TEMP 'gredja-slack-inbox-build\'
        dotnet build (Join-Path $repoRoot 'Scripts\Tools\Tools.csproj') `
            -v q --nologo "-p:BaseOutputPath=$buildOutput" | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Tools build failed (exit $LASTEXITCODE)" }

        Start-Process -FilePath (Join-Path $buildOutput 'Debug\net11.0\Tools.exe') `
            -ArgumentList 'slack-inbox' `
            -RedirectStandardError (Join-Path $env:TEMP 'slack-inbox.log') `
            -WindowStyle Hidden
    }
}
finally {
    Pop-Location
}
