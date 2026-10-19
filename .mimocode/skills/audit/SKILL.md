---
name: audit
description: Use when the user says "audit", "/audit", "аудит", "рефакторинг"/"refactor" for whole-codebase work, or wants code improved against Rules/*.md. One skill, two stages: (1) audit the entire codebase against Rules/*.md and produce a detailed fix plan — a human-readable table with explanations and statuses, saving it to .mimocode/plans/audit-fixes-<date>.md — fixes are NOT applied immediately, they happen later on the user's signal; (2) apply the approved plan in gated batches (build/format/HealthCheck/touched suites after each batch, one /commit per batch, structural moves with docs sync) and update plan statuses. NOT for uncommitted-changes review (/review-commit) or PR reviews (/review-pr).
---

# Skill: Full Code Audit & Refactor

Two stages, explicitly separated in time:

1. **Диагностика (this invocation):** audit the whole codebase against `Rules/*.md` → detailed
   fix plan with explanations and statuses → present the table → STOP (no fixes unless the
   user explicitly says "чини сейчас").
2. **Лечение (later, on the user's signal — same or another session):** read the plan file,
   apply pending items in gated batches through `/commit`, update statuses, close the plan.

The plan file is the cross-session state: it must contain everything a future session needs.

---

## Stage 1 — Audit

### Step 1: Load rules (main agent)

Read all files in `Rules/` directory. These are the authoritative rules for the audit.

### Step 2: Collect files (main agent)

```powershell
Get-ChildItem -Recurse -Include "*.cs" | Where-Object { $_.FullName -notmatch "\\obj\\" } | Select-Object -ExpandProperty FullName
```

**Exclusions (never touched in later fixes either):** `documentation/Katas/` (training,
read-only), `bin/`, `obj/`, `.mimocode/node_modules/`, generated files.

### Step 3: Audit by category (main agent + parallel `explore` subagents)

Group findings by category; each subagent reads its own `Rules/*.md` files IN FULL first,
then scans ALL `.cs` (grep-driven whole-repo coverage is fine, sampling is not).

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

Each subagent returns a table:
```
| Severity | File:Line | Rule | Current | Suggested |
```
Severity: `major` (rule violation, bug risk) | `minor` (style, suboptimal) | `style` (cosmetic).

**Standard exclusions for subagents:** `AllureTestResultBuilder` dictionary keys (Allure JSON
spec); `[Category("...")]` values are enum-like; fallback values (`"Tests"`, `"Unknown"`) stay
inline; test data literals inside tests are expected.

### Step 4: Detailed fix plan — the deliverable of Stage 1 (main agent)

**Never start fixing in this stage** (unless the user explicitly says "чини сейчас" — then go
straight to Stage 2 after presenting the plan). Create `.mimocode/plans/audit-fixes-<YYYY-MM-DD>.md`:

1. Header: date, scope (files reviewed), totals (X major / Y minor / Z style), source branch.
2. **Batches (my chosen fix order):** group items into fix batches ordered from
   independent/safe to broad: comments-purge → Core → Api → E2E → Ui → adapters →
   structure-moves (if any). Each batch = one commit-worth of coherent change.
3. **Per-item table** (this is what the user reads — explanations in human language):
   ```
   | # | Sev | File:Line | Что не так (простыми словами) | Что сделаю / Почему SKIP | Батч | Статус |
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
link the file, and ask two things: scope (all / major only / specific numbers) and **when to
fix** (сейчас / позже — время фикса всегда решает пользователь). Then STOP.

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

**Gates:** build clean · format pass · HealthCheck X/X · touched suites: <numbers>
**Commits:** <batch hashes>
**Plan file:** deleted (all batches closed) / open items remain: <statuses>
```

Delete the plan file when every item is `done`/`skipped`; keep it (with statuses) while anything
is open. Also delete any completed structural-refactor plan files from `.mimocode/plans/` when
their done-criteria are met.

---

## Rules (HARD)

- Audit ALL .cs files, not a sample; line references must be exact (`file:line`)
- Stage 1 never fixes (unless the user says so) — the deliverable is the plan-table
- No fixes without the user's approved scope — an approved plan file counts as that approval
  for its listed items; later sessions re-confirm scope before mass-fixing
- Gates after every batch: `build` + `format --verify-no-changes` + `HealthCheck` + touched suites
- Commit per batch ONLY through `/commit`; if a fix breaks the build — revert and report
- SKIP items need an explicit reason in the plan — no silent drops
- New rules → `Rules/*.md` + `AGENTS.md` mirror before committing them
- PR/CI waits → background watcher subagent, never a blocking loop
- Exclusions never touched: `documentation/Katas/`, `bin/`, `obj/`, `.mimocode/node_modules/`, generated
- Group related fixes per batch for efficient application; batch = one coherent commit
