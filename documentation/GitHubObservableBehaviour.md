# Observable Behaviour — GitHub REST API v3

**Project:** Gredja (.NET 10.0)
**API Under Test:** GitHub REST API v3 (https://api.github.com)
**Date:** 2026-02-21
**Scope:** GET (Phase 1) + POST/PATCH/DELETE (Phase 2 E2E)
**Test Plan:** [GitHubTestPlan.md](GitHubTestPlan.md)

---

## What AI does

When generating or reviewing GitHub API tests, the AI agent:

1. **Reads this document first** — before any code generation or review, loads the relevant endpoint section
2. **Uses Positive bullets as assertions** — each `- Object has: \`field\` (type)` becomes a `.Should().Be()` / `.Should().NotBeNull()` / `.Should().BeGreaterThan()` call
3. **Uses Negative bullets as test cases** — each negative bullet = one `[Category("Negative")]` test method
4. **Uses Validation Rules for model generation** — Response Field Constraints map directly to C# model properties with attributes (`[PositiveId]`, `[RequiredField]`, `[ValueRange]`)
5. **Uses Request Body table for POST/PATCH tests** — required fields → mandatory assertions, optional fields → conditional assertions
6. **Uses State transitions for E2E chains** — e.g. "state: open → closed" maps to PATCH test that verifies `state` changed
7. **Uses Endpoint Priority for coverage ordering** — P0 first, P3 last when generating incrementally
8. **Never invents fields** — only asserts fields listed in this document; if a field is missing from the response, reports it as a discrepancy
9. **Uses Seed methodology** — for each endpoint: 5 seeds → expand to table with `# | Case | Category | Priority | Source seed` → enforce 5+ negatives → see `Rules/test-practices.md` → "Seed methodology"

| Priority | Endpoints | Rationale |
|---|---|---|
| **P0 — Core** | Issues (CRUD), Issue Comments (CRUD), PRs (CRUD + merge) | Ядро GitHub — то, ради чего люди используют платформу. Баги здесь критичны. |
| **P1 — Important** | Repos (read), Branches (CRUD), Rate Limit | Инфраструктура для Issues/PRs. Без репозиториев и веток E2E-цепочки не работают. |
| **P2 — Useful** | Contributors, Languages, Topics, Tags | Метаданные репозитория. Полезны для мониторинга и аналитики, но не блокируют основной workflow. |
| **P3 — Reference** | Users, User Repos, Auth Repos, Public Repos | Справочная информация. Критична для аутентификации и авторизации, но реже используется в E2E-тестах. |

---

## 1. GET /repos/{owner}/{repo}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `name` (string, matches {repo}), `owner` (object with `login` matching {owner}), `html_url` (url), `default_branch` (string), `visibility` (string), `stargazers_count` (integer ≥ 0), `forks_count` (integer ≥ 0), `open_issues_count` (integer ≥ 0)
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Non-existent owner → 404, body has `message` (string: "Not Found")
- Private repo without auth → 404 (not 403), body has `message` (string: "Not Found")

---

## 2. GET /repositories

- Response status is 200 OK
- Response body is a JSON array
- Array contains ≥ 1 public repository object
- Each object has: `name` (string), `owner` (object), `html_url` (url)
- Content-Type is application/json

**Pagination:**
- `?since=N` — returns repos created after ID N
- `?per_page=N` — array length ≤ N

**Negative:**
- `?since=999999999` → 200 OK, empty array `[]`
- `?per_page=0` → 200 OK, returns default page (30 items)
- `?per_page=101` → 200 OK, capped at 100 items

---

## 3. GET /users/{username}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `login` (string, matches {username}), `id` (integer > 0), `avatar_url` (url), `html_url` (url), `type` (string), `public_repos` (integer ≥ 0), `followers` (integer ≥ 0), `following` (integer ≥ 0)
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Non-existent user → 404, body has `message` (string: "Not Found"), `documentation_url` (url)

---

## 4. GET /users/{username}/repos

- Response status is 200 OK
- Response body is a JSON array
- Each object has: `name` (string), `owner` (object with `login` matching {username}), `html_url` (url)
- Content-Type is application/json

**Pagination:**
- `?per_page=N` — array length ≤ N
- `?page=N` — returns page N

**Negative:**
- Non-existent user → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- `?per_page=0` → 200 OK, returns default page (30 items)

---

## 5. GET /user/repos

- Response status is 200 OK (requires auth)
- Response body is a JSON array
- Array contains repositories the authenticated user has access to
- Each object has: `name` (string), `owner` (object), `html_url` (url)
- Content-Type is application/json

**Negative:**
- No auth header → 401, body has `message` (string: "Requires authentication"), `documentation_url` (url)
- Invalid token → 401, body has `message` (string: "Bad credentials"), `documentation_url` (url)
- Token with wrong scope → 403, body has `message` (string), `documentation_url` (url)

---

## 6. GET /repos/{owner}/{repo}/issues

- Response status is 200 OK
- Response body is a JSON array
- Each object has: `id` (integer), `title` (string), `state` (string: "open" | "closed"), `user` (object with `login`, `id`), `created_at` (datetime), `updated_at` (datetime), `labels` (array), `comments` (integer ≥ 0)
- Content-Type is application/json
- Response time < 5s

**Filtering:**
- `?state=open` — all items have `state` = "open"
- `?state=closed` — all items have `state` = "closed"
- `?per_page=N` — array length ≤ N

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Invalid `?state=invalid` → 200 OK, returns default (open + closed mixed)

---

## 7. GET /repos/{owner}/{repo}/issues/{number}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `id` (integer), `number` (integer, matches {number}), `title` (string), `state` (string: "open" | "closed"), `user` (object with `login`, `id`), `body` (string), `labels` (array), `comments` (integer ≥ 0), `created_at` (datetime), `updated_at` (datetime)
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Non-existent issue number → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Non-existent repo → 404, body has `message` (string: "Not Found")
- Issue number = 0 → 404, body has `message` (string: "Not Found")
- Negative issue number → 404, body has `message` (string: "Not Found")

---

## 8. GET /repos/{owner}/{repo}/issues/{number}/comments

- Response status is 200 OK
- Response body is a JSON array
- Each object has: `id` (integer), `body` (string), `user` (object with `login`, `id`), `created_at` (datetime), `updated_at` (datetime)
- Items ordered by `created_at` ascending
- Content-Type is application/json

**Pagination:**
- `?per_page=N` — array length ≤ N
- `?page=N` — returns page N

**Negative:**
- Non-existent issue → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Non-existent repo → 404, body has `message` (string: "Not Found")
- Issue with no comments → 200 OK, empty array `[]`

---

## 9. GET /repos/{owner}/{repo}/pulls

- Response status is 200 OK
- Response body is a JSON array
- Each object has: `id` (integer), `title` (string), `state` (string: "open" | "closed"), `user` (object with `login`, `id`), `head` (object with `ref`, `sha`), `base` (object with `ref`, `sha`), `created_at` (datetime), `updated_at` (datetime), `merged_at` (datetime or null)
- Content-Type is application/json
- Response time < 5s

**Filtering:**
- `?state=open` — all items have `state` = "open"
- `?state=closed` — all items have `state` = "closed"
- `?per_page=N` — array length ≤ N

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Invalid `?state=invalid` → 200 OK, returns default (open + closed mixed)
- Repo with no PRs → 200 OK, empty array `[]`

---

## 10. GET /repos/{owner}/{repo}/branches

- Response status is 200 OK
- Response body is a JSON array
- Array contains ≥ 1 branch (at least default branch)
- Each object has: `name` (string, non-empty), `commit` (object with `sha` string of 40 hex chars)
- Content-Type is application/json
- Response time < 5s

**Pagination:**
- `?per_page=N` — array length ≤ N

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)

---

## 11. GET /repos/{owner}/{repo}/branches/{branch}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `name` (string, matches {branch}), `commit` (object with `sha` string of 40 hex chars)
- Content-Type is application/json

**Negative:**
- Non-existent branch → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Non-existent repo → 404, body has `message` (string: "Not Found")

---

## 12. GET /repos/{owner}/{repo}/contributors

- Response status is 200 OK
- Response body is a JSON array
- Each object has: `login` (string), `id` (integer > 0), `contributions` (integer > 0)
- Items sorted by `contributions` descending
- Excludes bots and empty accounts
- Content-Type is application/json

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Repo with no contributors (empty) → 200 OK, empty array `[]`

---

## 13. GET /repos/{owner}/{repo}/languages

- Response status is 200 OK
- Response body is a JSON object
- Keys are language names (string), values are bytes of code (integer > 0)
- Non-empty repos have ≥ 1 language
- Content-Type is application/json

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Empty repo (no code) → 200 OK, empty object `{}`

---

## 14. GET /repos/{owner}/{repo}/topics

- Response status is 200 OK
- Response body is a JSON object with `names` (array of lowercase alphanumeric strings, may contain hyphens)
- Requires `Accept: application/vnd.github.mercy-preview+json` header for topics
- Content-Type is application/json

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Repo with no topics → 200 OK, `names` is empty array `[]`

---

## 15. GET /repos/{owner}/{repo}/tags

- Response status is 200 OK
- Response body is a JSON array
- Each object has: `name` (string, non-empty), `commit` (object with `sha` string of 40 hex chars)
- Content-Type is application/json

**Negative:**
- Non-existent repo → 404, body has `message` (string: "Not Found"), `documentation_url` (url)
- Repo with no tags → 200 OK, empty array `[]`

---

## 16. GET /rate_limit

- Response status is 200 OK
- Response body is a JSON object with `resources` and `rate` sections
- `resources.core` has: `limit` (integer), `remaining` (integer ≥ 0), `reset` (unix timestamp), `used` (integer)
- `resources.search` has same fields
- `rate.limit` is 5000 (auth) or 60 (no auth)
- `rate.reset` is a unix timestamp in the future
- Response headers include: `X-RateLimit-Remaining`, `X-RateLimit-Reset`, `X-RateLimit-Limit`
- Content-Type is application/json

**Negative:**
- Always returns 200 OK — no negative cases for this endpoint
- When `remaining` = 0, next requests (not this one) return 403

---

# WRITE OPERATIONS (Phase 2 — E2E)

All write operations target the sandbox repo `Gredja/AiTest`. Require `Authorization: Bearer <token>` with `repo` scope.

---

## 17. POST /repos/{owner}/{repo}/issues

- Request body: `title` (string, required), `body` (string, optional), `labels` (array of strings, optional)
- Response status is 201 Created
- Response body is a JSON object
- Object has: `id` (integer > 0), `number` (integer > 0), `title` (string, matches request), `state` (string: "open"), `user` (object), `created_at` (datetime)
- Content-Type is application/json

**Negative:**
- No auth → 401, body has `message` (string: "Requires authentication")
- Missing `title` → 422 Unprocessable Entity, body has `message` (string: "Validation Failed"), `errors` (array)
- Non-existent repo → 404, body has `message` (string: "Not Found")

---

## 18. POST /repos/{owner}/{repo}/issues/{number}/comments

- Request body: `body` (string, required)
- Response status is 201 Created
- Response body is a JSON object
- Object has: `id` (integer > 0), `body` (string, matches request), `user` (object), `created_at` (datetime)
- Content-Type is application/json

**Negative:**
- No auth → 401
- Missing `body` → 422 Unprocessable Entity
- Non-existent issue → 404
- Non-existent repo → 404

---

## 19. PATCH /repos/{owner}/{repo}/issues/{number}

- Request body: any subset of `title` (string), `body` (string), `state` (string: "open" | "closed"), `labels` (array of strings)
- Response status is 200 OK
- Response body is a JSON object with updated fields
- Object has: `id`, `number`, `title`, `state`, `body`, `labels`, `updated_at`
- Content-Type is application/json

**State transitions:**
- `state: "open"` → `state: "closed"` closes the issue
- `state: "closed"` → `state: "open"` reopens the issue

**Negative:**
- No auth → 401
- Non-existent issue → 404
- Invalid `state` value → 422 Unprocessable Entity
- Non-existent repo → 404

---

## 20. DELETE /repos/{owner}/{repo}/issues/{number}/comments/{comment_id}

- Response status is 204 No Content
- Response body is empty

**Negative:**
- No auth → 401
- Non-existent comment → 404
- Non-existent issue → 404
- Non-existent repo → 404

---

## 21. POST /repos/{owner}/{repo}/pulls

- Request body: `title` (string, required), `head` (string, required — branch name), `base` (string, required — target branch), `body` (string, optional), `issue` (integer, optional — linked issue number)
- Response status is 201 Created
- Response body is a JSON object
- Object has: `id` (integer > 0), `number` (integer > 0), `title` (string), `state` (string: "open"), `head` (object), `base` (object), `html_url` (url)
- Content-Type is application/json

**Negative:**
- No auth → 401
- Missing required fields → 422 Unprocessable Entity
- `head` branch does not exist → 404 or 422
- `base` branch does not exist → 422
- `head` = `base` → 422 Unprocessable Entity
- PR already exists for same head/base → 422 with validation error

---

## 22. PATCH /repos/{owner}/{repo}/pulls/{pull_number}

- Request body: any subset of `title` (string), `body` (string), `state` (string: "open" | "closed")
- Response status is 200 OK
- Response body is a JSON object with updated fields
- Content-Type is application/json

**Negative:**
- No auth → 401
- Non-existent PR → 404
- Non-existent repo → 404

---

## 23. PUT /repos/{owner}/{repo}/pulls/{pull_number}/merge

- Request body (optional): `commit_title` (string), `commit_message` (string), `merge_method` (string: "merge" | "squash" | "rebase", default "merge")
- Response status is 200 OK with `merged` (boolean: true)
- Response body has: `sha` (string — merge commit SHA), `merged` (boolean: true), `message` (string: "Pull Request successfully merged")

**Negative:**
- No auth → 401
- Non-existent PR → 404
- PR already merged → 405 Method Not Allowed, body has `message` (string: "Pull Request is not mergeable")
- PR has conflicts → 405, body has `message` (string: "Pull Request is not mergeable")
- PR is open (not approved) → may fail depending on branch protection rules

---

## 24. DELETE /repos/{owner}/{repo}/issues/{number}

- Response status is 204 No Content
- Response body is empty

**Negative:**
- No auth → 401
- Non-existent issue → 404
- Non-existent repo → 404

---

## 25. POST /repos/{owner}/{repo}/git/refs

- Request body: `ref` (string, required — e.g. "refs/heads/feature"), `sha` (string, required — commit SHA to branch from)
- Response status is 201 Created
- Response body is a JSON object with `ref` (string) and `object` (object with `sha`, `type`: "commit")
- Content-Type is application/json

**Negative:**
- No auth → 401
- Missing required fields → 422
- `sha` does not exist → 422 Unprocessable Entity
- `ref` already exists → 422 with validation error ("Reference already exists")

---

## 26. DELETE /repos/{owner}/{repo}/git/refs/{ref}

- Path: `ref` = branch name without `refs/heads/` prefix (e.g. `heads/feature`)
- Response status is 204 No Content
- Response body is empty

**Negative:**
- No auth → 401
- Non-existent ref → 404
- Non-existent repo → 404
- Attempt to delete default branch → 403 Forbidden, body has `message` (string: "Cannot delete the default branch")

---

# READ OPERATIONS

---

## Response Time SLA

| Category | Endpoints | SLA | Rationale |
|---|---|---|---|
| Single resource | /repos/{owner}/{repo}, /users/{username}, /issues/{number}, /branches/{branch} | < 1s | Simple lookup by ID, minimal processing |
| List resources | /repositories, /users/{username}/repos, /repos/{owner}/{repo}/branches | < 2s | Pagination overhead, multiple items |
| Filtered lists | /repos/{owner}/{repo}/issues, /pulls, /comments | < 2s | Query filtering + sorting |
| Metadata | /repos/{owner}/{repo}/languages, /topics, /tags, /contributors | < 2s | Aggregation over repo data |
| Auth-gated | /user/repos | < 2s | Token validation + filtered results |
| Rate limit | /rate_limit | < 500ms | Pure internal counter, no I/O |
| Write operations | POST /issues, /comments, /pulls, /git/refs | < 2s | Creates resource, returns new object |
| Write operations | PATCH /issues/{n}, /pulls/{n} | < 1s | Updates single resource |
| Write operations | PUT /pulls/{n}/merge | < 5s | Merge may trigger CI, depends on conflicts |
| Write operations | DELETE /issues/{n}, /comments/{id}, /git/refs/{ref} | < 1s | Removes resource, returns 204 |

**Test thresholds:** Performance tests use < 5s as a generous safety net. CI flakiness threshold is < 3s. Actual p95 is typically < 1s for all endpoints.

---

## Validation Rules

### Path Segments

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| `{owner}` | Existing GitHub username/org (case-insensitive) | Non-existent → 404 | GitHub normalizes case: `/repos/octocat/Hello-World` = `/repos/Octocat/hello-world` |
| `{repo}` | Existing repo name under owner | Non-existent → 404 | Repo names are case-insensitive |
| `{username}` | Existing GitHub user | Non-existent → 404 | Usernames are case-insensitive |
| `{number}` | Positive integer (1, 2, 3...) | 0 → 404, negative → 404, non-numeric → 404 | Issue/PR numbers are sequential, start at 1 |
| `{branch}` | Existing branch name | Non-existent → 404 | Branch names can contain `/` (e.g. `feature/foo`) |

### Query Parameters

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| `state` | `"open"`, `"closed"`, `"all"` (where supported) | Any other string → silently ignored, returns default | No 400 for invalid values |
| `per_page` | Integer 1–100 | 0 → treated as default (30), >100 → capped at 100, negative → treated as default | GitHub silently normalizes |
| `page` | Positive integer (1, 2, 3...) | 0 → treated as page 1, negative → treated as page 1 | No 400 for invalid values |
| `since` | Positive integer (repo ID) | Very large → empty array `[]` | Filters repos created after given ID |
| `sort` | `"created"`, `"updated"`, `"pushed"`, `"full_name"` (where supported) | Invalid → default sort | Not tested in current scope |

### Headers

| Header | Valid | Invalid | API behavior |
|---|---|---|---|
| `Authorization` | `Bearer <valid-token>` | Missing → 401, invalid → 401, expired → 401 | Token must have appropriate scope |
| `Accept` | `application/json` (default) | Topics endpoint needs `application/vnd.github.mercy-preview+json` | Without preview header, topics may return differently |
| `Content-Type` | `application/json` | Missing or wrong → 415 Unsupported Media Type | Required for POST/PATCH with JSON body |

### Request Body (Write Operations)

| Endpoint | Required Fields | Optional Fields | Invalid → Error |
|---|---|---|---|
| POST /issues | `title` (string) | `body` (string), `labels` (array of strings) | Missing `title` → 422 |
| POST /issues/{n}/comments | `body` (string) | — | Missing `body` → 422 |
| PATCH /issues/{n} | — | `title`, `body`, `state` ("open"\|"closed"), `labels` | Invalid `state` → 422 |
| POST /pulls | `title` (string), `head` (string), `base` (string) | `body` (string), `issue` (integer) | Missing required → 422; `head`=`base` → 422 |
| PATCH /pulls/{n} | — | `title`, `body`, `state` ("open"\|"closed") | Invalid `state` → 422 |
| PUT /pulls/{n}/merge | — | `commit_title`, `commit_message`, `merge_method` ("merge"\|"squash"\|"rebase") | Already merged → 405; conflicts → 405 |
| DELETE /issues/{n} | — | — | — |
| DELETE /issues/{n}/comments/{id} | — | — | — |
| POST /git/refs | `ref` (string, e.g. "refs/heads/feature"), `sha` (string, 40 hex) | — | Missing required → 422; sha invalid → 422; ref exists → 422 |
| DELETE /git/refs/{ref} | — | — | Default branch → 403 |

### Response Field Constraints

| Field | Constraint | Type |
|---|---|---|
| `id` | Positive integer, unique per resource type | `integer > 0` |
| `number` | Positive integer, sequential per repo (issues/PRs) | `integer > 0` |
| `name` | Non-empty string | `string (non-empty)` |
| `title` | Non-empty string | `string (non-empty)` |
| `state` | One of: `"open"`, `"closed"` (issues/PRs) | `enum` |
| `sha` | Exactly 40 hex characters (`[0-9a-f]{40}`) | `string (regex)` |
| `login` | Non-empty string, matches GitHub username format | `string (non-empty)` |
| `created_at` | ISO 8601 datetime string | `datetime` |
| `updated_at` | ISO 8601 datetime string, ≥ `created_at` | `datetime` |
| `merged_at` | ISO 8601 datetime or `null` (PRs only) | `datetime \| null` |
| `body` | String, can be empty | `string` |
| `contributions` | Positive integer | `integer > 0` |
| `html_url` | Valid URL string starting with `https://github.com/` | `url` |
| `avatar_url` | Valid URL string starting with `https://avatars.githubusercontent.com/` | `url` |
| `documentation_url` | Valid URL string (error responses only) | `url` |

### Data Type Summary

| GitHub type | C# mapping | Nullable? | Notes |
|---|---|---|---|
| integer | `int` | No (always present) | Use `[PositiveId]` for `id` fields |
| string | `string` | No | Use `[RequiredField]` |
| number | `double` | No | For `rate` fields |
| boolean | `bool` | No | Rare in GET responses |
| array | `List<T>` | No | May be empty `[]` |
| object | Nested model | No | Use separate model class |
| null or datetime | `DateTime?` | Yes | Only `merged_at` on PRs |
| url | `string` | No | Always starts with `https://` |

---

## Cross-cutting

- All responses are JSON with `Content-Type: application/json`
- Authenticated requests include `Authorization: Bearer <token>` → 5000 req/hour
- Unauthenticated requests → 60 req/hour
- Invalid token → 401 Unauthorized
- Default page size: 30 items; max: 100 (`?per_page=100`)
- Pagination: `Link` header with `rel="next"`, `rel="last"` URLs
- Rate limit headers present on every response
- When remaining = 0, responses return 403 with `"API rate limit exceeded"` message
- Errors return JSON body with `message` (string) and `documentation_url` (url)
- GitHub supports conditional requests via `ETag` / `If-None-Match` → 304 Not Modified (not tested in current scope)

---

## Risk Framing

### Known Gotchas

- **Issues include Pull Requests** — `GET /repos/{owner}/{repo}/issues` returns PRs disguised as issues. Filtering by `state` does not separate them. Tests should not assume all items are pure issues.
- **Private repos return 404, not 403** — if a repo is private and no auth is provided, GitHub returns 404 (not 403). This is by design to avoid leaking repo existence.
- **Topics require special Accept header** — `GET /repos/{owner}/{repo}/topics` needs `Accept: application/vnd.github.mercy-preview+json`. Without it, response may differ.
- **`merged_at` is nullable** — on Pull Requests, `merged_at` is `null` for open and closed-but-not-merged PRs. Tests must handle this.
- **Invalid query params are ignored** — e.g. `?state=invalid` does not return 400; it returns default results silently.
- **`per_page=0` is not an error** — GitHub treats it as default (30), not as a bad request.

### Rate Limit Impact

- Each test run consumes ~55 requests (one per test). With 5000/hr limit (authenticated), this is safe.
- Unauthenticated runs (60/hr) will fail after ~55 requests in a single run. Always use token.
- Parallel CI runs share the same token quota. Multiple concurrent runs may exhaust the limit.
- Monitor `X-RateLimit-Remaining` header in CI to detect approaching limits.

### Data Dependencies

- Tests depend on specific existing resources: repo `Gredja/AiTest`, issue #5, user `Gredja`.
- If repo is renamed, deleted, or issue #5 is closed/deleted — tests will fail with false negatives.
- Dynamic non-existent IDs (`maxId + 1`) depend on at least one item existing in the collection.
- Empty collections (e.g. repo with no tags, no contributors) return 200 with `[]` — tests must handle this gracefully.

### Flake Risks

- **Response time tests** — network-dependent. 5s threshold is generous but CI runners under load may exceed it. Consider marking as advisory, not blocking.
- **Pagination tests** — assume default page size is 30. GitHub could change this without notice.
- **State filter tests** — depend on repo having both open and closed issues/PRs. If all issues are closed, `?state=open` returns `[]` — test passes but doesn't validate filtering.
- **Contributors sorted by contributions** — order is deterministic but depends on commit history. New commits change the order.

### Auth Scope Requirements

- `public_repo` scope — minimum for read operations on public repos
- `repo` scope — required for private repo access and write operations (Phase 2)
- Token without appropriate scope → 403 Forbidden on affected endpoints
- Expired or revoked token → 401 Unauthorized on all authenticated endpoints

### Write Operation Risks

- **Cleanup is mandatory** — every E2E test that creates a resource (issue, comment, PR, branch) must delete it in teardown. Leftover resources pollute the sandbox repo.
- **Merge conflicts** — if two tests create PRs from different branches targeting the same base, second merge may fail with 405.
- **Rate limit on writes** — write operations count toward the same 5000/hr quota. E2E tests create + read + delete = 3x request count per scenario.
- **Branch protection rules** — if `main` has protection rules (required reviews, status checks), PUT /pulls/{n}/merge will fail with 405.
- **Race conditions** — parallel E2E runs creating issues/PRs in the same repo may interfere with each other's state assertions.
- **Ref deletion safety** — DELETE /git/refs/{ref} cannot delete the default branch. Attempt returns 403.

---

## Authoring Method

- **Structure + fields:** AI-drafted from GitHub API docs
- **Edge cases + gotchas:** Human-reviewed from production incidents
- **Risk framing:** Human-owned (business context)
