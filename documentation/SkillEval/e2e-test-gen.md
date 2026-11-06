# Eval: e2e-test-gen

Skill: `.mimocode/skills/e2e-test-gen/SKILL.md` (version: see Revision History).
Format and run procedure: `documentation/SkillEval/README.md`.

| # | Case | Status before | Status after |
|---|------|---------------|--------------|
| 1 | Write chain create → verify → cleanup | not run | **pass (2026-11-05, skill v1.7)** |

**Case 1 — run notes (2026-11-05):** GitHub issues chains (E2E-10), blind subagent with
anti-hang budget. Plan-gate STOP → approval → generation; only the three approved files
touched. Mechanical reconciliation: 6/6 matched; live sandbox run 6/6 (0 failed, 19 s);
format/build green; validator PASS; `cleanupVerified = true`. Spot-check: NUnit
Assert/Sleep/999/finally/helpers/conditional-registration — 0; registry + OneTimeTearDown
(last member) present; register-then-assert via the JUST-REWRITTEN helper (status assert →
unconditional add) confirmed in code. Deviations, all recorded: (1) two planned generator
factories collapsed into one (`WithTitleBodyLabels`), (2) combined PATCH sent as
`CreateIssueModelRequest` (the exact OB §19 content subset) instead of extending
`UpdateIssueModelRequest`, (3) `ShouldMatchRequest` → explicit label-name asserts
(documented shape exception), (4) filter scoped with `FullyQualifiedName~` to avoid
substring-matching Api tests. Doc drift found and fixed en route (floor table 5→6, global
counters re-measured). Verdict: skill held the contract, including the updated
register-then-assert rule.

---

## Case 1 — Write chain create → verify → cleanup

**Input (exact prompt to the agent):**

> Read `documentation/{Service}ObservableBehaviour.md` and generate E2E tests via
> /e2e-test-gen for `{resource}` in the sandbox {repo}. Service = {Service}.

**Must:**
- [ ] Plan: Part 1 (seeds: happy/negative/edge) + Part 2 (full list of method names, categories, target file) → STOP; no code before "yes"
- [ ] All Rules/*.md read before code
- [ ] Chain: POST → verify → GET/PATCH → cleanup; cleanup order (comments before issues, branches before PRs)
- [ ] Fixture-registry: IDs registered immediately after creation, BEFORE assert (register-then-assert)
- [ ] Cleanup ONLY in `[OneTimeTearDown]` (last class member), warning-only; no per-test `[SetUp]`/`[TearDown]`/`finally`
- [ ] No helper methods in `*Tests.cs` (chains live in base/helpers)
- [ ] Write data fictional, ≥2 dimensions varied; `ShouldMatchRequest()` for request↔response
- [ ] Plan↔code reconciliation table before format/build/tests; run report table; run green
- [ ] Report JSON: `cleanupVerified = true`, validator → PASS

**Must not:**
- [ ] Cleanup in a test's `finally`; register after assert (leaks on assert failure)
- [ ] `Assert.*` (NUnit), sync `Execute`, `Thread.Sleep`
- [ ] Hardcoded non-existent IDs (999); static write data ("test"); hardcoded endpoint paths
- [ ] Helpers inside the test class; login/logout in a test; violated cleanup order
- [ ] A red run or `cleanupVerified = false` with a commit suggestion
