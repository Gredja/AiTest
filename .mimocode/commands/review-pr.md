---
description: Review a pull request against project rules
argument-hint: [PR number]
---

# Review PR

Automated code review for Gredja pull requests. Checks against project rules + general quality.

## Step 1: Gather PR info

Ask user for PR number if not provided via `$ARGUMENTS`.

Verify prerequisites:
- GitHub PAT with `repo` scope in `.env` as `GITHUB_PAT=github_pat_...`
- PowerShell (Invoke-RestMethod)

## Step 2: Spawn subagent

Spawn a `general` subagent with this prompt:

```
You are a PR review subagent for Gredja. Execute the following steps precisely. Do NOT ask the user anything.

Working dir: {working_dir}
PR number: {pr_number}

## Steps

1. Fetch PR metadata:
   $token = (Get-Content ".env" | Select-String "GITHUB_PAT=(.*)" ).Matches[0].Groups[1].Value
   $headers = @{ "Authorization" = "token $token"; "Accept" = "application/vnd.github.v3+json" }
   $pr = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/{pr_number}" -Headers $headers

2. Fetch changed files + patches:
   $files = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/pulls/{pr_number}/files" -Headers $headers

3. Read current versions of changed files (for "modified" or "added"):
   $content = Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/contents/<PATH>?ref=<HEAD_SHA>" -Headers $headers
   $decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($content.content))

4. Read Rules/*.md for project rules context.

5. Review each changed file against checklist:
   Gredja-specific:
   - Models: Model/Request suffix, Id field, property types
   - Assertions: FluentAssertions only
   - Comments: no comments unless regex or non-obvious WHY
   - Naming: PascalCase/camelCase/_camelCase, no abbreviations
   - Methods: max ~30 lines, one responsibility
   - No magic numbers/strings
   - Config: Endpoints in Endpoints.cs
   - Non-existent IDs: dynamic (GET all → maxId + 1), not static 999
   General:
   - No hardcoded secrets/tokens
   - No dead code, no duplicate code
   - File structure: one class per file
   - Build passes (if buildable from diff)
   - New code has test coverage

6. Post review comment:
   $body = @{ body = "<review markdown>" } | ConvertTo-Json
   Invoke-RestMethod -Uri "https://api.github.com/repos/Gredja/AiTest/issues/{pr_number}/comments" -Method Post -Headers $headers -Body $body

7. Report:
   - PR title and author
   - Summary (1-2 sentences)
   - Verdict: APPROVE / REQUEST_CHANGES / COMMENT
   - Issues: numbered list with file:line and severity (blocking / suggestion / nit)
   - Positives (brief)
```

## Step 3: Deliver result

Report the subagent's output to the user.

---

## Rules

- Never skip code review
- Severity levels: blocking (must fix) / suggestion (should consider) / nit (optional)
- If GitHub PAT is missing — report and stop
