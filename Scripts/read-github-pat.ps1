# Reads GITHUB_PAT from the repo-root .env and returns it to the caller.
#
# Contract (single source of truth for every skill/script):
#   capture the output directly into a variable — the token must NEVER be printed,
#   logged, echoed, or embedded into messages, reports, or commit bodies.

$envPath = Join-Path $PSScriptRoot '..\.env'
$line = Select-String -Path $envPath -Pattern '^GITHUB_PAT=' | Select-Object -First 1

if ($null -eq $line) {
    throw "GITHUB_PAT line not found in $envPath"
}

$line.Line -replace '^GITHUB_PAT=', ''
