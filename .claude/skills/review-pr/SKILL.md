# Skill: Review PR

Automated code review for Gredja pull requests. Checks against project rules + general quality.

## Usage

```
/review-pr <PR-number-or-URL>
```

Example: `/review-pr 15` or `/review-pr https://github.com/Gredja/AiTest/pull/15`

## What it does

1. Fetches PR diff via `gh pr diff <number>`
2. Reviews changes against checklist below
3. Posts review comment via `gh pr review <number>`
4. Reports summary to user

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

Post a PR review with:
- **Summary:** 1-2 sentence overview of changes
- **Verdict:** APPROVE / REQUEST_CHANGES / COMMENT
- **Issues:** Numbered list with file:line references and severity (🔴 blocking / 🟡 suggestion / 🟢 nit)
- **Positives:** What was done well (brief)

## Severity levels

- 🔴 **blocking** — Must fix before merge (security, rule violation, broken code)
- 🟡 **suggestion** — Should consider (code quality, naming, best practices)
- 🟢 **nit** — Optional improvement (style, minor cleanup)
