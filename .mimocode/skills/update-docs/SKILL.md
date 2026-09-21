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

## Step 2: Update FILE_STRUCTURE.md

Read `documentation/FILE_STRUCTURE.md` and compare with actual file system:

1. **New files** — add to appropriate section in FILE_STRUCTURE.md
2. **Deleted files** — remove from FILE_STRUCTURE.md
3. **Renamed files** — update name in FILE_STRUCTURE.md
4. **New directories** — add directory structure

Checklist:
- [ ] `Core/Models/` — new model files?
- [ ] `Core/Config/` — new endpoint files?
- [ ] `Core/Helpers/` — new helper files?
- [ ] `Api/` — new test files?
- [ ] `E2E/` — new test files?
- [ ] `.mimocode/skills/` — new skills?
- [ ] `Scripts/` — new scripts?
- [ ] `Rules/` — new rule files?
- [ ] `documentation/` — new doc files?

## Step 3: Update README.md

Read `documentation/README.md` and verify:

1. **Technologies** — new packages added? Version bumps?
2. **APIs Under Test** — new endpoints/services?
3. **Test Structure** — new directories or files?
4. **Running Tests** — new commands?
5. **Documentation** — new doc files?
6. **Coverage** — new coverage tools/commands?

## Step 4: Update backups/SETUP.md

Read `backups/SETUP.md` and verify:

1. **Skills** — new skills added?
2. **Rules** — new rules or changed rules?
3. **Project structure** — new directories?
4. **Observable Behaviour** — new docs?
5. **Config** — new endpoints?
6. **AGENTS.md** or **README.md** — changes?

## Step 5: Update other md files

Check and update if needed:

- `Rules/*.md` — if code/rules changed
- `documentation/*ObservableBehaviour.md` — if endpoints changed
- `documentation/*TestPlan.md` — if test coverage changed
- `AGENTS.md` — if project rules changed

## Step 6: Report

Report what was updated:

```
## Documentation Updated

| File | Changes |
|------|---------|
| FILE_STRUCTURE.md | Added coverage/SKILL.md, test-coverage.ps1 |
| README.md | Updated FluentAssertions version, added coverage section |
| backups/SETUP.md | Added Guarantee Data pattern |
| Rules/test-practices.md | Added Guarantee Data section |

## Files to Verify
- [ ] FILE_STRUCTURE.md — manually verify new file locations
- [ ] README.md — manually verify tech stack and commands
```

---

## Rules

- Run `git diff --name-only` first to detect actual changes
- Never remove content that hasn't been deleted from the project
- Keep file paths accurate — verify before writing
- Update backups/SETUP.md whenever skills, rules, or structure change
- Check for new model naming conventions (ModelResponse/ModelRequest)
- Verify endpoint counts in coverage tables
- If unsure about a change — ask user, don't guess
