# Pulls project secrets from Infisical into the CURRENT PowerShell process (in memory).
# Nothing is persisted anywhere: no HKCU\Environment, no setx, no files — plaintext
# exists only while this process (and its child processes) live.
#
# Contract: secret VALUES are never printed, logged, or written to files —
# the output lists key names and counts only (same rule as read-github-pat.ps1).
#
# Usage — run it in the SAME shell/session as the command that needs the secrets:
#   Scripts/sync-secrets.ps1; dotnet test --filter Category=HealthCheck
# Every new shell/session must run it again (there is no local cache by design).
# Prerequisites: `npm install -g @infisical/cli` and `infisical login` (once per machine),
# the repo-linked `.infisical.yaml`.

[CmdletBinding()]
param(
    [string]$Environment = 'dev'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

# The npm infisical.ps1 shim can exit 0 with empty output under some hosts —
# prefer the native binary, then the Application-type shim (infisical.cmd).
$cliPath = $null
$npmRoot = & npm root -g 2>$null | Select-Object -First 1
if (-not [string]::IsNullOrWhiteSpace($npmRoot)) {
    $exePath = Join-Path $npmRoot '@infisical\cli\bin\infisical.exe'
    if (Test-Path $exePath) {
        $cliPath = $exePath
    }
}
if ($null -eq $cliPath) {
    $cliCommand = Get-Command infisical -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -ne $cliCommand) {
        $cliPath = $cliCommand.Source
    }
    else {
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

$requiredKeys = @(
    'GITHUB_PAT',
    'GITHUB_UI_EMAIL',
    'GITHUB_UI_PASSWORD',
    'GITHUB_UI_PAT',
    'SLACK_BOT_TOKEN'
)

$byKey = @{}
foreach ($secret in $secrets) {
    if (-not [string]::IsNullOrWhiteSpace($secret.key) -and -not [string]::IsNullOrEmpty($secret.value)) {
        $byKey[$secret.key] = $secret.value
    }
}

# Validate before applying anything — a partial environment is worse than none.
$missing = @($requiredKeys | Where-Object { -not $byKey.ContainsKey($_) })
if ($missing.Count -gt 0) {
    throw "Infisical [$Environment] is missing required keys: $($missing -join ', ') — environment left untouched"
}

foreach ($key in $requiredKeys) {
    Set-Item -Path "Env:$key" -Value $byKey[$key]
}
$extra = @($byKey.Keys | Where-Object { $requiredKeys -notcontains $_ })
foreach ($key in $extra) {
    Set-Item -Path "Env:$key" -Value $byKey[$key]
}

Write-Output "loaded $($requiredKeys.Count) required keys (+$($extra.Count) extra) from Infisical [$Environment] into process memory: $(($requiredKeys + $extra) -join ', ')"
Write-Output 'Not persisted anywhere — run this script again in every new shell/session that needs the secrets.'
