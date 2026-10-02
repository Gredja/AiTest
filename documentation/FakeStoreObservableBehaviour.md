# Observable Behaviour — FakeStore API

**Project:** Gredja (.NET 10.0)
**API Under Test:** FakeStore API (https://fakestoreapi.com)
**Date:** 2026-10-02
**Scope:** GET + POST /auth/login
**Test Plan:** [FakeStoreTestPlan.md](FakeStoreTestPlan.md)

---

## What AI does

When generating or reviewing FakeStore API tests, the AI agent:

1. **Reads this document first** — before any code generation or review, loads the relevant endpoint section
2. **Uses Positive bullets as assertions** — each `- Object has: \`field\` (type)` becomes a `.Should().Be()` / `.Should().NotBeNull()` / `.Should().BeGreaterThan()` call
3. **Uses Negative bullets as test cases** — each negative bullet = one `[Category("Negative")]` test method
4. **Uses Validation Rules for model generation** — Response Field Constraints map directly to C# model properties with attributes (`[PositiveId]`, `[RequiredField]`, `[ValueRange]`)
5. **Uses Request Body table for POST tests** — required fields → mandatory assertions, optional fields → conditional assertions
6. **Uses State transitions for E2E chains** — FakeStore has no persisted write state; only login issues a token
7. **Uses Endpoint Priority for coverage ordering** — P0 first, P3 last when generating incrementally
8. **Never invents fields** — only asserts fields listed in this document; if a field is missing from the response, reports it as a discrepancy
9. **Uses Seed methodology** — for each endpoint: 5 seeds → expand to table with `# | Case | Category | Priority | Source seed` → enforce 5+ active negatives → see `Rules/test-practices.md` → "Seed methodology"
10. **Uses Test Plan risks for coverage** — maps each Top-3 risk from `FakeStoreTestPlan.md` to at least one test case; an uncovered risk is a coverage gap, reported in the review

---

## Test Categories

Full definitions and rules: `Rules/categories.md`. One service category on class, one check-type category on method.

| Category | Applies to | Example test name pattern |
|----------|-----------|---------------------------|
| `HealthCheck` | Status code checks only | `*_ReturnsOk`, `*_ReturnsCreated` |
| `ContractCheck` | Response/request schema matches expected models | `*_ResponseMatchesContract` |
| `Smoke` | Basic functionality — not empty, correct count, content-type | `*_ReturnsNonEmptyList`, `*_ContentTypeIsJson` |
| `Regression` | Schema / field validation via attributes | `*_EachItemHasValidFields`, `*_HasValidFields` |
| `Negative` | Error handling — non-existent ID, bad input, missing field | `*_NonExistentId_*`, `*_MissingPassword_*` |
| `Performance` | Response time | `*_ResponseTimeIsAcceptable` |

---

## Endpoint Priority (by business value)

| Priority | Endpoints | Rationale |
|---|---|---|
| **P0 — Core** | GET /products, GET /products/{id}, POST /auth/login | Каталог и вход — основной сценарий API |
| **P1 — Important** | GET /products/categories, GET /products/category/{category}, GET /users/{id} | Фильтрация каталога, карточка пользователя |
| **P2 — Useful** | GET /carts, GET /carts/{id} | Корзины — вторичный ресурс |
| **P3 — Reference** | GET /users | Список пользователей (мок-данные, справочный) |

---

# READ OPERATIONS

---

## 1. GET /products

- Response status is 200 OK
- Response body is a JSON array of exactly 20 items (`TestConfig.ExpectedProductCount`)
- Each item has: `id` (integer > 0, unique, range 1–20), `title` (string, non-empty), `price` (number ≥ 0), `description` (string, non-empty), `category` (string, non-empty; set of 4 categories), `image` (url string), `rating` (object)
- `rating` has: `rate` (number, 0–5), `count` (integer ≥ 0)
- Union of all `category` values covers every entry of GET /products/categories
- Content-Type is application/json
- Response time < 5s (`TestConfig.MaxResponseTimeMs`)

**Negative:**
- Invalid query parameter (e.g. `?invalid=true`) → **200 OK, list unchanged** — server silently ignores unknown params — Bug: documentation/Bugs/FakeStore/FS-010-list-endpoints-invalid-params-return-200.md (expected 400, `[Ignore]`d)

---

## 2. GET /products/{id}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `id` (integer, equals requested), `title` (string, non-empty), `price` (number ≥ 0), `description` (string, non-empty), `category` (string, non-empty), `image` (url string), `rating` (object with `rate` 0–5, `count` ≥ 0)
- Valid ids: 1–20; boundary checks at id 5 and id 20
- Repeated calls return identical data (static dataset)
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Non-existent numeric id (maxId + 1) → **200 OK with empty object** — expected 404 — Bug: FS-001 ( `[Ignore]`d)
- id = 0 → **200 OK** — expected 404 — Bug: FS-002 ( `[Ignore]`d)
- Negative id (-1) → **200 OK** — expected 404 — Bug: FS-003 ( `[Ignore]`d)
- Non-numeric segment (`/products/abc`) → **200 OK** — products route accepts any segment (no active negative possible for this endpoint)

---

## 3. GET /products/categories

- Response status is 200 OK
- Response body is a JSON array of exactly 4 unique strings (`TestConfig.ExpectedCategoryCount`)
- Every entry is a valid category usable in GET /products/category/{category}
- Content-Type is application/json
- Response time < 5s

**Negative:**
- None — endpoint takes no parameters (mock never errors)

---

## 4. GET /products/category/{category}

- Response status is 200 OK
- Response body is a JSON array; every item matches the requested `category` field
- Known category `electronics` (`TestConfig.TestCategoryName`) returns exactly 6 items (`TestConfig.ExpectedProductsInCategoryCount`)
- Each item has the same fields as GET /products items
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Non-existent category → **200 OK with empty array** (not 404) — active test asserts empty list

---

## 5. GET /carts

- Response status is 200 OK
- Response body is a JSON array of exactly 7 items (`TestConfig.ExpectedCartCount`)
- Each item has: `id` (integer > 0), `userId` (integer > 0), `date` (datetime string), `products` (non-empty array)
- Each `products[]` entry has: `productId` (integer > 0), `quantity` (integer)
- Content-Type is application/json
- Response time < 5s

**Negative:**
- None documented (list takes no meaningful parameters; invalid-query behavior not probed)

---

## 6. GET /carts/{id}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `id` (integer, equals requested), `userId` (integer > 0), `date` (datetime), `products` (non-empty array of `{productId, quantity}`)
- Valid ids: 1–7; boundary checks at id 4 and id 7 (last)
- Repeated calls return identical data
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Non-existent numeric id (maxId + 1) → **200 OK** — expected 404 — Bug: FS-007 ( `[Ignore]`d)
- id = 0 → **200 OK** — expected 404 — Bug: FS-008 ( `[Ignore]`d)
- Negative id (-1) → **200 OK** — expected 404 — Bug: FS-009 ( `[Ignore]`d)
- Non-numeric segment (`/carts/abc`) → **400 Bad Request** (active — carts route validates the segment)

---

## 7. GET /users

- Response status is 200 OK
- Response body is a JSON array of exactly 10 items (`TestConfig.ExpectedUserCount`)
- Each item has: `id` (integer > 0, unique), `email` (string), `username` (string), `password` (string), `name` (object: `firstname`, `lastname`), `phone` (string), `address` (object: `city`, `street`, `number`, `zipcode`, `geolocation`)
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Invalid query parameter → **200 OK, list unchanged** — Bug: FS-010 ( `[Ignore]`d, expected 400)

---

## 8. GET /users/{id}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `id` (integer, equals requested), `email`, `username`, `password`, `name` (object with `firstname`/`lastname`), `phone`, `address` (object with `city`/`street`/`number`/`zipcode`/`geolocation`)
- Valid ids: 1–10; boundary checks at id 5 and id 10 (last)
- Repeated calls return identical data
- Content-Type is application/json
- Response time < 5s

**Negative:**
- Non-existent numeric id (maxId + 1) → **200 OK** — expected 404 — Bug: FS-004 ( `[Ignore]`d)
- id = 0 → **200 OK** — expected 404 — Bug: FS-005 ( `[Ignore]`d)
- Negative id (-1) → **200 OK** — expected 404 — Bug: FS-006 ( `[Ignore]`d)
- Non-numeric segment (`/users/abc`) → **400 Bad Request** (active — users route validates the segment)

---

# WRITE OPERATIONS

---

## 9. POST /auth/login

- Request body: `username` (string, required), `password` (string, required)
- Valid credentials live in `TestConfig.LoginUsername` / `TestConfig.LoginPassword`
- Response status is **201 Created** (not 200)
- Response body is a JSON object with: `token` (string, non-empty)
- Content-Type is application/json

**Negative:**
- Missing `username` → 400 Bad Request (active)
- Missing `password` → 400 Bad Request (active)
- Empty body (`{}` or null fields) → 400 Bad Request (active)
- Empty-string credentials → 400 Bad Request (active)
- Whitespace-only credentials → 401 Unauthorized (active)
- Invalid username → 401 Unauthorized (active)
- Invalid password → 401 Unauthorized (active)

---

## Response Time SLA

| Category | Endpoints | SLA | Rationale |
|---|---|---|---|
| Single resource | /products/{id}, /carts/{id}, /users/{id} | < 5000 ms (`TestConfig.MaxResponseTimeMs`) | Static mock; generous budget for network |
| List resources | /products, /products/categories, /products/category/{c}, /carts, /users | < 5000 ms | Same |

---

## Validation Rules

### Path Segments

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| `{id}` (products) | Integer 1–20 | 0, negative, maxId+1 → expected 404 | **Returns 200** (FS-001/002/003) |
| `{id}` (carts) | Integer 1–7 | 0, negative, maxId+1 → expected 404 | **Returns 200** (FS-007/008/009); `abc` → 400 |
| `{id}` (users) | Integer 1–10 | 0, negative, maxId+1 → expected 404 | **Returns 200** (FS-004/005/006); `abc` → 400 |
| `{category}` | One of 4 known categories | Non-existent category | 200 with empty array |

### Query Parameters

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| any | — | Unknown/garbage values | **200 OK, ignored** (FS-010) |

### Headers

| Header | Valid | Invalid | API behavior |
|---|---|---|---|
| — | No auth required for GET | — | Login token is decorative; never needed elsewhere |

### Request Body (Write Operations)

| Endpoint | Required Fields | Optional Fields | Invalid → Error |
|---|---|---|---|
| POST /auth/login | `username` (string), `password` (string) | — | Missing/empty → 400; wrong values → 401 |

### Response Field Constraints

| Field | Constraint | Type |
|---|---|---|
| `id` | Positive integer, unique per collection | `integer > 0` |
| `price` | ≥ 0 | `number` |
| `rating.rate` | 0–5 | `number` |
| `rating.count` | ≥ 0 | `integer` |
| `title`, `description`, `category`, `image` | Non-empty string | `string (non-empty)` |
| `products` | Non-empty array | `array` |
| `token` | Non-empty string | `string (non-empty)` |

### Data Type Summary

| FakeStore type | C# mapping | Nullable? | Notes |
|---|---|---|---|
| integer | `int` | No | Ids and counts |
| number | `decimal` / `double` | No | `price`, `rating.rate` |
| string | `string` | No | |
| boolean | `bool` | No | Not used in current models |
| array | `List<T>` | No | May be empty only for non-existent category |
| object | Nested model | No | `rating`, `name`, `address`, `geolocation` |
| date | `DateTime` | No | Cart `date` |

---

## Cross-cutting

- All responses are JSON with `Content-Type: application/json`
- No authentication headers required; `POST /auth/login` returns a token that is never used
- No rate limiting on the mock
- Static dataset: 20 products, 7 carts, 10 users, 4 categories — expected counts live in `testsettings.json` (`TestConfig.Expected*`)
- No persistence: there are no POST/PUT/DELETE operations for resources (login only)
- Dataset is immutable between runs — repeated-call assertions are stable

---

## Risk Framing

### Known Gotchas

- FS-001…FS-009: GetById never returns 404 — non-existent/zero/negative ids return **200**; corresponding tests are `[Ignore]`d with bug references
- FS-010: list endpoints ignore invalid query parameters (200 instead of 400)
- `/products/abc` returns **200** (greedy route) while `/carts/abc` and `/users/abc` return **400** — routes behave differently, do not generalize
- Login returns **201** for success and **400** (not 401) for empty credentials

### Rate Limit Impact

- None — mock has no rate limits; full suite can hammer it freely

### Data Dependencies

- Expected counts are configuration-driven (`TestConfig.Expected*`) — if the upstream mock changes its dataset, tests fail by design (drift detection)
- Category tests depend on `TestConfig.TestCategoryName` = `electronics`

### Flake Risks

- Low overall (static mock); Performance tests (< 5s) may flake on network hiccups — SLA has 5× headroom

### Auth Scope Requirements

- None. Login credentials are read from `testsettings.json`; never hardcode them in tests

---

## Authoring Method

- **Structure + fields:** AI-drafted from live probes + existing C# models (`Core/Models/FakeStore/`)
- **Edge cases + gotchas:** probed live 2026-09/10, documented in `documentation/Bugs/FakeStore/FS-001…FS-010`
- **Risk framing:** human-owned (business context)
