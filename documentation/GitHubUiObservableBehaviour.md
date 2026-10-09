# Observable Behaviour — GitHub UI (Web)

**Project:** Gredja (.NET 10.0)
**Surface under test:** GitHub web UI (https://github.com) — sandbox repo `Gredja/AiTest`
**Date:** 2026-10-09
**Scope:** UI pages and flows (Playwright, Chromium)
**Test Plan:** [.mimocode/plans/ui-playwright-github.md](../.mimocode/plans/ui-playwright-github.md)

> Sections are added **per page** when locators are verified against a **live snapshot**
> (workflow: `.mimocode/skills/ui-test-gen` → Step 1.2 / Step 5).
> Every locator bullet carries a `verified {date}` marker — undated claims are drafts.

---

## What AI does

When generating or reviewing GitHub UI tests, the AI agent:

1. **Reads this document first** — before any code generation, loads the relevant page section
2. **Re-snapshots the page live** — never trusts stale selectors; GitHub markup changes between releases
3. **Writes role/text/label locators only** — from the snapshot's accessibility tree, never CSS-classes
4. **Puts actions in page-objects** — tests orchestrate and assert; they never contain raw locators
5. **Cross-verifies writes via API** — UI creates, Core API reads back as the oracle (Core `RequestHelper`)
6. **Cleans via API in teardown** — register-then-assert, `[OneTimeTearDown]` last member, warning-only cleanup
7. **Never logs in/out inside tests** — storageState only; login = `[Explicit]` bootstrap
8. **Never invents text** — button/label texts come from snapshots; a missing text = re-snapshot, not guess

---

## Page: Repository landing

- Status: **draft — no verified locators yet** (scaffold 2026-10-09)

## Page: Issues list

- Status: **draft — no verified locators yet**

## Page: Issue detail

- Status: **draft — no verified locators yet**

## Page: Issue create form

- Status: **draft — requires authenticated storageState**

---

## Gotchas (verified as they are discovered)

- Login wall: anonymous writers are redirected to `/login` — auth-dependent sections must document their storageState prerequisite
- (empty)

---

## Authoring Method

- **Structure + locators:** AI-drafted from live Playwright snapshots, dated per verification
- **Flows + cleanup:** human-reviewed against sandbox rules (`Rules/ui-testing.md`)
- **Risk framing:** human-owned
