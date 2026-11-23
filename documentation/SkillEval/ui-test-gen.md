# Eval: ui-test-gen

Skill: `.mimocode/skills/ui-test-gen/SKILL.md` (version: see Revision History).
Format and run procedure: `documentation/SkillEval/README.md`.

| # | Case | Status before | Status after |
|---|------|---------------|--------------|
| 1 | Page with locators: navigation + check | pass (2026-11-16, skill v1.6) | **pass (2026-11-23, skill v1.7)** |
| 1 | Page with locators: navigation + check | pass (2026-11-12, skill v1.5) | **pass (2026-11-16, skill v1.6)** |
| 1 | Page with locators: navigation + check | pass (2026-11-06, skill v1.4) | **pass (2026-11-12, skill v1.5)** |

**Case 1 — run notes (2026-11-23, skill v1.7, blind subagent, SkillEval ban honored):**
Same concrete input as prior runs (GitHub issues-tab navigation, UI-4). Status before reused
from the 2026-11-16 v1.6 run — pre-edit skill state (HEAD) identical to evaluated v1.6; the
only delta under test is the v1.7 live-snapshot command port (`node Scripts/locator-dump.mjs`
→ `dotnet run --project Scripts/Tools -- locator-dump`). Research 11 tool calls; phase 1 hit a
**runner-corrected misread** — the agent quoted and ran the retired node path (MODULE_NOT_FOUND)
while the skill on disk already stated the dotnet command; runner re-pointed it at the skill's
verbatim step (no checklist/SkillEval information leaked; ban honored throughout). Plan gate
STOP → runner approval WITH modifications (snapshot-first with the skill's actual command;
extend-only; new locators only from a successful dump) → phase 2: **live snapshot via the new
C# tool succeeded** (roles confirmed: `heading "Issues" level=1`, `link "New issue"`,
`link "Open (0)"`, `link "Closed"`, `combobox "Search issues"`, `button "Search"`; empty-state
heading dumped at level=2 → locator must not pin level, recorded in OB). Implementation:
`files.created = []`; +2 Regression methods (`Issues_NavigateViaTab_ClosedFilter_ShowsListChrome`,
`Issues_NavigateViaTab_OpenFilter_RoundTrip`), `IssuesPage.ClickOpenTabAsync` (same bounded
re-apply as Closed), OB UI-4/UI-17 re-dated 2026-11-23. Reconciliation 3/3 matched (existing
Smoke kept, no duplicates); run `Category=GitHubUi&FullyQualifiedName~IssuesUiTests` → **6/6
green (30 s)**; `dotnet format` clean; report validator PASS; `locators.draft = 0`,
`verifiedLive = 5`. Contamination: none. Verdict: skill held the contract despite the
command-port delta; score 10/10 must, 5/5 must-not.

**Case 1 — run notes (2026-11-16, skill v1.6, blind subagent, SkillEval ban honored):**
Same concrete input as the v1.4/v1.5 runs (GitHub issues-tab navigation, UI-4). Status
before reused from the 2026-11-12 v1.5 run — the pre-edit skill state (HEAD) is identical
to the evaluated v1.5; the only delta under test is the v1.6 troubleshooting row (credential
source synced with the `.env` removal). Research 16 tool calls incl. a live locator-dump
(skill Step 1.2, pre-gate); plan gate STOP → runner approval WITH modification by the
evaluator (empty-state heading rejected as data-dependent — breaks when an open issue
exists; data-independent `New issue` chrome assert approved) → implementation: 2 files, no
new locators (reuse `HasNewIssueButtonAsync` — reuse-before-extract), OB UI-4 re-verified
2026-11-16. Reconciliation 1/1; run 4/4 green (IssuesUiTests filter, 0 failed); format
pass; validator PASS; `locators.draft = 0`. Total 33 tool calls. Contamination: none.
Verdict: skill held the contract, score 10/10 must, 5/5 must-not.

---

**Case 1 — run notes (2026-11-12, skill v1.5, blind subagent, SkillEval ban honored):**
Same concrete input as the v1.4 run (GitHub issues-tab navigation, UI-4). Research finding:
coverage already exists → per hard rule 6 the plan EXTENDED `Issues_NavigateViaTab_ShowsListView`
instead of duplicating (no new files; 3 modifications). Plan gate STOP → runner approval →
implementation. Live dump 2026-11-12 for `heading "Issues" level=1` + `link "Open (0)"` (the
snapshot exposed a second sidebar `heading "Issues" level=2` — locator pinned `Level = 1`);
repo-tab locator reused from OB UI-3 (verified 2026-11-06, reuse-before-extract). Reconciliation
1/1 matched; run 4/4 green (IssuesUiTests filter, 0 failed); format pass; validator PASS
(re-run by runner: exit 0); `locators.draft = 0`. Flagged Rules-win deviation: counter parsed
via shared `TabCountParserHelper` instead of the plan's ad-hoc `Open\D*(\d+)` (code-style →
same pattern in 2+ files) — accepted, OB bullet keeps the format guidance. Troubleshooting-row
edit (the motivation for v1.5) did not disturb any must/must-not item. Verdict: skill held the
contract; score 10/10 must, 5/5 must-not.

---

**Case 1 — run notes (2026-11-06, skill v1.4):** GitHub issues-tab navigation (UI-4), blind subagent,
anti-hang budget. Plan-gate STOP → approval → implementation; exactly the 4 approved files.
Mechanical reconciliation: 1/1 matched, pre-existing tests untouched; run 1/1 (3 s, 0
failed); format clean; validator PASS; `locators.draft = 0`, `verifiedLive = 3` (live dump
2026-11-06 for `link "Issues"`, OB-dated reuse for heading/Open-tab per reuse-before-extract).
Spot-check: WaitForTimeout/CSS/XPath/NUnit Assert — 0; role locators at file bottom;
outcome wait (`WaitForURLAsync`) before asserts; navigation-only flow → no registry/teardown
changes (creates nothing). Deviations: none material (Open-tab locator uses `Exact = false`
to cover `Open (N)`/`Open 0` — OB gotcha, within plan intent). Verdict: skill held the
contract, including storageState and locator-discipline hard rules.

---

## Case 1 — Page with locators: navigation + check

**Input (exact prompt to the agent):**

> Read `documentation/{service}UiObservableBehaviour.md` and generate UI tests via
> /ui-test-gen for the page `{page}`: `{flow}`. Service = {service}.

**Must:**
- [ ] Live snapshot first: `dotnet run --project Scripts/Tools -- locator-dump <url>` — every locator from the snapshot, dated in the UI-OB
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
