---
name: review-pr
description: Use when the user says "review-pr", "/review-pr", or wants to review a pull request. Automated code review for Gredja pull requests.
---

# Review PR — Gredja

All steps executed directly by the main agent. No subagent.

## Step 1: Gather PR info

Ask user for PR number if not provided.

Verify prerequisites:
- GitHub PAT with `repo` scope in `.env` as `GITHUB_PAT=github_pat_...`
- PowerShell (Invoke-RestMethod)

## Step 2: Fetch PR data

```powershell
$token = (Get-Content ".env" | Select-String "GITHUB_PAT=(.*)" ).Matches[0].Groups[1].Value
$headers = @{ "Authorization" = "token $token"; "Accept" = "application/vnd.github.v3+json" }
$pr = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/{pr_number}" -Headers $headers
$files = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/{pr_number}/files" -Headers $headers
```

For each changed file (modified/added), fetch content:
```powershell
$content = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/contents/<PATH>?ref=<HEAD_SHA>" -Headers $headers
$decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($content.content))
```

## Step 3: Read rules

Read `Rules/*.md` for full project rules context.

## Step 4: Review each changed file

Against checklist:

**Gredja-specific:**
- Models: Model/Request suffix, Id field, property types
- Assertions: FluentAssertions only
- Comments: no comments unless regex or non-obvious WHY
- Naming: PascalCase/camelCase/_camelCase, no abbreviations
- Methods: max ~30 lines, one responsibility
- No magic numbers/strings
- Config: Endpoints in `Core/Config/Endpoints.cs`
- Non-existent IDs: dynamic (GET all → maxId + 1), not static 999

**General:**
- No hardcoded secrets/tokens
- No dead code, no duplicate code
- File structure: one class per file
- Build passes (if buildable from diff)
- New code has test coverage

## Step 5: Post review comment

```powershell
$body = @{ body = "<review markdown>" } | ConvertTo-Json
Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/issues/{pr_number}/comments" -Method Post -Headers $headers -Body $body
```

## Step 6: Report

- PR title and author
- Summary (1-2 sentences)
- Verdict: APPROVE / REQUEST_CHANGES / COMMENT
- Issues: numbered list with file:line and severity (blocking / suggestion / nit)
- Positives (brief)

---

## Rules

- Never skip code review
- Severity levels: blocking (must fix) / suggestion (should consider) / nit (optional)
- If GitHub PAT is missing — report and stop
