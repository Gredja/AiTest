# Eval: api-test-gen

Skill: `.mimocode/skills/api-test-gen/SKILL.md` (version: see Revision History).
Format and run procedure: `documentation/SkillEval/README.md`.

| # | Case | Status before | Status after |
|---|------|---------------|--------------|
| 1 | Read-only endpoint from OB | not run | **pass (2026-11-05, skill v1.18)** |
| 2 | Required fields and 422 negatives | not run | **pass (2026-11-05, skill v1.18)** |

**Case 1 — run notes (2026-11-05):** blind subagent (never saw the checklist), plan-gate
STOP → approval → generation. Mechanical reconciliation (Grep vs Part 2): 11/11 matched,
0 extra; run 20/20 (0 failed); validator PASS; spot-check: NUnit Assert/Sleep/999 — 0,
FluentAssertions — 21. Two deviations from the plan — both justified and recorded:
(1) `Id` was not duplicated into the service JsonFields (single-source: already in
`Generic.JsonFields`), (2) `IdQueryParam(object)` instead of `(int)` — non-numeric id from
test 14.10. Verdict: the skill held the contract as designed.

**Case 2 — run notes (2026-11-05):** GitHub POST /issues, blind subagent (attempt 2 — the
first hung on a tool call and was cancelled). **Key catch during the run:** the first code
draft registered conditionally — `if (created.Data is { Number: > 0 }) ... Add` BEFORE the
status assert — seen by the user in chat and sent back as a correction (unconditional
register after `ShouldHaveStatusCode`). After the fix: 11/11 matched, run 17/17 (0 failed),
format OK, validator PASS. Deviations recorded: (1) `LongTitle` — conditional register
after `BeOneOf(201,422)` (legitimate: a 422 creates nothing), (2) `ShouldMatchRequest` via
a reduced request (labels string[] → Label objects — the documented exception pattern).
**Verdict: the skill does not forbid `if`-around-registration — a finding for a skill text
strengthening (candidate for an early fix, not only M2 Day 6).**

---

## Case 1 — Read-only endpoint from OB

**Input (exact prompt to the agent):**

> Read `documentation/{Service}ObservableBehaviour.md` and generate tests for the
> read-only endpoint `{endpoint}` via the api-test-gen skill. Service = {Service}.

**Must:**
- [ ] Plan first: Part 1 (seed table) + Part 2 (full list of method names, categories, target file, created/modified) → STOP for approval; no code before "yes"
- [ ] All Rules/*.md read before code (visible in the report)
- [ ] Tests exactly per the approved plan — no extras, no drops (plan↔code reconciliation table before format/tests)
- [ ] `[Category]` on every test, `[Category("{Service}")]` on the class, `[Description]` on methods
- [ ] FluentAssertions only (`ShouldHaveStatusCode` etc.); typed models; async; no comments
- [ ] Endpoints/IDs from `{Service}Endpoints.cs`; non-existent IDs dynamic (maxId+1), never 999
- [ ] Run report table (method/category/result/error) — run green
- [ ] Report JSON passes `Scripts/validate-skill-report.ps1` → PASS
- [ ] Models: `ModelResponse`/`ModelRequest`, attributes on separate lines; constants in Endpoints

**Must not:**
- [ ] `Assert.*` (NUnit), `JsonElement`+`TryGetProperty`, sync `Execute`, `Thread.Sleep`
- [ ] Hardcoded URLs/endpoints, static "non-existent" IDs, invented error messages
- [ ] Code generated before plan approval; duplicate files
- [ ] Commit suggestion after a red gate; a red run in the report
- [ ] Comments, magic numbers, single-letter lambda parameters (`p =>`)

---

## Case 2 — Required fields and 422 negatives

**Input (exact prompt to the agent):**

> Read `documentation/{Service}ObservableBehaviour.md` and generate tests for the
> write endpoint `{endpoint}` via the api-test-gen skill, with special attention to the
> negative cases. Service = {Service}.

**Must:**
- [ ] Plan includes at least 5 active negatives (`[Ignore]`d do not count)
- [ ] A separate `*_Missing{Field}_*` negative for EVERY required body field (an empty body does not substitute)
- [ ] Negatives with a dynamic non-existent ID (maxId+1); static 0/-1 only as "always invalid"
- [ ] `ShouldHaveError` with the OB-documented text (or `[Ignore]` with an explanation for a documented API bug)
- [ ] Guarantee data: GET empty → POST in OneTimeSetUp → cleanup in OneTimeTearDown (last member)
- [ ] `ShouldMatchRequest()` for POST/PATCH request↔response comparison
- [ ] Write data fictional, varied across ≥2 dimensions via `DataGenerator`
- [ ] Plan↔code reconciliation + run report + validator PASS (as in case 1)

**Must not:**
- [ ] A single token negative for the whole endpoint instead of one per required field
- [ ] Static 999 as the non-existent ID; hardcoded endpoint paths
- [ ] `[Ignore]` without an explanation; an invented error text absent from OB
- [ ] Cleanup in a per-test `finally`; Api+E2E overlap in one run without the Offset
