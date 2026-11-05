---
name: update-docs
description: Use when the user says "update-docs", "/update-docs", or wants to update project documentation files after structural/code changes.
---

# Update Docs Agent for Gredja

Update project documentation files after structural or code changes. Ensures all .md files are in sync with actual project state.

## Step 1: Detect changes

Run `git diff --name-only` and `git status --short` to identify:
- New files added
- Files renamed/deleted
- Files modified
- Untracked files

## Step 2: Plan gate (mandatory)

Present a table of planned documentation edits (`file → what changes`) and STOP — wait for user approval before touching any file. Requested changes → update the plan, re-present, then apply.

Constraints in force at every edit:
- Never remove content that hasn't been deleted from the project
- Every file path you write must be verified against disk first (Glob) — no guessed paths

## Step 3: Update FILE_STRUCTURE.md

Read `documentation/FILE_STRUCTURE.md` and compare with actual file system:

1. **New files** — add to appropriate section in FILE_STRUCTURE.md
2. **Deleted files** — remove from FILE_STRUCTURE.md
3. **Renamed files** — update name in FILE_STRUCTURE.md
4. **New directories** — add directory structure

Checklist:
- [ ] `Core/Models/` — new model files?
- [ ] `Core/Config/` — new endpoint files?
- [ ] `Core/Generators/` — new generator files?
- [ ] `Core/Helpers/` — new helper files?
- [ ] `Api/` — new test files?
- [ ] `E2E/` — new test files?
- [ ] `Ui/` — new pages/tests/support files?
- [ ] `.mimocode/skills/` — new skills?
- [ ] `Scripts/` — new scripts?
- [ ] `Rules/` — new rule files?
- [ ] `documentation/` — new doc files?

## Step 4: Update README.md

Read `README.md` (repository root) and verify:

1. **Technologies** — new packages added? Version bumps?
2. **APIs Under Test** — new endpoints/services?
3. **Test Structure** — new directories or files?
4. **Running Tests** — new commands?
5. **Documentation** — new doc files?
6. **Coverage** — new coverage tools/commands?

## Step 5: Update backups/SETUP.md

Read `backups/SETUP.md` and verify:

1. **Skills** — new skills added?
2. **Rules** — new rules or changed rules?
3. **Project structure** — new directories?
4. **Observable Behaviour** — new docs?
5. **Config** — new endpoints?
6. **AGENTS.md** or **README.md** — changes?

## Step 6: Update other md files

Check and update if needed:

- `Rules/*.md` — if code/rules changed (includes `Rules/localization-testing.md` for UI/locale changes)
- `documentation/*ObservableBehaviour.md` — if endpoints changed
- `documentation/*TestPlan.md` — if test coverage changed
- `AGENTS.md` — if project rules changed
- `education/TESTING_PLAYBOOK.md` — if test rules, generation skills, gates, categories, or templates changed (portable delta only)

## Step 7: Verify (external — not delegated to the user)

Before reporting done:

1. Glob every file path newly written into the changed `.md` files — each must exist on disk (or be a documented on-demand path)
2. Re-read `git diff` of the changed `.md` files — no stale claims, nothing removed that still exists in the project
3. Report the verification result (`paths checked: N, all exist`) as part of the report — a "manually verify" checklist item delegated to the user is NOT acceptable

## Step 8: Report

Report what was updated AND the Step 7 verification result:

```
## Documentation Updated

| File | Changes |
|------|---------|
| FILE_STRUCTURE.md | Added coverage/SKILL.md, test-coverage.ps1 |
| README.md | Updated FluentAssertions version, added coverage section |

## Verification
- paths checked: N — all exist
- git diff re-read — no stale claims
```

---

## Rules

- Run `git diff --name-only` first to detect actual changes
- Plan gate first (Step 2) — no edits before approval (Rules/workflow.md)
- Never remove content that hasn't been deleted from the project
- Keep file paths accurate — verify before writing
- Update backups/SETUP.md whenever skills, rules, or structure change
- Check for new model naming conventions (ModelResponse/ModelRequest)
- Verify endpoint counts in coverage tables
- If unsure about a change — ask user, don't guess
