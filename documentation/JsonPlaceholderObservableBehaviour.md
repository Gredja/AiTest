# Observable Behaviour — JsonPlaceholder API

**Project:** Gredja (.NET 10.0)
**API Under Test:** JsonPlaceholder API (https://jsonplaceholder.typicode.com)
**Date:** 2026-10-02
**Scope:** GET + POST/PUT/PATCH/DELETE /posts
**Test Plan:** [JsonPlaceholderTestPlan.md](JsonPlaceholderTestPlan.md)

---

## What AI does

When generating or reviewing JsonPlaceholder API tests, the AI agent:

1. **Reads this document first** — before any code generation or review, loads the relevant endpoint section
2. **Uses Positive bullets as assertions** — each `- Object has: \`field\` (type)` becomes a `.Should().Be()` / `.Should().NotBeNull()` / `.Should().BeGreaterThan()` call
3. **Uses Negative bullets as test cases** — each negative bullet = one `[Category("Negative")]` test method
4. **Uses Validation Rules for model generation** — Response Field Constraints map directly to C# model properties with attributes (`[PositiveId]`, `[RequiredField]`, `[ValueRange]`)
5. **Uses Request Body table for POST/PUT/PATCH tests** — required fields → mandatory assertions, optional fields → conditional assertions
6. **Uses State transitions for E2E chains** — no persisted state: DELETE does not delete, PATCH does not merge (see gotchas)
7. **Uses Endpoint Priority for coverage ordering** — P0 first, P3 last when generating incrementally
8. **Never invents fields** — only asserts fields listed in this document; if a field is missing from the response, reports it as a discrepancy
9. **Uses Seed methodology** — for each endpoint: 5 seeds → expand to table with `# | Case | Category | Priority | Source seed` → enforce 5+ active negatives → see `Rules/test-practices.md` → "Seed methodology"
10. **Uses Test Plan risks for coverage** — maps each Top-3 risk from `JsonPlaceholderTestPlan.md` to at least one test case; an uncovered risk is a coverage gap, reported in the review

---

## Test Categories

Full definitions and rules: `Rules/categories.md`. One service category on class, one check-type category on method.

| Category | Applies to | Example test name pattern |
|----------|-----------|---------------------------|
| `HealthCheck` | Status code checks only | `*_ReturnsOk`, `*_ReturnsCreated` |
| `ContractCheck` | Response/request schema matches expected models | `*_ResponseMatchesContract` |
| `Smoke` | Basic functionality — not empty, correct count, content-type | `*_ReturnsNonEmptyList`, `*_ContentTypeIsJson` |
| `Regression` | Schema / field validation via attributes | `*_EachItemHasValidFields`, `*_HasValidFields` |
| `Negative` | Error handling — non-existent ID, malformed body, missing field | `*_NonExistentId_ReturnsNotFound`, `*_MalformedJson_*` |
| `Performance` | Response time | `*_ResponseTimeIsAcceptable` |

---

## Endpoint Priority (by business value)

| Priority | Endpoints | Rationale |
|---|---|---|
| **P0 — Core** | GET /posts, GET /posts/{id}, POST /posts, PUT/PATCH /posts/{id}, DELETE /posts/{id} | Основной CRUD-ресурс — единственная write-поверхность API |
| **P1 — Important** | GET /comments, GET /comments/{id}, GET /users, GET /users/{id}, GET /users/{id}/posts | Комментарии и пользователи — связующие сущности |
| **P2 — Useful** | GET /albums (+{id}, ?userId), GET /todos (?userId), GET /users/{id}/albums, GET /users/{id}/todos | Производные коллекции |
| **P3 — Reference** | GET /photos (+{id}, ?albumId) | Медиа-справочник (5000 элементов) |

---

# READ OPERATIONS

---

## 1. GET /posts

- Response status is 200 OK
- Response body is a JSON array of exactly 100 items (`TestConfig.ExpectedPostCount`)
- Each item has: `id` (integer > 0, unique), `userId` (integer > 0), `title` (string, non-empty), `body` (string, non-empty)
- Content-Type is application/json
- Response time < 5s (`TestConfig.MaxResponseTimeMs`)

**Negative:**
- None — mock ignores query input on this route (mock-API exception; filters live on dedicated routes below)

---

## 2. GET /posts/{id}

- Response status is 200 OK
- Response body is a JSON object with `id` (equals requested), `userId`, `title`, `body`
- Valid ids: 1–100; boundary checks at id 50 and id 100 (last)
- Repeated calls return identical data (static dataset)
- Response time < 5s

**Negative:**
- Non-existent id (maxId + 1) → 404 with `message` (string: "Not Found") (active)
- id = 0 → 404 (active)
- Negative id (-1) → 404 (active)
- Non-numeric id (`/posts/abc`) → 404 (active)
- id = int.MaxValue → 404 (active)

---

## 3. GET /comments

- Response status is 200 OK
- Response body is a JSON array of exactly 500 items (`TestConfig.ExpectedCommentCount`)
- Each item has: `id` (integer > 0, unique), `postId` (integer > 0), `name` (string, non-empty), `email` (string, non-empty), `body` (string, non-empty)
- Response time < 5s

**Negative:**
- None — list route (mock-API exception)

---

## 4. GET /comments/{id}

- Response status is 200 OK
- Response body is a JSON object with `id`, `postId`, `name`, `email`, `body`
- Valid ids: 1–500; boundary checks at id 250 and id 500 (last)
- Response time < 5s

**Negative:**
- Non-existent id (maxId + 1), id = 0, id = -1, non-numeric `abc`, id = int.MaxValue → **404** (all active)

---

## 5. GET /comments?postId={postId}

- Response status is 200 OK
- Response body is a JSON array; every item has `postId` equal to the filter
- Filter for an existing post returns a non-empty list
- Response time < 5s

**Negative:**
- Non-existent postId → **200 OK with empty array** (active — asserts empty list, not an error)

---

## 6. GET /albums

- Response status is 200 OK
- Response body is a JSON array of exactly 100 items (`TestConfig.ExpectedAlbumCount`)
- Each item has: `id` (integer > 0, unique), `userId` (integer > 0), `title` (string, non-empty)
- Response time < 5s

**Negative:**
- None — list route (mock-API exception)

---

## 7. GET /albums/{id}

- Response status is 200 OK
- Response body is a JSON object with `id`, `userId`, `title`
- Valid ids: 1–100; boundary checks at id 50 and id 100
- Response time < 5s

**Negative:**
- Non-existent id (maxId + 1), id = 0, id = -1, non-numeric `abc`, id = int.MaxValue → **404** (all active)

---

## 8. GET /albums?userId={userId}

- Response status is 200 OK
- Response body is a JSON array; every item has `userId` equal to the filter
- Response time < 5s

**Negative:**
- Non-existent userId → **200 OK with empty array** (active)

---

## 9. GET /photos

- Response status is 200 OK
- Response body is a JSON array of exactly 5000 items (`TestConfig.ExpectedPhotoCount`)
- Each item has: `id` (integer > 0, unique), `albumId` (integer > 0), `title` (string), `url` (url string), `thumbnailUrl` (url string)
- Response time < 5s

**Negative:**
- None — list route (mock-API exception)

---

## 10. GET /photos/{id}

- Response status is 200 OK
- Response body is a JSON object with `id`, `albumId`, `title`, `url`, `thumbnailUrl`
- Valid ids: 1–5000; boundary checks at id 2500 and id 5000
- Response time < 5s

**Negative:**
- Non-existent id (maxId + 1), id = 0, id = -1, non-numeric `abc`, id = int.MaxValue → **404** (all active)

---

## 11. GET /photos?albumId={albumId}

- Response status is 200 OK
- Response body is a JSON array; every item has `albumId` equal to the filter
- Response time < 5s

**Negative:**
- Non-existent albumId → **200 OK with empty array** (active)

---

## 12. GET /todos

- Response status is 200 OK
- Response body is a JSON array of exactly 200 items (`TestConfig.ExpectedTodoCount`)
- Each item has: `id` (integer > 0, unique), `userId` (integer > 0), `title` (string, non-empty), `completed` (boolean)
- Response time < 5s

**Negative:**
- None — list route (mock-API exception)

---

## 13. GET /todos?userId={userId}

- Response status is 200 OK
- Response body is a JSON array; every item has `userId` equal to the filter
- Response time < 5s

**Negative:**
- Non-existent userId → **200 OK with empty array** (active)

---

## 14. GET /users

- Response status is 200 OK
- Response body is a JSON array of exactly 10 items (`TestConfig.ExpectedUserCount`)
- Each item has: `id` (integer > 0, unique), `name` (string), `username` (string), `email` (string), `address` (object: `street`, `suite`, `city`, `zipcode`, `geo` {`lat`, `lng`}), `phone` (string), `website` (string), `company` (object: `name`, `catchPhrase`, `bs`)
- Response time < 5s

**Negative:**
- None — list route (mock-API exception)

---

## 15. GET /users/{id}

- Response status is 200 OK
- Response body is a JSON object with all fields listed in GET /users
- Valid ids: 1–10; boundary checks at id 5 and id 10
- Response time < 5s

**Negative:**
- Non-existent id (maxId + 1), id = 0, id = -1, non-numeric `abc`, id = int.MaxValue → **404** (all active)

---

## 16. GET /users/{id}/albums

- Response status is 200 OK
- Response body is a JSON array of albums; every item has `userId` equal to the requested id
- Response time < 5s

**Negative:**
- Non-existent userId → **200 OK with empty array** (active)

---

## 17. GET /users/{id}/posts

- Response status is 200 OK
- Response body is a JSON array of posts; every item has `userId` equal to the requested id
- Response time < 5s

**Negative:**
- Non-existent userId → **200 OK with empty array** (active)

---

## 18. GET /users/{id}/todos

- Response status is 200 OK
- Response body is a JSON array of todos; every item has `userId` equal to the requested id
- Response time < 5s

**Negative:**
- Non-existent userId → **200 OK with empty array** (active)

---

# WRITE OPERATIONS

---

## 19. POST /posts

- Request body: `userId` (integer, required), `title` (string, required), `body` (string, required)
- Response status is **201 Created**
- Response body echoes the request plus `id` (integer, mock always assigns 101)
- Response matches request fields (`ShouldMatchRequest`)
- Response time < 5s

**Negative:**
- Malformed JSON body (`{"title":`) → **500 Internal Server Error** (active)
- Non-object JSON body (`"just-a-string"`) → **500 Internal Server Error** (active)
- Missing `title` / missing `body` / missing `userId` → **201 Created** — expected 400 — Bug: JP-001 ( `[Ignore]`d)
- Wrong field types (number title, boolean body, string userId) → **201 Created** — expected 400 — Bug: JP-001 ( `[Ignore]`d)
- Empty body `{}` → **201 Created** — expected 400 — Bug: JP-001 ( `[Ignore]`d)

---

## 20. PUT /posts/{id}

- Request body: `userId` (integer), `title` (string), `body` (string) — full replacement
- Response status is 200 OK
- Response body echoes the request plus the requested `id`
- Response time < 5s

**Negative:**
- id = 0 → **500 Internal Server Error** (active)
- Negative id (-1) → **500** (active)
- Non-existent id (maxId + 1) → **500** — expected 404 (active, mock returns server error)
- Non-numeric id (`abc`) → **500** (active)
- Malformed JSON body → **500** (active)
- Missing `title` / missing `body` / missing `userId` → **200 OK** — expected 400 — Bug: JP-005 ( `[Ignore]`d)

---

## 21. PATCH /posts/{id}

- Request body: any subset of `userId`, `title`, `body` (partial update is valid semantics)
- Response status is 200 OK
- Response time < 5s

**Negative:**
- Malformed JSON body → **500 Internal Server Error** (active)
- id = 0 / id = -1 / non-existent id / non-numeric `abc` → **200 OK** — expected 400/404 — Bug: JP-006 ( `[Ignore]`d)
- Response contains ONLY sent fields + `id` (rest null) — expected merge with existing data — Bug: JP-004 ( `[Ignore]`d, regression category)

---

## 22. DELETE /posts/{id}

- Response status is 200 OK
- Response body is an empty JSON object `{}`
- Response time < 5s

**Negative:**
- Malformed JSON body → **500 Internal Server Error** (active)
- DELETE on collection (`/posts`) or trailing slash (`/posts/`) → **404 Not Found** (active)
- id = 0 / id = -1 / non-existent id (maxId + 1) / non-numeric `abc` → **200 OK** — expected 404 — Bug: JP-003 ( `[Ignore]`d)
- Deleted resource still returned by subsequent GET (mock does not persist deletes) — Bug: JP-002 ( `[Ignore]`d, regression category)

---

## Response Time SLA

| Category | Endpoints | SLA | Rationale |
|---|---|---|---|
| Single resource | /{resource}/{id} (posts, comments, albums, photos, todos, users) | < 5000 ms (`TestConfig.MaxResponseTimeMs`) | Static mock; generous network budget |
| List resources | all lists + filters | < 5000 ms | 5000-item photos list is the heaviest |

---

## Validation Rules

### Path Segments

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| `{id}` (GET by id) | Integer 1..N | 0, -1, abc, int.MaxValue, maxId+1 | **404** (active — the only mock resource that errors correctly) |
| `{id}` (PUT) | Integer 1..100 | 0, -1, abc, maxId+1 | **500** (server error, not 404) |
| `{id}` (PATCH) | Integer 1..100 | 0, -1, abc, maxId+1 | **200** (ignored — Bug JP-006) |
| `{id}` (DELETE) | Integer 1..100 | 0, -1, abc, maxId+1 | **200** (ignored — Bug JP-003) |
| `{id}` (users nested) | Integer 1–10 | Non-existent user | **200** with empty array |

### Query Parameters

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| `postId`, `userId`, `albumId` | Existing owner id | Non-existent id | **200** with empty array (filter semantics, not an error) |

### Headers

| Header | Valid | Invalid | API behavior |
|---|---|---|---|
| — | No auth at all | — | Mock is fully open |

### Request Body (Write Operations)

| Endpoint | Required Fields | Optional Fields | Invalid → Error |
|---|---|---|---|
| POST /posts | `userId` (int), `title` (string), `body` (string) | — | Missing/wrong type → **201 anyway** (JP-001); malformed JSON → 500 |
| PUT /posts/{id} | `userId`, `title`, `body` (full replacement) | — | Missing → **200 anyway** (JP-005); malformed JSON → 500 |
| PATCH /posts/{id} | — (partial is valid) | any subset | Malformed JSON → 500; invalid id → 200 (JP-006) |
| DELETE /posts/{id} | — | — | Malformed JSON → 500; any id → 200 (JP-003) |

### Response Field Constraints

| Field | Constraint | Type |
|---|---|---|
| `id` | Positive integer, unique per collection | `integer > 0` |
| `userId`, `postId`, `albumId` | Positive integer | `integer > 0` |
| `title`, `body`, `name`, `email` | Non-empty string | `string (non-empty)` |
| `completed` | Boolean (JSON `completed`, C# `IsCompleted`) | `boolean` |
| `url`, `thumbnailUrl` | URL string | `string (url)` |

### Data Type Summary

| JsonPlaceholder type | C# mapping | Nullable? | Notes |
|---|---|---|---|
| integer | `int` | No | Ids |
| string | `string` | No | |
| boolean | `bool` | No | todos `completed` |
| array | `List<T>` | No | May be empty for non-existent filters |
| object | Nested model | No | `address`, `company`, `geo` |

---

## Cross-cutting

- All responses are JSON with `Content-Type: application/json`
- No authentication, no rate limiting — full suite can run freely
- **No persistence**: writes are accepted but never stored — DELETE does not delete (JP-002), POST-created records never appear in GET lists, PATCH/PUT do not mutate the dataset
- Static dataset: 100 posts, 500 comments, 100 albums, 5000 photos, 200 todos, 10 users — counts from `testsettings.json` (`TestConfig.Expected*`)
- Dataset is immutable between runs — repeated-call and count assertions are stable
- POST always assigns `id = 101` (mock convention)

---

## Risk Framing

### Known Gotchas

- JP-001: POST accepts any body — missing fields, wrong types, empty object all return 201
- JP-002: DELETE returns 200 but does not actually delete — subsequent GET still returns the resource
- JP-003: DELETE on any id (including 0/-1/non-existent/non-numeric) returns 200
- JP-004: PATCH returns only sent fields + id — no merge with existing data
- JP-005: PUT accepts body missing required fields — no validation
- JP-006: PATCH returns 200 for any id — 0, -1, non-existent, non-numeric
- Malformed JSON (unparseable body) reliably returns **500** on POST/PUT/PATCH/DELETE — the only body-level error the mock produces
- GET by id is the ONLY resource route that behaves correctly (404 for invalid ids)

### Rate Limit Impact

- None — mock has no rate limits

### Data Dependencies

- Expected counts are configuration-driven (`TestConfig.Expected*`); upstream dataset change = test failure by design
- Filter tests depend on known owner ids (e.g. existing postId/userId/albumId from OneTimeSetUp fetches)

### Flake Risks

- Low (static mock, no shared state); Performance tests (< 5s) have ~5× headroom; photos list (5000 items) is the heaviest response

### Auth Scope Requirements

- None — API is fully open

### Write Operation Risks

- Writes are safe: mock discards them — no cleanup required, no sandbox pollution; read-after-write persistence tests are `[Ignore]`d by design (JP-002)

---

## Authoring Method

- **Structure + fields:** AI-drafted from live probes (2026-10-02) + existing C# models (`Core/Models/JsonPlaceholder/`)
- **Edge cases + gotchas:** probed live, documented in `documentation/Bugs/JsonPlaceholder/JP-001…JP-006`
- **Risk framing:** human-owned (business context)
