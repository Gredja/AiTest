---
name: refactor
description: Phased full-project refactor against Rules/*.md — structure/naming moves, comment purge, per-project code-style sweeps, each phase gated and committed only through /commit, ending with a report and plan cleanup. Use when the user says "/refactor", "полный рефакторинг", "сделай рефакторинг", "рефакторинг всех проектов", or asks to clean up the whole codebase by the project rules.
---

# Refactor

Phased refactor of the whole solution. Never skip a gate; never commit outside `/commit`.

## Instructions

### Step 1: Plan file first

Create `.mimocode/plans/refactor-<topic>.md` (file-placement rule: plans live only in
`.mimocode/plans/`) with phases, statuses and the fixed exclusion list:

- **Exclusions (never touched):** `documentation/Katas/` (training, read-only), `bin/`, `obj/`,
  `.mimocode/node_modules/`, generated files.
- Show the plan to the user; if the user grants autonomy — mark it in the plan and stop asking
  until an exceptional case (destructive/ambiguous decision).

### Step 2: R-phase — structure & naming

Order: **naming first, then common sense, then future plans** (the file-placement rule in
`Rules/code.md` → File Layout).

- One structural move per commit-worth step: folder rename/move (`Move-Item`), namespace
  rewrite in the moved files (`Ui.Helper.X` → `Ui.X`), usings repo-wide, then docs sync in the
  SAME flow: `documentation/FILE_STRUCTURE.md`, `AGENTS.md`, `Rules/*` references, skill path
  tables (`ui-test-gen`), tracked `backups/SETUP.md`.
- Inconsistencies to hunt: plural drift (`Test` vs `Tests`), duplicated path segments
  (`Helper/UiHelper`), semantic mismatches (a base class living in `Helpers/`), dead folders,
  root hygiene (tracked junk outside configs/docs/solution).
- Gates after every move: `dotnet build` → `dotnet format --verify-no-changes` → affected suite
  (e.g. `--filter "Category=GitHubUi"` for `Ui/` changes).

### Step 3: C0 — comment purge (all projects)

Apply `Rules/comments.md` decision procedure to every `*.cs`:

1. Delete the comment — code became unclear/dangerous? No → delete.
2. Fixable by code (reference a constant, better name)? Fix code, delete comment.
3. Undeniable WHY (concrete bug workaround, hidden constraint, danger) code cannot express →
   keep, as short as possible.

Never touch: string literals, FluentAssertions `because` messages, regex patterns (they keep
their explaining comment), `///`, pre-existing config comment blocks (`.runsettings`).
Deliverable: one commit `Remove excess code comments` (after gates).

### Step 4: Per-project code sweeps (C-Core, C-Api, C-E2E, C-Ui, C-AllureAdapter)

Fix against `Rules/code.md`, `Rules/code-style.md`, `Rules/code-principles.md`,
`Rules/models.md`, `Rules/assertions.md`, `Rules/test-practices.md`:

- `var` where legal; expression-bodied single-expression members; `Any()` over `Count() > 0`;
  `FirstOrDefault(pred)`; interpolation; `IsNullOrEmpty`; `is not null`; no nested ternaries;
  no magic numbers/strings (2+ same-meaning strings → const in the right home);
  `_camelCase` private fields; Boolean `Is/Has/Can/Should`; blank line before `return`;
  sync `List<T>` side-effect loops → `.ForEach`; no private helper methods inside `*Tests.cs`;
  status-assert before `.Data!`; `ShouldHaveError` where OB documents the message.

**Before deleting anything as "dead":** grep the `Rules/*.md` for its name — a rule-documented
utility is API, not dead code (`RandomIntExclusive` lesson).

Sweep order: spawn one read-only inventory subagent per sweep (or one for all) → apply fixes
yourself or via a general subagent with the exact adjudicated list → gates after each project:
`build` + `format` + `HealthCheck` + touched suites (`Category=GitHub` 376, `GitHubE2E` 9,
`GitHubUi` 14) → own commit per project.

### Step 5: Commit discipline (HARD)

- Every commit ONLY through the `/commit` skill (hard user rule) — gates, all-rules review,
  backups sync and `Scripts/normalize-eol.ps1` live inside it.
- Push feature branch → PR → wait for `gate` via a background watcher subagent (never
  sleep-poll in the main thread) → merge when green.
- EOL: `.cs/.ps1/.yml → CRLF`, `.md → LF` per `.gitattributes` — `git add` must show ZERO
  "LF will be replaced" warnings (the normalizer does LF→CRLF only; for `.md` write LF yourself).

### Step 6: Final report & cleanup

Report per phase: what changed, gates numbers, commits/PR, findings intentionally skipped
(with reasons). Delete completed plan files (`.mimocode/plans/`), keep open ones updated.

## Important

- One phase at a time; never start the next phase with a dirty tree or red gates.
- Wait/polling (PR gate, CI runs) → always a background subagent, never a blocking loop.
- New rules discovered mid-refactor → write them into `Rules/*.md` + mirror in `AGENTS.md`
  (workflow rule 7) before committing them.

## Troubleshooting

- Build broke after a move → missing `using` for the old parent namespace (C# resolves
  upward: `Ui.Pages.X` no longer sees `Ui.Helper` members) — add the using, rebuild.
- Tests fail after comment purge → you touched a string literal or an assertion `because`
  message — restore it verbatim.
- `git add` warnings after your own file writes → you wrote LF/CRLF against `.gitattributes`;
  normalize that extension before staging.
