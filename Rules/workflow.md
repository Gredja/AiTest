# Rules: Workflow

All changes require a plan first, then user approval, then execution.

1. Show the plan (what files, what changes)
2. Wait for user approval
3. Make changes
4. Show a brief report of what was done
5. After commit — review `Rules/` and suggest updates if needed
6. After structural changes — update `FILE_STRUCTURE.md`. Structural changes include
   REMOVALS of gitignored files/folders — update `FILE_STRUCTURE.md`/`AGENTS.md` in the
   same turn even without a commit trigger (gitignored junk leaves no commit to hook onto)
7. After changing any `Rules/*.md` — update root `AGENTS.md` to match
