---
name: api-checklist
description: Generate a coverage checklist (markdown table) for one API endpoint from Observable Behaviour and Rules — each row is a check with its category and OB source, plus explicit coverage gaps. Use when the user says "checklist for endpoint", "api checklist", "what to verify for GET /posts", "чек-лист по эндпоинту". NOT for writing test code — for generated tests use /api-test-gen instead.
---

# API Checklist

## Instructions

Produce a **checklist**, not test code: one markdown table the user can review before
generating or reviewing tests. Read the sources first — never checklist from memory.

### Step 1: Pin the endpoint

- Get service + path + HTTP method(s) from the user's request (e.g. `GitHub GET /repos/{owner}/{repo}/collaborators`).
- If the path is ambiguous, ask once; do not guess an endpoint that is not in the docs.

### Step 2: Read the sources (in this order)

1. `documentation/{Service}ObservableBehaviour.md` — the section(s) for this endpoint
   (search by path). This is the source of truth for fields, types, status codes, negatives.
2. `Rules/assertions.md` — which helper covers which check (`ShouldHaveStatusCode`,
   `ShouldHaveError`, `ShouldMatchRequest`, `ShouldHaveValidContract/Fields`).
3. `Rules/categories.md` — the closed set of check-type categories and the
   name-pattern → category mapping.
4. For write endpoints also `Rules/test-practices.md` → "Guarantee Data",
   "Read-after-write visibility", "Test data for write operations".

If the endpoint has no OB section — STOP and report that the checklist cannot be
grounded; offer to probe the live API instead of inventing rows.

### Step 3: Build the contract list

From the OB section extract, before writing any row:

- methods + success statuses (and documented error statuses with their messages);
- path/query parameters and which are required;
- request fields (for writes): required vs optional;
- response fields the OB documents as validated (types, patterns, enums);
- documented quirks (pagination, sorting, rate limits, idempotency).

### Step 4: Emit the checklist table

Columns (exact):

`| # | Category | Check | Source |`

- **Category** — one of: `HealthCheck`, `ContractCheck`, `Smoke`, `Regression`,
  `Negative`, `Performance` (Rules/categories.md). Never invent a category.
- **Check** — one sentence, assert-shaped ("status 201 + Location header present"),
  naming the helper when one exists (`ShouldHaveStatusCode(201)`).
- **Source** — `OB §N.M` (section number) or `GAP — verify live` when the check is
  implied by practice but not documented in the OB.

Row set (stop when exhausted — ceiling rule applies, never pad):

1. success status(s) → `HealthCheck`;
2. response contract (documented fields present/typed) → `ContractCheck`;
3. content type / shape sanity → `Smoke`;
4. each documented invariant (field pattern, state rule, id match) → `Regression`;
5. each documented error status → `Negative` (status + documented message via `ShouldHaveError`);
6. **one `Negative` row per required request field that omits exactly that field**
   (`*_Missing{Field}_*` — mandatory per Rules/test-practices.md);
7. write endpoints: create → visible to GET (`Smoke`) + record matches request
   (`Regression`) + visible within time limit (`Performance`);
8. GET endpoints: response time informational (`Performance`).

Order rows by category in the order above. Number them 1..N.

### Step 5: Close with a gaps block

After the table:

```
**Gaps:** <count> — <one line each: what OB does not document and must be probed live>
**Negatives:** <N> (floor 5 per endpoint; state achieved count or the ceiling reason)
```

## Important

- **Every row cites a source.** A row you cannot source is `GAP — verify live`, never a
  confident claim. Checklist facts come from the OB file, not from the model's memory.
- This skill never writes `.cs` files, test methods, or `[Category]` attributes — that is
  `/api-test-gen`. Output ends at the markdown table + gaps block.
- Do not duplicate checks: one OB fact = one row; `ShouldHaveValidFields()` covers
  attribute checks (Rules/test-practices.md → "Don't duplicate attribute checks").
- Mock-API / documented-empty quirks from the OB must appear as rows with the quirk as
  the expected behaviour, not be silently dropped.

## Examples

User: "чек-лист для GitHub GET /repos/{owner}/{repo}/commits"
→ read OB section for list-commits → table rows: 200 HealthCheck, JSON array ContractCheck,
pagination params Smoke/Regression per OB, 404 Negative (documented message), 401 Negative,
Performance row; gaps: none/…

User: "checklist for POST /posts (JsonPlaceholder)"
→ plus write rows (created record visible, matches request, time limit — with the
fake-API `[Ignore]` quirk noted as expected behaviour per OB).

## Troubleshooting

- OB section missing → stop, report, offer live probe; never emit ungrounded rows.
- OB contradicts an existing test → checklist follows the OB, add a gap line noting the
  conflict for the user to resolve (live run is the arbiter — see Rules/test-practices.md).
- Endpoint has >25 useful rows → keep them; only dedupe exact repeats, do not cap.
