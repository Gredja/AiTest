# General Plan — Gredja

Permanent backlog of the user's wishes for the whole project. This file is NEVER deleted
and NEVER trimmed — completed items stay here with the `DONE` status.
Only the user's wishes belong here (wish text + status); implementation details live in
`documentation/FILE_STRUCTURE.md`, `AGENTS.md`, commits and `.mimocode/plans/`.

**Status legend:** `TODO` · `IN PROGRESS` · `DONE`

**Rules:**
- Never remove an item — only change its status
- New items are appended with the next number
- Status history for each item lives inside the item

---

## 1. Run report: one file per test run — DONE

**Wish (2026-10-02):** every test run — even a single test, and a full suite run too —
produces exactly ONE report file in tabular form: per-test results plus an overall
summary (counts and percentages), without any agent involvement. Email sending was
explicitly declined by the user ("давай без отправки на почту") — the report file is
the only deliverable.

**Status history:**
- 2026-10-02 — `IN PROGRESS`: plan approved, implementation started
- 2026-10-02 — `DONE`: report file delivered and verified

---

## 2. Research: is CI/CD needed for this project? — TODO

**Wish (2026-10-02):** investigate whether CI/CD is needed for this project at all —
if yes, what it would look like (trigger, gates, artifacts); if no, document why not.
Research/decision first, implementation only if the answer is "yes".

**Status history:**
- 2026-10-02 — `TODO`: added to backlog by user decision (research first, no implementation yet)
