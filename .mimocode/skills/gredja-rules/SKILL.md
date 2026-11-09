---
name: gredja-rules
description: Use when the user asks about Gredja project rules, coding standards, or conventions. Entry point for all Gredja project rules.
---

# Skill: Gredja Rules

Entry point for all Gredja project rules. Overview + pointers to detailed rule files.

## Project

Gredja — .NET 10.0 test automation solution (NUnit, RestSharp, Playwright). Services: JSONPlaceholder (mock API), GitHub API, Moodle demo (`https://school.moodledemo.net`, localization testing).

- Repo: https://github.com/Gredja/AiTest.git
- Branch: `main`, changes in `features/<topic>`
- Test projects: `Api/` (API tests), `E2E/` (scenario chains), `Ui/github/` + `Ui/moodledemo/` (Playwright UI)
- Test base classes: `JsonPlaceholderRequestHelper`, `GitHubRequestHelper`, `GitHubTestBase` (Core/Helpers); `GitHubE2ETestBase` (E2E/GitHub); `GitHubUiTestBase` (Ui/github/Tests/Support)
- Endpoints: `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs`, `Core/Config/MoodledemoEndpoints.cs`

## Rule Files

Read the relevant file before working on the corresponding entity:

| File | Covers |
|------|--------|
| `Rules/models.md` | Model/Request naming, property rules, namespaces |
| `Rules/assertions.md` | FluentAssertions patterns, OnlyContain gotchas |
| `Rules/code.md` | Naming, types, methods, general code style |
| `Rules/code-style.md` | Error handling, LINQ, strings, null safety, magic strings |
| `Rules/code-principles.md` | SOLID, general principles |
| `Rules/test-practices.md` | Isolation, seed methodology, cleanup, non-existent IDs |
| `Rules/categories.md` | Category-to-test mapping, check-type boundaries |
| `Rules/ui-testing.md` | Playwright UI tests: locators, storageState, cleanup |
| `Rules/localization-testing.md` | Localization testing (Moodle demo): locale locators, contract, HTTP-first L1 |
| `Rules/comments.md` | When comments are needed (regex, non-obvious WHY only) |
| `Rules/git.md` | Remote, branches, commits, secrets |
| `Rules/workflow.md` | Plan → approval → changes → report |
| `Rules/config.md` | Endpoints, no hardcoded URLs |

## Key Rules (quick reference)

- **FluentAssertions only** — no `Assert.That()`
- **Models:** `ModelResponse` suffix (response), `ModelRequest` suffix (request), nested/supporting models without suffix; value types `?` only if JSON field can be null/absent
- **Lambda parameters:** readable singular noun (`product => product.Id`), never single-letter
- **Reference types:** no `?`, no init. **Value types:** `?` if JSON field can be null
- **No comments** unless regex pattern or non-obvious WHY
- **No magic numbers** — extract to constants
- **Non-existent IDs:** dynamic (GET all → maxId + 1; concurrent E2E writes → maxId + Offset), never static 999
- **Categories:** `[Category("{Service}")]` on the class, check-type category on every test method — see `Rules/categories.md`
- **Localization (Moodle demo):** locale contract, HTTP-first L1, language-agnostic locators — see `Rules/localization-testing.md`
- **Commits:** only on user request, English, action + object format

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-11-08 | S5: Project section refreshed (Moodle demo service, moodledemo endpoints file, test projects, E2E/Ui base classes); Key Rules gained categories + localization pointers. Revision History added | MiMo |
