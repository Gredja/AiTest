# Eval: ui-test-gen

Skill: `.mimocode/skills/ui-test-gen/SKILL.md` (version: see Revision History).
Format and run procedure: `documentation/SkillEval/README.md`.

| # | Case | Status before | Status after |
|---|------|---------------|--------------|
| 1 | Page with locators: navigation + check | not run | not run |

---

## Case 1 — Page with locators: navigation + check

**Input (exact prompt to the agent):**

> Read `documentation/{service}UiObservableBehaviour.md` and generate UI tests via
> /ui-test-gen for the page `{page}`: `{flow}`. Service = {service}.

**Must:**
- [ ] Live snapshot first: `node Scripts/locator-dump.mjs <url>` — every locator from the snapshot, dated in the UI-OB
- [ ] Plan: Part 1 (case, category, resources→cleanup, locator source) + Part 2 (names `{{area}}_{{Flow}}_{{Outcome}}`, categories, `[Description]` with UI-OB §, files incl. page-object) → STOP; no files before "yes"
- [ ] All Rules read before code (including `ui-testing.md`)
- [ ] Locators role/text/label only (`GetByRole`/`GetByText`/`GetByLabel`); CSS/XPath on the page under test forbidden
- [ ] Page-objects: business actions (`CreateIssue`, not raw clicks); tests orchestrate and assert
- [ ] Auth via `storageState`; login only in `LoginSignInTests`; hybrid "act via UI → verify via API"
- [ ] UI write → API cleanup in `[OneTimeTearDown]` (last member), register-then-assert
- [ ] Data via `DataGenerator.RandomString` + `TestDataConstants`; no `WaitForTimeout` without a WHY
- [ ] Plan↔code reconciliation (tests and page-object) before format/tests; run report table; run green
- [ ] Report JSON: `locators.draft = 0`, validator → PASS

**Must not:**
- [ ] Locators invented from memory; CSS classes/XPath on the page under test; draft locators in code
- [ ] Logout in a feature test; login in every test; `WaitForTimeoutAsync(3000)` as a wait
- [ ] `Assert.*` (NUnit), helper methods in `*Tests.cs`, screenshot code in tests
- [ ] Static "test" data; cleanup without register-before-assert
- [ ] Files created before plan approval; a red run with a commit suggestion
