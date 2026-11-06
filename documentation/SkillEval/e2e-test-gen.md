# Eval: e2e-test-gen

Skill: `.mimocode/skills/e2e-test-gen/SKILL.md` (version: see Revision History).
Format and run procedure: `documentation/SkillEval/README.md`.

| # | Case | Status before | Status after |
|---|------|---------------|--------------|
| 1 | Write chain create → verify → cleanup | not run | not run |

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
