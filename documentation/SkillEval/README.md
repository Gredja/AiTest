# SkillEval — eval sets for generation skills

An eval set is a fixed input + a must/must-not checklist for one skill. It turns a skill
edit into a testable hypothesis: run before → edit → run after → status table, rather than
"it felt better".

Same principle as mutation testing vs line coverage: not "is there a check", but "does the
check feel the error" (see `education/TESTING_PLAYBOOK.md` §14.9).

## Rule (mandatory)

**Any edit to `.mimocode/skills/api-test-gen|e2e-test-gen|ui-test-gen/SKILL.md` → run that
skill's eval set before committing.** Gate in `/commit`: without an eval table in the report,
a commit touching `*test-gen*` is blocked.

## Case format

One file per skill (`api-test-gen.md`, `e2e-test-gen.md`, `ui-test-gen.md`).
Each case:

- **Input** — the exact prompt given to the agent (service/endpoint + context);
- **Must** — checklist of items that MUST be present in the response (from the skill);
- **Must not** — checklist of things that must NOT appear in the response;
- **Score** — `passed must / total must` + `passed must-not / total must-not` (e.g. `9/9 must, 5/5 must-not`);
  a case is `pass` only at full score, a partial score is recorded in run notes with the missed items —
  the number is what makes before/after comparable, not vibes;
- **Status before / after** — `not run` | `pass` | `fail` (+ run date).

## How to run

1. Record the skill version (git hash) in the "before" column.
2. Give the agent the case INPUT only — never the checklist (the runner sees the checklist,
   the generator must not). **Explicitly forbid the agent from reading anything under
   `documentation/SkillEval/` during the run** (checklists, run notes, status tables all
   live there) — a prompt that only omits the checklist is not enough: the agent can find
   and read these files on its own, which invalidates the run (observed 2026-11-10,
   M2 Day 5). The skill under test (`.mimocode/skills/<skill>/SKILL.md` and its
   `references/`) MUST be read — that is the test subject. If the agent reports having read
   `documentation/SkillEval/` anyway, mark the run contaminated and redo it with the ban
   stated in the prompt.
   **Research budget in the same prompt:** deliver the plan (or refusal) within ~12–15
   tool calls — target-read OB sections by heading (never the whole file unless the case
   requires it), touch only the endpoint's own test file/config for the "existing code"
   step, no repo-wide exploratory greps beyond what the skill mandates. Mandatory skill
   readings (SKILL.md, references, the Rules set at code stage) do not count against the
   spirit of the budget — trim everything above them. Over budget without a plan → runner
   nudges once, then cancels and records the run as `cancelled (budget)`.
3. Check the response against the Must / Must-not checklist by eye; validate the report JSON
   additionally via `powershell -File Scripts/validate-skill-report.ps1 <file>`
   (this is the only mechanical point of the run; everything else is the checklist).
4. Fill in the statuses, add the date and the skill version.
5. After a skill edit, repeat steps 2–4 in the "after" column for **every case of that
   skill — not only the case that motivated the edit** (regression rule: a fix for one case
   can break another; a full-set rerun is the only way to see it). Compare the tables.

## Status as of 2026-11-05

Starting baseline: 4 cases, all `not run`. Expansion to 5–7 cases and skill edits driven by
the findings are scheduled for M2 Day 6 (learning plan, `.mimocode/plans/ai-learning-plan.md`,
gitignored personal copy).
