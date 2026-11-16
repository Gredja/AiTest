# Reads GITHUB_PAT from the process environment and returns it to the caller.
#
# Contract (single source of truth for every skill/script):
#   capture the output directly into a variable — the token must NEVER be printed,
#   logged, echoed, or embedded into messages, reports, or commit bodies.

$token = $env:GITHUB_PAT

if ([string]::IsNullOrEmpty($token)) {
    throw "GITHUB_PAT environment variable is not set (user environment variable, inherited by new shells)"
}

$token
