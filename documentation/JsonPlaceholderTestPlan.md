# JsonPlaceholder Test Plan

**Project:** Gredja (.NET 10.0)
**API Under Test:** JsonPlaceholder API (https://jsonplaceholder.typicode.com)
**Observable Behaviour:** [JsonPlaceholderObservableBehaviour.md](JsonPlaceholderObservableBehaviour.md)

**Source of truth:** OB defines API behavior first → this plan is the derived test inventory → tests. A test that contradicts the OB is a bug in the test; an OB bullet without a test is a coverage gap.

**Status legend:** `Done` — tests implemented and passing · `[Ignore]`d tests are documented mock bugs (`documentation/Bugs/JsonPlaceholder/JP-001…JP-006`), counted separately

---

## Coverage Summary

- **Endpoints:** 20/20 covered (100%)
- **Tests:** 214 total — 197 executable + 17 `[Ignore]`d (16 Negative + 1 Regression)
- **Fixtures:** 21 (Posts split into GetAll / GetById / Create / Update / Delete)
- **Negative floor:** see "Negative Floor Status" below (mock-API exception + ceiling rule apply)

---

## Endpoint Inventory

### Posts (CRUD)

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.1 | GET /posts | Status, contract, non-empty, Content-Type, fields, count=100, unique ids, response time | Done |
| 1.2 | GET /posts/{id} | Status, contract, fields, id match, boundary ids (50, 100), repeated calls, 5 negatives (non-existent/0/-1/abc/intMax → 404) | Done |
| 1.3 | POST /posts | Status 201, generated id, fields, ShouldMatchRequest, special chars, malformed JSON → 500, non-object JSON → 500, 5 × missing/wrong-type/empty [Ignore] JP-001 | Done |
| 1.4 | PUT /posts/{id} | Status 200, fields, ShouldMatchRequest, preserves id, 5 negatives active (0/-1/non-existent/abc/malformed → 500), 3 × missing field [Ignore] JP-005 | Done |
| 1.5 | PATCH /posts/{id} | Status 200, updated title, malformed JSON → 500, 3 × invalid id [Ignore] JP-006, merge behavior [Ignore] JP-004 | Done |
| 1.6 | DELETE /posts/{id} | Status 200, empty object, malformed JSON → 500, collection/trailing slash → 404, 4 × any-id-200 [Ignore] JP-003, still-deleted GET [Ignore] JP-002 | Done |

### Comments

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 2.1 | GET /comments | Status, contract, non-empty, Content-Type, fields, count=500, unique ids, response time | Done |
| 2.2 | GET /comments/{id} | Status, contract, fields, id match, boundary ids (250, 500), repeated calls, 5 negatives → 404 | Done |
| 2.3 | GET /comments?postId= | Status, contract, all items belong to post, non-existent post → empty | Done |

### Albums

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 3.1 | GET /albums | Status, contract, non-empty, Content-Type, fields, count=100, unique ids, response time | Done |
| 3.2 | GET /albums/{id} | Status, contract, fields, id match, boundary ids (50, 100), repeated calls, 5 negatives → 404 | Done |
| 3.3 | GET /albums?userId= | Status, contract, all items belong to user, non-existent user → empty | Done |

### Photos

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 4.1 | GET /photos | Status, contract, non-empty, Content-Type, fields, count=5000, unique ids, response time | Done |
| 4.2 | GET /photos/{id} | Status, contract, fields, id match, boundary ids (2500, 5000), repeated calls, 5 negatives → 404 | Done |
| 4.3 | GET /photos?albumId= | Status, contract, all items belong to album, non-existent album → empty | Done |

### Todos

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 5.1 | GET /todos | Status, contract, non-empty, Content-Type, fields, count=200, unique ids, response time | Done |
| 5.2 | GET /todos?userId= | Status, contract, all items belong to user, non-existent user → empty | Done |

### Users (+ nested)

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 6.1 | GET /users | Status, contract, non-empty, Content-Type, fields, count=10, unique ids, response time | Done |
| 6.2 | GET /users/{id} | Status, contract, non-null, fields, id match, boundary ids (5, 10), repeated calls, 5 negatives → 404 | Done |
| 6.3 | GET /users/{id}/albums | Status, contract, all items belong to user, non-existent user → empty | Done |
| 6.4 | GET /users/{id}/posts | Status, contract, all items belong to user, non-existent user → empty | Done |
| 6.5 | GET /users/{id}/todos | Status, contract, all items belong to user, non-existent user → empty | Done |

---

## Test Categories

Counts by check-type category (214 tests total):

| Category | Count |
|---|---|
| HealthCheck | 22 |
| ContractCheck | 18 |
| Smoke | 36 |
| Regression | 64 (1 `[Ignore]`d — JP-004) |
| Negative | 64 (48 active + 16 `[Ignore]`d) |
| Performance | 10 |

---

## Negative Floor Status

Rule: `Rules/test-practices.md` Step 4 — ≥5 active negatives per endpoint, mock-API exception + ceiling rule apply when distinct useful failure modes are exhausted (verified by live probes 2026-10-02).

| Fixture / Endpoint | Active | Ignored | Ceiling / Note |
|---|---|---|---|
| GET /posts/{id} | 5 | 0 | Floor met |
| GET /comments/{id} | 5 | 0 | Floor met |
| GET /albums/{id} | 5 | 0 | Floor met |
| GET /photos/{id} | 5 | 0 | Floor met |
| GET /users/{id} | 5 | 0 | Floor met |
| PUT /posts/{id} | 5 | 3 | Floor met (500-family) |
| PATCH /posts/{id} | 1 | 4 | Malformed JSON is the only active failure mode; invalid ids return 200 (JP-006) |
| POST /posts | 2 | 5 | Mock accepts any body (JP-001); malformed/non-object JSON → 500 are the only active modes |
| DELETE /posts/{id} | 2 | 5 | Mock deletes nothing (JP-002/003); malformed → 500 and collection → 404 are active |
| Filter endpoints (postId/userId/albumId) ×7 | 2 each | 0 | Non-existent filter → 200 empty is the designed behavior; second negative = in-filter validation |
| GetAll lists ×6 | 0 | 0 | Mock-API exception (lists accept any input) |

---

## Top-3 Risks

1. **Mock accepts everything** — write negatives are mostly `[Ignore]`d (JP-001/003/005/006); active sentinels are malformed-JSON → 500 tests. A regression that makes the mock silently 200 on broken bodies would go unnoticed if those two tests are removed.
2. **No persistence** — DELETE does not delete (JP-002), POST records never appear in GET lists: read-after-write and state-transition tests cannot exist as active tests; cleanup logic must never depend on JP writes.
3. **Dataset drift** — expected counts (100/500/100/5000/200/10) come from `testsettings.json`; upstream dataset change fails the suite by design (drift detection), centralized in `TestConfig.Expected*`.

---

## Authoring Method

- **Inventory:** generated 2026-10-02 from the actual test suite (`Api/JsonPlaceholder/Tests/`), category counts computed from code
- **Risks:** derived from JP bug reports (JP-001…JP-006) + live probes; human-owned wording
