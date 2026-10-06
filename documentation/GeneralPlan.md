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

---

## 3. Un-ignore FakeStore tests after API recovery — TODO

**Wish (2026-10-06):** when fakestoreapi.com recovers from the HTTP 521 outage, remove the
fixture-level `[Ignore("FakeStoreAPI: service under investigation — outage HTTP 521 ...")]`
from all 9 FakeStore test classes (`Api/FakeStore/Tests/**`), verify the API actually works
(direct probe, then full test run), and re-enable the whole FakeStore suite. Until then the
service stays fully ignored with the marker in the attributes.

**Status history:**
- 2026-10-06 — `TODO`: added to backlog — FakeStore tests ignored during HTTP 521 outage
  (commits `854ccba`); tracked ONLY in this backlog (per user decision, no session task)
