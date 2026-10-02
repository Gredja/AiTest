# FakeStore Test Plan

**Project:** Gredja (.NET 10.0)
**API Under Test:** FakeStore API (https://fakestoreapi.com)
**Observable Behaviour:** [FakeStoreObservableBehaviour.md](FakeStoreObservableBehaviour.md)

**Source of truth:** OB defines API behavior first → this plan is the derived test inventory → tests. A test that contradicts the OB is a bug in the test; an OB bullet without a test is a coverage gap.

**Status legend:** `Done` — tests implemented and passing · `[Ignore]`d tests are documented API bugs (`documentation/Bugs/FakeStore/FS-001…FS-010`), counted separately

---

## Coverage Summary

- **Endpoints:** 9/9 covered (100%)
- **Tests:** 95 total — 84 executable + 11 `[Ignore]`d
- **Negative floor:** see "Negative Floor Status" below (mock-API exception + ceiling rule apply)

---

## Endpoint Inventory

### Products

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.1 | GET /products | Status, contract, non-empty, Content-Type, fields, count=20, rating, unique ids, all categories, response time, invalid query [Ignore] FS-010 | Done |
| 1.2 | GET /products/{id} | Status, contract, non-empty, fields, id match, boundary ids (5, 20), repeated calls, response time, non-existent/zero/negative [Ignore] FS-001…003 | Done |
| 1.3 | GET /products/categories | Status, contract, non-empty, Content-Type, count=4, valid + unique categories, response time | Done |
| 1.4 | GET /products/category/{category} | Status, contract, non-empty, Content-Type, all items belong to category, fields, electronics count=6, non-existent category → empty, response time | Done |

### Carts

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 2.1 | GET /carts | Status, contract, non-empty, Content-Type, count=7, fields, unique ids, products present, response time | Done |
| 2.2 | GET /carts/{id} | Status, contract, non-null, fields, id match, products, boundary ids (4, 7), repeated calls, response time, invalid segment → 400, non-existent/zero/negative [Ignore] FS-007…009 | Done |

### Users

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 3.1 | GET /users | Status, contract, non-empty, Content-Type, fields, count=10, unique ids, response time, invalid query [Ignore] FS-010 | Done |
| 3.2 | GET /users/{id} | Status, contract, non-null, fields, id match, boundary ids (5, 10), repeated calls, response time, invalid segment → 400, non-existent/zero/negative [Ignore] FS-004…006 | Done |

### Auth

| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 4.1 | POST /auth/login | Status 201, token non-empty, Content-Type, invalid username/password → 401, empty creds → 400, missing username/password → 400, empty body → 400, whitespace creds → 401 | Done |

---

## Test Categories

Counts by check-type category (95 tests total):

| Category | Count |
|---|---|
| HealthCheck | 9 |
| ContractCheck | 8 |
| Smoke | 18 |
| Regression | 31 |
| Negative | 21 (10 active + 11 `[Ignore]`d) |
| Performance | 8 |

---

## Negative Floor Status

Rule: `Rules/test-practices.md` Step 4 — ≥5 active negatives per endpoint, mock-API exception + ceiling rule apply when distinct useful failure modes are exhausted (verified by live probes).

| Endpoint | Active | Ignored | Ceiling / Note |
|---|---|---|---|
| POST /auth/login | 7 | 0 | Floor exceeded |
| GET /carts/{id} | 1 | 3 | Mock returns 200 for any numeric id (FS-007…009); only `abc` → 400 is active |
| GET /users/{id} | 1 | 3 | Mock returns 200 for any numeric id (FS-004…006); only `abc` → 400 is active |
| GET /products/{id} | 0 | 3 | Mock returns 200 for any input including `abc` — no active failure mode exists (documented exception) |
| GET /products/category/{category} | 1 | 0 | Non-existent category → 200 empty list is the only failure mode |
| GET /products | 0 | 1 | Invalid query ignored (FS-010); lists covered by mock-API exception |
| GET /users | 0 | 1 | Invalid query ignored (FS-010); lists covered by mock-API exception |
| GET /carts | 0 | 0 | No parameterizable failure mode (documented exception) |
| GET /products/categories | 0 | 0 | No parameters at all (documented exception) |

---

## Top-3 Risks

1. **Mock bug farm masks regressions** — FakeStore never 404s on GetById (FS-001…009); a real routing regression would still return 200. Mitigation: `[Ignore]` tests carry exact expected behavior; invalid-segment (`abc`) tests are the only active sentinels per GetById resource.
2. **Dataset drift** — expected counts (20/7/10/4, electronics=6) are asserted from `testsettings.json`; upstream mock changing its dataset fails the suite by design. Mitigation: counts centralized in `TestConfig.Expected*`, no hardcoded numbers in tests.
3. **Performance test flakiness** — 8 response-time tests depend on network to a public mock; SLA 5000 ms has ~5× headroom. Mitigation: generous `TestConfig.MaxResponseTimeMs`.

---

## Authoring Method

- **Inventory:** generated 2026-10-02 from the actual test suite (`Api/FakeStore/Tests/`), category counts computed from code
- **Risks:** derived from FS bug reports + suite structure; human-owned wording
