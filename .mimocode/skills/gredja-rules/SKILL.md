---
name: gredja-rules
description: Use when the user asks about Gredja project rules, coding standards, or conventions. Entry point for all Gredja project rules.
---

# Skill: Gredja Rules

Entry point for all Gredja project rules. Overview + pointers to detailed rule files.

## Project

Gredja — .NET 10.0 test automation solution (NUnit, RestSharp, Playwright). FakeStoreAPI for API testing.

- Repo: https://github.com/Gredja/AiTest.git
- Branch: `main`, changes in `features/<topic>`
- Test base class: `ApiTestBase` in `Api/ApiTestBase.cs`
- Endpoints: `Core/Config/Endpoints.cs`

## Rule Files

Read the relevant file before working on the corresponding entity:

| File | Covers |
|------|--------|
| `Rules/models.md` | Model/Request naming, property rules, namespaces |
| `Rules/assertions.md` | FluentAssertions patterns, OnlyContain gotchas |
| `Rules/code.md` | Naming, types, methods, general code style |
| `Rules/comments.md` | When comments are needed (regex, non-obvious WHY only) |
| `Rules/git.md` | Remote, branches, commits, secrets |
| `Rules/workflow.md` | Plan → approval → changes → report |
| `Rules/config.md` | Endpoints, no hardcoded URLs |

## Key Rules (quick reference)

- **FluentAssertions only** — no `Assert.That()`
- **Models:** `Model` suffix (response), `Request` suffix (request), `Id` always non-nullable
- **Reference types:** no `?`, no init. **Value types:** `?` if JSON field can be null
- **No comments** unless regex pattern or non-obvious WHY
- **No magic numbers** — extract to constants
- **Non-existent IDs:** dynamic (GET all → maxId + 1), never static 999
- **FakeStoreAPI:** returns 200 OK for non-existent IDs — mark tests with `[Ignore]`
- **Commits:** only on user request, English, action + object format
