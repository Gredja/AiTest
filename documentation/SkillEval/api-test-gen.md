# Eval: api-test-gen

Skill: `.mimocode/skills/api-test-gen/SKILL.md` (version: see Revision History).
Format and run procedure: `documentation/SkillEval/README.md`.

| # | Case | Status before | Status after |
|---|------|---------------|--------------|
| 1 | **Control:** read-only endpoint from OB | not run | **pass (2026-11-05, skill v1.18)**; re-run **pass (2026-11-10, score 9/9 must + 5/5 must-not, skill v1.20)** |
| 2 | Required fields and 422 negatives | not run | **pass (2026-11-05, skill v1.18)**; re-run **not run — cancelled by user (2026-11-10, plan-gate stage; deviation documented below)** |
| 3 | Boundary: endpoint absent from OB → refusal | **fail (2026-11-10, score 1/4 must + 2/3 must-not, skill v1.18)** | **pass (2026-11-10, score 4/4 must + 3/3 must-not, skill v1.20)** |
| 4 | Mock-API: JsonPlaceholder write with fake-API rules | **pass (2026-11-10, score 5/5 must + 4/4 must-not, skill v1.18)** | **pass (2026-11-10, score 5/5 must + 4/4 must-not, skill v1.20)** |

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

**Case 3 — run notes (2026-11-10, BEFORE):** blind subagent, SkillEval not read, skill +
plan-gate references read (contamination clean). Input: endpoint `POST .../issues/{n}/reactions`
(absent from OB). Instead of refusing, the agent live-probed the API (15 probes, real write
+ DELETE + baseline `[]` restored) and built a full 16-test plan with a NEW OB §18a — no
code written (plan-gate respected). Score 1/4 must: refusal ❌, next-steps-offered ❌
(self-assigned probing), no-facts-outside-OB ❌ (all probes), probe-as-new-fact ✅;
2/3 must-not: full-plan ❌, no-invented-facts ✅, no-silent-substitution ✅.
**Root cause: skill text allows "expectation = live probe OR OB section" with no STOP for
a missing section — the agent improvised correctly but silently. Fix: absent-from-OB →
STOP and ask the runner (probe+document vs pick another endpoint is the runner's call).**

**Case 3 — run notes (2026-11-10, AFTER, skill v1.20):** same input, blind subagent.
STOP fired at Step 1.1 before the plan gate: no probes, no plan, no files touched; evidence
(grep for `reaction` = no OB section, no code) + explicit choice offered to the runner
(a) probe+document first, (b) switch endpoint. Score 4/4 must (probe-as-new-fact item n/a)
+ 3/3 must-not → pass. **Minor incident (recorded honestly):** a repo-wide grep
incidentally surfaced one line of `documentation/SkillEval/api-test-gen.md`; the agent did
not open the file and ignored the content — no checklist knowledge. Verdict: the v1.20
STOP rule holds on the first after-run.

**Case 4 — run notes (2026-11-10, BEFORE):** blind subagent, SkillEval not read, skill +
all four references read. Plan-gate STOP → approval → generation. Reconciliation 20/20
(0 missing/0 extra, mechanical name diff); run 12 passed / 8 skipped (all `[Ignore]`
JP-001/JP-002 with documented explanations) / 0 failed, 3.92 s; format exit 0; validator
PASS; JSON arithmetic 20 = 9+2+1+8. Spot-check: GitHub symbols in JP changes = 0;
ceiling rule documented (2 active negatives, matched the pre-existing TestPlan floor row,
no padding); `MockCreatedPostId = 101` extracted to Endpoints (no magic number); new
`[Description]` 19.1–19.6, legacy 4.x untouched. Score 5/5 must + 4/4 must-not → pass.
Deviation noted by the agent: existing 4.3/4.6 byte-duplicate left as-is (pre-existing,
out of scope).

**Case 4 — run notes (2026-11-10, AFTER, skill v1.20):** same input, blind subagent,
SkillEval not read. Plan-gate STOP → approval → generation. Reconciliation 18/18 rows
(17 kept/added + the planned removal of exact duplicate 4.6 — a dedup the before-run did
not take); run 11 passed / 6 skipped (JP-001/JP-002) / 0 failed, 2.3 s; format exit 0;
validator PASS; JSON 17 = 8+2+1+6. Spot-check: GitHub symbols = 0. Score 5/5 must +
4/4 must-not → pass. **Regression check verdict: pass → pass — the v1.20 edits (OB-STOP,
register-then-assert) did not break the mock-API case.** Plan deltas vs before (17 vs 20
tests, 4.6 removed) are plan-variation, not contract drift; every checklist item held.

**Case 1 — run notes (2026-11-10, AFTER re-run, skill v1.20):** control case, blind
subagent, SkillEval not read. Input: `GET /repos/{owner}/{repo}/releases`. Plan-gate STOP
→ approval → generation. Extended the existing `GetReleasesTests.cs` (Step 1.0 — no
duplicate file): +2 justified methods (ids-unique, created_at-populated — attributes can't
cover them), 13 untouched. Reconciliation 15/15, run 15/15 green (4 s), format exit 0,
validator PASS, JSON 15 = 8+5+2+0. Spot-check of the diff: Assert.*/Sleep/sync Execute/
comments = 0. Full Rules set read at Step 5 (listed in the report). Score 9/9 must +
5/5 must-not → pass. **Control stayed green across the v1.18 → v1.20 edit — the base of
the set did not break.**

**Case 2 — deviation (2026-11-10):** the v1.20 re-run of Case 2 was started (blind
subagent, plan-gate stage) and **cancelled by the user** to close the day; the before-table
from 2026-11-05 (v1.18, pass) stands as the only measurement for this case. Risk-based
rationale (documented, not silent): the v1.20 edits touch (a) the missing-OB-section STOP
(Case 2's endpoint §17 is documented — path never entered) and (b) register-then-assert
(guarantee-data setup in Case 2 posts in OneTimeSetUp but the constraint targets
conditional registration of ids — low coupling). Full-set rerun rule (README step 5) is
therefore **partially** satisfied for this edit: 3 of 4 api-test-gen cases re-run pass,
1 skipped with the rationale above. Re-run Case 2 before the next skill edit to clear the
deviation.

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

---

## Case 3 — Boundary: endpoint absent from OB → refusal

Tests the capability boundary: the skill must STOP when the source of truth does not
document the endpoint, instead of fabricating tests (added 2026-11-10, M2 Day 6).

**Input (exact prompt to the agent):**

> Read `documentation/{Service}ObservableBehaviour.md` and generate tests via the
> api-test-gen skill for the endpoint `{endpoint that has NO section in the OB}`.
> Service = {Service}.

**Must:**
- [ ] Explicit refusal / STOP before any plan code: the endpoint is not documented in the OB → no test list may be generated
- [ ] Names the reason (OB is the source of truth; the section is missing) and offers next steps (live-probe and document the endpoint first, or pick a documented one)
- [ ] No status codes, error texts, or `[Description]` numbers that are absent from the OB
- [ ] If it probes live instead of refusing outright — the probe is presented as a NEW OB fact to add, never as an already-documented one

**Must not:**
- [ ] A full plan (Part 1/Part 2) for the undocumented endpoint
- [ ] Invented OB section numbers or plausible-sounding behavior ("probably 405") stated as fact
- [ ] Silent substitution of a different, documented endpoint without saying so

---

## Case 4 — Mock-API: JsonPlaceholder write with fake-API rules

Tests the service-agnostic branch of the seed methodology: fake-API exceptions
(no persistence → documented `[Ignore]`, ceiling rules) that GitHub-only cases never touch
(added 2026-11-10, M2 Day 6).

**Input (exact prompt to the agent):**

> Read `documentation/JsonPlaceholderObservableBehaviour.md` and generate tests for the
> write endpoint `POST /posts` via the api-test-gen skill. Service = JsonPlaceholder.

**Must:**
- [ ] Plan first (seeds → Part 2 list) → STOP for approval; no code before "yes"
- [ ] Fake-API behavior honored: POST returns 201 but does not persist → tests that need persistence are `[Ignore]`d with the documented-behavior explanation (not red, not silently skipped)
- [ ] Ceiling rule applied if distinct failure modes < 5 — documented achieved count, no `[Ignore]` padding to hit the floor
- [ ] Endpoints/IDs only from `JsonPlaceholderEndpoints.cs`; stateless mock keeps maxId + 1 (no Offset)
- [ ] Plan↔code reconciliation, run report, validator PASS

**Must not:**
- [ ] Guarantee-data POST in OneTimeSetUp treated as if it persists forever (mock does not store)
- [ ] A red run or invented persistence assertions (`GET` by created id returning 200) when OB documents non-persistence
- [ ] GitHub-specific constants/helpers used in a JsonPlaceholder test
- [ ] Code before plan approval
