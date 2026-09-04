# Skill: Review PR

Automated code review for Gredja pull requests. Checks against project rules + general quality.

## Usage

```
/review-pr <PR-number>
```

Example: `/review-pr 15`

## Prerequisites

- GitHub PAT with `repo` scope in `.env` as `GITHUB_PAT=github_pat_...`
- PowerShell (Invoke-RestMethod)

## How it works

### Step 1: Fetch PR metadata

```powershell
$token = (Get-Content ".env" | Select-String "GITHUB_PAT=(.*)" ).Matches[0].Groups[1].Value
$headers = @{ "Authorization" = "token $token"; "Accept" = "application/vnd.github.v3+json" }
$pr = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/<NUMBER>" -Headers $headers
```

### Step 2: Fetch changed files + patches

```powershell
$files = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/<NUMBER>/files" -Headers $headers
# Each file has: filename, status, additions, deletions, patch
```

### Step 3: Read current versions of changed files

```powershell
# For each file with status "modified" or "added":
$content = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/contents/<PATH>?ref=<HEAD_SHA>" -Headers $headers
$decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($content.content))
```

### Step 4: Review against checklist (below)

Analyze each changed file against the review checklist.

### Step 5: Post review comment

```powershell
$body = @{
    body = "<review markdown>"
} | ConvertTo-Json
Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/issues/<NUMBER>/comments" -Method Post -Headers $headers -Body $body
```

## Review checklist

### Gredja-specific rules

- [ ] **Models:** Response models have `Model` suffix + `Id` field. Request models have `Request` suffix, no `Id`
- [ ] **Model properties:** Reference types — no `?`, no init. Value types — `?` only if JSON field can be null/absent
- [ ] **Assertions:** FluentAssertions only (`Should().Be()`, not `Assert.That()`)
- [ ] **No `out _`** inside `OnlyContain` lambdas
- [ ] **Comments:** No comments unless regex pattern or non-obvious WHY
- [ ] **Naming:** PascalCase classes/methods/props, camelCase locals, `_camelCase` private fields, no abbreviations
- [ ] **Methods:** Max ~30 lines, one responsibility, max 3-4 params
- [ ] **No magic numbers/strings** — extract to constants
- [ ] **Config:** Endpoints in `Core/Config/Endpoints.cs`, never hardcoded in tests
- [ ] **Test base:** API tests inherit `ApiTestBase`
- [ ] **Non-existent IDs:** Dynamic approach (GET all → maxId + 1), not static 999

### General quality

- [ ] **Security:** No hardcoded secrets, tokens, or connection strings
- [ ] **No secrets in code:** `.env` not committed, no API keys in source
- [ ] **No dead code:** No commented-out blocks, no unused `using` statements
- [ ] **No duplicate code:** Extract shared logic to helpers
- [ ] **File structure:** One class per file, file name matches class name
- [ ] **Namespaces:** `Core.Models` for regular, `Core.Models.Generic` for reusable generics
- [ ] **Build:** `dotnet build` passes (if buildable from diff)
- [ ] **Tests:** New code has corresponding test coverage

## Output format

Post a PR review comment with:
- **Summary:** 1-2 sentence overview of changes
- **Verdict:** APPROVE / REQUEST_CHANGES / COMMENT
- **Issues:** Numbered list with file:line references and severity (blocking / suggestion / nit)
- **Positives:** What was done well (brief)

## Severity levels

- **blocking** — Must fix before merge (security, rule violation, broken code)
- **suggestion** — Should consider (code quality, naming, best practices)
- **nit** — Optional improvement (style, minor cleanup)
