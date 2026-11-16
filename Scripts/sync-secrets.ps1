# Pulls project secrets from Infisical into the user environment of this machine:
# process env (current session) + HKCU:\Environment via setx (new shells inherit).
#
# Contract: secret VALUES are never printed, logged, or written to files —
# the output lists key names and counts only (same rule as read-github-pat.ps1).
#
# Usage: Scripts/sync-secrets.ps1 [-Environment dev]
# Prerequisites: `npm install -g @infisical/cli` and `infisical login` (once per machine),
# the repo-linked `.infisical.yaml`. After a sync, restart open terminals and the agent
# session so new processes pick up the variables.

[CmdletBinding()]
param(
    [string]$Environment = 'dev'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

$cliCommand = Get-Command infisical -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
if ($null -ne $cliCommand) {
    $cliPath = $cliCommand.Source
}
else {
    $npmRoot = & npm root -g 2>$null | Select-Object -First 1
    $cliPath = if (-not [string]::IsNullOrWhiteSpace($npmRoot)) {
        Join-Path $npmRoot '@infisical\cli\bin\infisical.exe'
    }
    else {
        [string]::Empty
    }

    if ([string]::IsNullOrEmpty($cliPath) -or -not (Test-Path $cliPath)) {
        throw 'Infisical CLI not found — install it: npm install -g @infisical/cli'
    }
}

Push-Location $repoRoot
try {
    $json = & $cliPath secrets-manager secrets export --format json --env $Environment --silent | Out-String
    $exportExitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

if ($exportExitCode -ne 0) {
    throw "Infisical export failed (exit $exportExitCode) — run 'infisical login status' and check .infisical.yaml"
}

try {
    $secrets = $json | ConvertFrom-Json
}
catch {
    throw 'Infisical export returned unparsable output — environment left untouched'
}

$synced = [System.Collections.Generic.List[string]]::new()
$skipped = [System.Collections.Generic.List[string]]::new()

foreach ($secret in $secrets) {
    $key = $secret.key
    $value = $secret.value

    if ([string]::IsNullOrWhiteSpace($key)) {
        continue
    }

    if ([string]::IsNullOrEmpty($value)) {
        $skipped.Add($key)
        continue
    }

    Set-Item -Path "Env:$key" -Value $value
    & setx $key $value 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $synced.Add($key)
    }
    else {
        $skipped.Add($key)
    }
}

if ($synced.Count -eq 0) {
    throw "No secrets synced from Infisical [$Environment] (skipped: $($skipped -join ', '))"
}

Write-Output "synced $($synced.Count) keys from Infisical [$Environment]: $($synced -join ', ')"
if ($skipped.Count -gt 0) {
    Write-Warning "skipped (empty value or setx failure): $($skipped -join ', ')"
}
Write-Output 'Restart open terminals and the agent session so new processes inherit the variables.'
