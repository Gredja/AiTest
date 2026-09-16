# Observable Behaviour — {Service} API

**Project:** Gredja (.NET 10.0)
**API Under Test:** {Service} API ({BaseUrl})
**Date:** {Date}
**Scope:** {GET / POST / PATCH / DELETE}
**Test Plan:** [{Service}TestPlan.md]({Service}TestPlan.md)

---

## What AI does

When generating or reviewing {Service} API tests, the AI agent:

1. **Reads this document first** — before any code generation or review, loads the relevant endpoint section
2. **Uses Positive bullets as assertions** — each `- Object has: \`field\` (type)` becomes a `.Should().Be()` / `.Should().NotBeNull()` / `.Should().BeGreaterThan()` call
3. **Uses Negative bullets as test cases** — each negative bullet = one `[Category("Negative")]` test method
4. **Uses Validation Rules for model generation** — Response Field Constraints map directly to C# model properties with attributes (`[PositiveId]`, `[RequiredField]`, `[ValueRange]`)
5. **Uses Request Body table for POST/PATCH tests** — required fields → mandatory assertions, optional fields → conditional assertions
6. **Uses State transitions for E2E chains** — e.g. "state: open → closed" maps to PATCH test that verifies `state` changed
7. **Uses Endpoint Priority for coverage ordering** — P0 first, P3 last when generating incrementally
8. **Never invents fields** — only asserts fields listed in this document; if a field is missing from the response, reports it as a discrepancy
9. **Uses Seed methodology** — for each endpoint: 5 seeds → expand to table with `# | Case | Category | Priority | Source seed` → enforce 5+ negatives → see `Rules/test-practices.md` → "Seed methodology"

---

## Endpoint Priority (by business value)

| Priority | Endpoints | Rationale |
|---|---|---|
| **P0 — Core** | | |
| **P1 — Important** | | |
| **P2 — Useful** | | |
| **P3 — Reference** | | |

---

# READ OPERATIONS

---

## 1. GET /{path}

- Response status is 200 OK
- Response body is a JSON object
- Object has: `field` (type), `field` (type)
- Content-Type is application/json
- Response time < {SLA}

**Negative:**
- Non-existent resource → 404, body has `message` (string: "Not Found")

---

# WRITE OPERATIONS

---

## 2. POST /{path}

- Request body: `field` (type, required), `field` (type, optional)
- Response status is 201 Created
- Response body is a JSON object
- Object has: `id` (integer > 0), `field` (matches request)
- Content-Type is application/json

**Negative:**
- No auth → 401
- Missing required field → 422 Unprocessable Entity

---

## 3. PATCH /{path}/{id}

- Request body: any subset of `field` (type)
- Response status is 200 OK
- Response body is a JSON object with updated fields
- Content-Type is application/json

**Negative:**
- No auth → 401
- Non-existent resource → 404

---

## 4. DELETE /{path}/{id}

- Response status is 204 No Content
- Response body is empty

**Negative:**
- No auth → 401
- Non-existent resource → 404

---

## Response Time SLA

| Category | Endpoints | SLA | Rationale |
|---|---|---|---|
| Single resource | | | |
| List resources | | | |

---

## Validation Rules

### Path Segments

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| `{id}` | Positive integer | 0, negative → 404 | |

### Query Parameters

| Parameter | Valid | Invalid | API behavior |
|---|---|---|---|
| `per_page` | Integer 1–100 | 0 → default, >100 → capped at 100 | |

### Headers

| Header | Valid | Invalid | API behavior |
|---|---|---|---|
| `Authorization` | `Bearer <valid-token>` | Missing → 401 | |

### Request Body (Write Operations)

| Endpoint | Required Fields | Optional Fields | Invalid → Error |
|---|---|---|---|
| POST /path | `field` (type) | `field` (type) | Missing required → 422 |
| PATCH /path/{id} | — | `field` (type) | |

### Response Field Constraints

| Field | Constraint | Type |
|---|---|---|
| `id` | Positive integer, unique | `integer > 0` |
| `name` | Non-empty string | `string (non-empty)` |
| `created_at` | ISO 8601 datetime | `datetime` |

### Data Type Summary

| {Service} type | C# mapping | Nullable? | Notes |
|---|---|---|---|
| integer | `int` | No | |
| string | `string` | No | |
| boolean | `bool` | No | |
| array | `List<T>` | No | May be empty |
| object | Nested model | No | |

---

## Cross-cutting

- All responses are JSON with `Content-Type: application/json`
- Authenticated requests include `Authorization: Bearer <token>`
- Errors return JSON body with `message` (string) and optional `documentation_url` (url)
- Default page size: 30 items; max: 100 (`?per_page=100`)
- Rate limit headers present on every response

---

## Risk Framing

### Known Gotchas

- 

### Rate Limit Impact

- 

### Data Dependencies

- 

### Flake Risks

- 

### Auth Scope Requirements

- 

---

## Authoring Method

- **Structure + fields:** AI-drafted from {Service} API docs
- **Edge cases + gotchas:** Human-reviewed from production incidents
- **Risk framing:** Human-owned (business context)
