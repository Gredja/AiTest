# Spawn target for the slack-local MCP server (mimocode.jsonc → this script).
# Builds Tools into a FRESH %TEMP% directory every time (a running instance locks its own
# output — rebuilding into bin\Debug fails MSB3027/MSB3021), then runs the dll directly:
# stdout = MCP protocol only, everything else (sync-secrets, build) goes to stderr.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    & (Join-Path $repoRoot 'Scripts\sync-secrets.ps1') | Out-Null

    $buildRoot = Join-Path $env:TEMP 'gredja-serve-slack'
    New-Item -ItemType Directory -Force -Path $buildRoot | Out-Null

    # Reap previous builds; ones still locked by a live instance fail silently and
    # are reaped on the next spawn after that instance exits.
    Get-ChildItem $buildRoot -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

    $buildOutput = Join-Path $buildRoot ('build-' + (Get-Date -Format 'yyyyMMdd-HHmmssfff'))
    # PS 5.1 has no 1>&2 — stream build output to stderr line by line (stdout stays protocol-only)
    dotnet build Scripts/Tools "-p:BaseOutputPath=$buildOutput\" -nologo -v q -clp:ErrorsOnly |
        ForEach-Object { [Console]::Error.WriteLine($_) }
    if ($LASTEXITCODE -ne 0) {
        Write-Output "spawn-serve-slack: build failed (exit $LASTEXITCODE)"
        exit 1
    }

    $dll = Get-ChildItem $buildOutput -Recurse -Filter 'Tools.dll' | Select-Object -First 1
    if ($null -eq $dll) {
        Write-Output "spawn-serve-slack: Tools.dll not found under $buildOutput"
        exit 1
    }

    & dotnet $dll.FullName serve-slack
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
