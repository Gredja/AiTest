---
name: audit
description: Use when the user says "audit", "/audit", "аудит", "проверь структуру", "/structure-audit", "нейминг структуры", "рефакторинг"/"refactor" for whole-codebase work, or wants code improved against Rules/*.md. One skill, two stages: (1) audit the entire codebase against Rules/*.md (categories A–F + structure) and produce a detailed fix plan — a human-readable table with explanations and statuses, saving it to .mimocode/plans/audit-fixes-YYYY-MM-DD.md — fixes are NOT applied immediately, they happen later on the user's signal; (2) apply the approved plan in gated batches (build/format/HealthCheck/touched suites after each batch, one /commit per batch, structural moves with docs sync) and update plan statuses. NOT for uncommitted-changes review (/review-commit) or PR reviews (/review-pr).
---

# Skill: Full Code Audit & Refactor

Two stages, explicitly separated in time:

1. **Diagnosis (this invocation):** audit the whole codebase against `Rules/*.md` → detailed
   fix plan with explanations and statuses → present the table → STOP. **The audit NEVER fixes
   anything in the same run** — plan only; fixes happen in a separate, later invocation
   (Stage 2) on an explicit user signal.
2. **Treatment (later, on the user's signal — same or another session):** read the plan file,
   apply pending items in gated batches through `/commit`, update statuses, close the plan.

The plan file is the cross-session state: it must contain everything a future session needs.

---

## Stage 1 — Audit

### Step 1: Load rules (main agent)

Read ALL files in `Rules/` **in full** — these are the user's authoritative rules and the
primary audit source. Two check layers always run, never one without the other:

1. **User's rules layer** — every `Rules/*.md` is read before scanning; every finding that
   comes from a rule cites the exact file (and section when the rule has headings).
2. **Agent's own checks layer** — correctness bugs, race conditions, error-handling smells,
   data-loss risks, API-misuse and anything else the written rules do not cover — reported
   with `agent-check (<one-line why>)` in place of the rule reference.

A finding may come from either layer; an audit that only re-lists rule violations or only
uses the agent's judgment is incomplete.

### Step 2: Collect files (main agent)

```powershell
Get-ChildItem -Recurse -Include "*.cs" | Where-Object { $_.FullName -notmatch "\\obj\\" } | Select-Object -ExpandProperty FullName
```

**Exclusions (never touched in later fixes either):** `documentation/Katas/` (training,
read-only), `bin/`, `obj/`, `.mimocode/node_modules/`, generated files.

### Step 2b: Roslyn analyzer pass (main agent) — always runs with Steps 1–2

The repo ships an always-on Roslyn layer (`Roslynator.Analyzers` in `Directory.Build.props`,
curated set in `.editorconfig`):

```powershell
dotnet build --verbosity normal --no-incremental   # collect every `warning RCS*/IDE*`
dotnet format analyzers --verify-no-changes        # exit != 0 → analyzer violations exist
```

- Every diagnostic becomes a normal audit finding in the plan: exact `File:Line`, `Rule` column
  = the diagnostic id (`RCS1213` etc.) plus which `Rules/*.md` clause it enforces; the
  remediation column follows the id's code-fix.
- Zero diagnostics → report a clean analyzer pass in the report (one line).
- A curated-set gap (a rule class the set should cover but does not) is itself a finding
  (suggestion to add `dotnet_diagnostic.<id>` to `.editorconfig` — Stage 2, with approval).

### Step 3: Audit by category (main agent + parallel `explore` subagents)

Group findings by category; each subagent reads its own `Rules/*.md` files IN FULL first
(user's rules layer), then scans ALL `.cs` (grep-driven whole-repo coverage is fine, sampling
is not), additionally applying its own correctness/concurrency/security judgment
(agent's checks layer — report those with `agent-check (why)` in the Rule column).

#### Category A: Naming & Types (explore subagent)
`Rules/code.md`: PascalCase/camelCase/_camelCase; no abbreviations; Boolean prefix
Is/Has/Can/Should; file-scoped namespaces; one class per file; `var`; lambda params singular nouns.

#### Category B: Code Style (explore subagent)
`Rules/code-style.md`: specific exceptions, no exception flow control; `Any()` over
`Count() > 0`; `FirstOrDefault(pred)`; interpolation/StringBuilder/`IsNullOrEmpty`; magic
strings (2+ same-meaning → const); null safety (`is not null`, no `null!`); pattern matching.

#### Category C: Code Principles (explore subagent)
`Rules/code-principles.md`: guard clauses; expression-bodied single-expression members; no
magic numbers; no nested ternaries; braces; blank line before return; SOLID points as written.

#### Category D: Models & Config (explore subagent)
`Rules/models.md` + `Rules/config.md`: ModelResponse/ModelRequest suffixes; nullable rules;
plain containers; JSON names only via `JsonFields`; endpoints only in `Core/Config/*Endpoints.cs`
(no hardcoded URLs in tests).

#### Category E: Tests & Assertions (explore subagent)
`Rules/assertions.md` + `Rules/test-practices.md` + `Rules/categories.md` + `Rules/ui-testing.md`:
FluentAssertions only; isolation + register-then-assert + teardown-last; category boundaries;
`[Description]` ↔ OB sections; non-existent ID pattern; locators only in Page/Form; waits
post-condition; `ShouldHaveError` where OB documents the message.

#### Category F: Comments (explore subagent)
`Rules/comments.md`: zero-default decision procedure; regex comments mandatory; no
WHAT-comments/AAA/dividers; no TODO/FIXME in committed code; provenance notes belong to OB
(measured-dated WHY comments are sanctioned — do not flag them).

#### Category R: Structure & placement (explore subagent — read-only, absorbed `/structure-audit`)
Placement only — naming of CODE symbols is Category A's job. Inventory first:
```powershell
foreach ($proj in @('Api','Core','E2E','Ui','AllureAdapter')) {
  Get-ChildItem $proj -Recurse -File -Include *.cs,*.csproj |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
    ForEach-Object { $_.FullName.Replace("$PWD\$proj\", '') }
}
Get-ChildItem -Directory | Where-Object { $_.Name -notmatch '^\.' }
Get-ChildItem -File | ForEach-Object Name
# plus .mimocode/skills/, Scripts/, documentation/, Rules/; tracked root (git ls-files) vs ignored (git check-ignore)
```
Then check for EVERY folder/file:
- **Does the name equal the content?** (`Helpers/` must hold helpers only; plural consistency
  `Tests/` vs `Test/`; `Pages/` = page classes only; duplicated path segments like `Ui/Helper/UiHelper/`)
- Namespace ↔ folder and file name ↔ type name (one class per file)
- Layer violations (models with logic, tests inside Core, infra mixed with fixtures)
- Root hygiene: tracked junk/caches/logs/docs-in-disguise (git ls-files vs check-ignore)
- Dead/empty folders (`.gitkeep`-only); files with no consumers (grep usage before claiming)
- Future-proof placement per `Rules/code.md` → File Layout

Structure findings go into the same fix plan with move/rename proposals (verdicts:
`RENAME` / `MOVE` / `SPLIT` / `DELETE` / `UNTRACK`); the actual moves are executed in Stage 2
as their own batch — one move per flow + docs sync + gates (Step 5).

#### Category S: Security & log hygiene (explore subagent)
Secrets must never be born or printed: hardcoded tokens/passwords/keys in code or tracked
configs; `Authorization`/`Token`/cookie values leaking into log lines (request/response body
logging, exception messages, `Console`/Serilog output); `.env`-like files tracked by git;
secrets interpolated into `[Description]`/test data; test logs under `%TEMP%`/`TestResults`
that would contain credentials if a test fails. Evidence: `grep` the token prefixes
(`ghp_`, `github_pat_`, `Bearer `) and known env names (`GITHUB_PAT`, `GITHUB_UI_*`, passwords)
outside sanctioned config readers.

#### Category Y: Async & concurrency (explore subagent)
Blocking over async (`.Result`, `.Wait()`, `.WaitAndRetry`-style sync waits on Tasks);
`async void` methods (except event handlers the framework requires); discarded/`_`-elided
Tasks with side effects (fire-and-forget); shared mutable static state without synchronization
(dictionary/list mutated from parallel tests, non-thread-safe `StringBuilder`/file writes across
tests); `lock` bodies doing I/O for the whole operation.

#### Category Z: Determinism & rerunnability (explore subagent)
Culture/timezone dependence (`DateTime.Now` vs injectable clock in asserts, `ToString()`
without invariant culture in comparisons, calendar/date asserts without fixed input);
test-order dependence (shared static counters, mutate-then-read global fixtures); re-run
state (what survives a failed cleanup, what the next run assumes fresh); fixed sleeps without
measured WHY; reliance on external services beyond the sanctioned sandbox.

#### Category W: Document sync — code ↔ docs (explore subagent)
The document-sync rule made executable: config endpoints ↔ `*ObservableBehaviour.md`;
`[Description]` ↔ existing OB sections (UI-N/E2E-N); behavior that Rules/* prescribe ↔ what
the code actually does (stale rule wording vs live behavior — e.g. counter formats that
changed); OB claims with `verified {date}` older than the code that contradicts them.

#### Category U: Dead code & unused members (explore subagent)
Private fields/methods/constants with zero callers (grep each candidate — no guessing);
unused test helpers and page-object methods; orphaned endpoints/models no test touches;
stale `#if`/conditional branches. **Before reporting anything as dead: grep `Rules/*.md` —
a rule-documented utility is API, not dead code** (`RandomIntExclusive` lesson).

### Execution map — who does what (fill this in the report)

| Check | Owner |
|----------|-------|
|----------|---------------------|
| A–F, S, Y, Z, W, U, R | one `explore` subagent per category (parallel) |
| Roslyn analyzer pass (Step 2b: build RCS/IDE inventory + `dotnet format analyzers`) | main agent — always-on tooling layer feeding plan rows |
| Plan roll-up, statuses, tables | main agent (Step 4) |
| Fixes (Stage 2) | main agent, batches → gates → `/commit` |

Each subagent tags its rows with the category — the main agent rolls them into plan batches
by owning area (Core/Api/Ui/adapters/infrastructure).

Each subagent returns a table:
```
| Severity | File:Line | Rule | Current | Suggested |
```
`Rule` = `Rules/<file>.md` (+ section) for user-rule findings, or `agent-check (<why>)` for
the agent's own findings — both layers are always checked (Step 1).
Severity: `major` (rule violation, bug risk) | `minor` (style, suboptimal) | `style` (cosmetic).

**Standard exclusions for subagents:** `AllureTestResultBuilder` dictionary keys (Allure JSON
spec); `[Category("...")]` values are enum-like; fallback values (`"Tests"`, `"Unknown"`) stay
inline; test data literals inside tests are expected.

### Step 4: Detailed fix plan — the deliverable of Stage 1 (main agent)

**No fixes in this stage — none at all.** The audit produces ONLY a plan.
Create `.mimocode/plans/audit-fixes-<YYYY-MM-DD>.md`:

1. Header: date, scope (files reviewed), totals (X major / Y minor / Z style), source branch.
2. **Batches (my chosen fix order):** group items into fix batches ordered from
   independent/safe to broad: comments-purge → Core → Api → E2E → Ui → adapters →
   structure-moves (if any). Each batch = one commit-worth of coherent change.
3. **Per-item table** (this is what the user reads — explanations in human language):
   ```
   | # | Sev | File:Line | What is wrong (plain words) | What I will do / Why SKIP | Batch | Status |
   ```
   Status vocabulary: `pending` → `in progress` → `done` / `skipped (reason)`.
4. **SKIP section:** findings deliberately not fixed, each with a concrete reason
   (accepted debt, churn worse than the smell, rule scope nuance) — surfaced to the user,
   never silently dropped.
5. Optional: structural/refactor-phase notes (moves need `Move-Item` → namespaces/usings →
   docs sync `FILE_STRUCTURE.md`, `AGENTS.md`, `Rules/*`, `backups/SETUP.md` in the same flow).
6. Rules found mid-audit that don't exist yet → write into `Rules/*.md` + mirror in `AGENTS.md`
   BEFORE they are treated as binding (workflow rule).

**Present the plan as a table in chat** (batch summary + counts + top findings in plain words),
link the file, ask the desired scope (all / major only / specific numbers) and remind that
fixing happens in a later, separate invocation (Stage 2). Then STOP — zero code edits.

---

## Stage 2 — Fix (separate invocation, on the user's signal)

Trigger: user says "чини по плану аудита", "fix audit findings", etc. Read the plan file —
statuses are the persistent state.

### Step 5: Apply fixes in gated batches (main agent)

1. For each batch (in plan order): apply only `pending` items marked for the approved scope.
2. **Gates after EVERY batch** — never skip:
   ```bash
   dotnet build --verbosity quiet
   dotnet format --verify-no-changes
   dotnet format analyzers --verify-no-changes
   dotnet test --verbosity minimal --filter Category=HealthCheck
   # plus suites the batch touched:
   #   Api/GitHub → --filter "Category=GitHub&Category!=GitHubUi&Category!=GitHubE2E"
   #   Ui         → --filter Category=GitHubUi
   #   E2E        → --filter Category=GitHubE2E
   ```
   Red gate → fix or revert the batch's last change; never proceed on red.
3. **Commit per batch ONLY through the `/commit` skill** (message approval, all-rules review,
   backups/education sync, `Scripts/normalize-eol.ps1`, push). Never `git commit` directly.
   EOL: `.cs/.ps1/.yml → CRLF`, `.md → LF`; `git add` must show zero "LF will be replaced" warnings.
4. Structural moves: one move per flow + docs sync in the same flow + gates, still `/commit`.
5. After each batch: update the plan file statuses (`done` / `skipped`) — the file is the
   cross-session source of truth.
6. One phase at a time; never start the next batch with a dirty tree or red gates.
7. If a rule discovered mid-fix needs writing → `Rules/*.md` + `AGENTS.md` mirror BEFORE the commit.

Optional final step (only if the user asks): push branch → PR → wait for CI gate via a
**background watcher subagent** (never a blocking sleep-poll in the main thread) → merge when green.

### Step 6: Report & close

```
# Audit Fixes Complete

| Batch | Fixed | Skipped (reason) |
|-------|-------|------------------|
| Comments | N | ... |
| ... | | |

**Gates:** build clean · format pass · analyzers pass · HealthCheck X/X · touched suites: <numbers>
**Commits:** <batch hashes>
**Plan file:** deleted (all batches closed) / open items remain: <statuses>
```

Delete the plan file when every item is `done`/`skipped`; keep it (with statuses) while anything
is open. Also delete any completed structural-refactor plan files from `.mimocode/plans/` when
their done-criteria are met.

---

## Rules (HARD)

- Audit ALL .cs files, not a sample; line references must be exact (`file:line`)
- **Both check layers always run**: the user's `Rules/*.md` (read in full, findings cite the
  rule) PLUS the agent's own checks (correctness/concurrency/security judgment beyond the
  written rules, findings cite `agent-check (why)`) — never one layer without the other
- Stage 1 NEVER fixes — not even on a casual "давай сразу" — the deliverable is the plan-table
  only; fixing is always a separate Stage-2 invocation on an explicit user signal
- No fixes without the user's approved scope — an approved plan file counts as that approval
  for its listed items; later sessions re-confirm scope before mass-fixing
- Gates after every batch: `build` + `format --verify-no-changes` + `analyzers --verify-no-changes` + `HealthCheck` + touched suites
- Commit per batch ONLY through `/commit`; if a fix breaks the build — revert and report
- SKIP items need an explicit reason in the plan — no silent drops
- New rules → `Rules/*.md` + `AGENTS.md` mirror before committing them
- PR/CI waits → background watcher subagent, never a blocking loop
- Exclusions never touched: `documentation/Katas/`, `bin/`, `obj/`, `.mimocode/node_modules/`, generated
- Group related fixes per batch for efficient application; batch = one coherent commit
