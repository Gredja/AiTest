---
name: structure-audit
description: Read-only audit of the project file structure — checks that every folder name matches its contents (naming first, then common sense, then future plans), finds plural drift, duplicated segments, misplaced files and root junk, and outputs a findings table with move/rename proposals. Use when the user says "/structure-audit", "проверь структуру", "соответствуют ли папки содержимому", "проверь расположение файлов", "нейминг структуры".
---

# Structure Audit

Read-only review of where files live. Reports only — moves happen through `/audit` (Stage 2, gated batches)
after the user approves the proposals.

## Instructions

### Step 1: Inventory

```powershell
foreach ($proj in @('Api','Core','E2E','Ui','AllureAdapter')) {
  Get-ChildItem $proj -Recurse -File -Include *.cs,*.csproj |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
    ForEach-Object { $_.FullName.Replace("$PWD\$proj\", '') }
}
Get-ChildItem -Directory | Where-Object { $_.Name -notmatch '^\.' }
Get-ChildItem -File | ForEach-Object Name
```

Also list `.mimocode/skills/`, `Scripts/`, `documentation/`, `Rules/`, tracked root files
(`git ls-files` root level) vs ignored (`git check-ignore`).

### Step 2: Naming check (primary criterion)

For EVERY folder/file ask: **does the name equal the content?**

- `Helpers/` must hold helpers only — a test base class or page-object in it = mismatch
- `Tests/`/`Test/` — plural consistency across projects
- `Pages/`/`Forms/` — page classes only; locators live there, never in helpers
- Duplicated segments (`Ui/Helper/UiHelper/`) — double naming from naive folder→namespace mapping
- Namespace ↔ path: file-scoped namespace should mirror the folder (exceptions must be
  deliberate, note them)
- File name ↔ type name (one class per file)
- Boolean/naming rules of `Rules/code.md` are OUT of scope here — this audit is placement only

### Step 3: Common-sense & future-plans check (secondary criteria)

- Layer violations: models with logic, tests inside Core, infra mixed with fixtures,
  script output tracked in git
- Root hygiene: only project artifacts (configs, AGENTS, README, solution) — flag tracked
  files/dirs that are docs-in-disguise, caches, logs (compare `git ls-files` vs `git check-ignore`)
- Dead/empty folders (`.gitkeep`-only dirs), files with no consumers (grep usage before claiming)
- Will the placement still make sense when the project grows? (the file-placement rule)

### Step 4: Report table

Output ONLY this shape (no code changes!):

```
| # | Path | Имя говорит | Реально внутри | Вердикт | Предложение |
|---|------|-------------|----------------|---------|-------------|
| 1 | Ui/Helper/ | хелперы | база тестов + page-objects | MISMATCH | Pages/ → Tests/Support/ |
```

Verdicts: `OK` (no row needed — summarize count instead) | `RENAME` | `MOVE` | `SPLIT` |
`DELETE` (dead folder) | `UNTRACK` (root junk → gitignore/remove).

End with: totals per verdict + suggested execution order (as fix batches for the `/audit` fix plan).

## Important

- **READ-ONLY** — no `Move-Item`, no edits, no commits; the user routes approved
  proposals into the `/audit` fix plan.
- **Never include `documentation/Katas/`** — training material, out of scope entirely.
- If unsure whether a file has consumers — grep before asserting; mark as "verify" instead of guessing.

## Examples

User says "проверь структуру" → inventory → table (`Ui/Test` vs `Api/.../Tests` = RENAME,
`metrics.md` в корне = MOVE в documentation/Katas/) → "решения нужны по строкам N…".

## Troubleshooting

- Output too large → group by project, keep one row per real mismatch, not per file.
- Ambiguous verdict → write both options in Предложение with the tradeoff in one clause.
