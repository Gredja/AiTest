# Observable Behaviour — GitHub UI (Web)

**Project:** Gredja (.NET 10.0)
**Surface under test:** GitHub web UI (https://github.com) — sandbox repo `Gredja/AiTest`
**Date:** 2026-10-09
**Scope:** UI pages and flows (Playwright, Chromium)
**Test plan history:** retired (lifecycle — plans deleted after implementation); durable status lives in this document and `Rules/ui-testing.md`

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
7. **Never logs in/out inside feature tests** — they load storageState; the dedicated Smoke test `LoginSignInTests` signs in anonymously and saves the state file
8. **Never invents text** — button/label texts come from snapshots; a missing text = re-snapshot, not guess

---

## Page: Login (UI-1)

- URL: `https://github.com/login` — verified 2026-10-09
- `GetByRole(Textbox, "Username or email address")`, `GetByRole(Textbox, "Password")` — verified 2026-10-09
- `GetByRole(Button, "Sign in")` (exact name) — verified 2026-10-09
- Success: redirect to `https://github.com/` (dashboard); header shows `GetByRole(Button, "Open user navigation menu")` — verified 2026-10-09
- User menu: click → `GetByRole(Dialog, "User navigation")`; `GetByRole(Heading)` text contains the signed-in username — verified 2026-10-09
- Prerequisites: credentials `GITHUB_UI_EMAIL` / `GITHUB_UI_PASSWORD` in `.env`; the login test overrides `IsStorageStateEnabled => false` (anonymous context) and saves storageState only after the assert
- Gotcha: an account created without a password (social login / passkey-only) fails with "This account does not support password sign-in" — fix: sign in via Google/Apple/passkey once, then Settings → Password and authentication → create a password (verified 2026-10-09)

## Page: Repository landing (ui-sandbox)

- URL: `https://github.com/gredjapl/ui-sandbox` — verified 2026-10-13
- `button "main branch"` (default-branch switcher, text `main`) — verified 2026-10-13
- `link "README.md, (File)"` in the file tree — verified 2026-10-13
- TestRepo path comes from `TestConfig.UiTestRepo` — never hardcode the repo in page-objects

## Page: Issues list (ui-sandbox, empty)

- URL: `.../issues` — verified 2026-10-13
- `heading "Issues" level=1` · `link "New issue"` — verified 2026-10-13
- Empty state: `heading "No results matched your search"` — verified 2026-10-13
- Tab counter renders `Open (0)` anonymous / `Open 0` authenticated — parse with `Open\D*(\d+)` regex

## Page: Issue create form (ui-sandbox)

- URL: `.../issues/new` (requires auth; anonymous → `/login?return_to=...`) — verified 2026-10-13
- `heading "Create new issue" level=1` — verified 2026-10-13
- `textbox "Add a title"` (placeholder `Title`, aria-required) — verified 2026-10-13
- `textbox "Markdown value"` (description textarea, placeholder `Type your description here…`) — verified 2026-10-13
- Submit: accessible name is **`Create ( control enter )`** (shortcut hint in name) — `GetByRole(Button, Name = "Create ( control enter )")` — verified 2026-10-13
- `button "Cancel"` — verified 2026-10-13

## Page: Pull requests list (ui-sandbox, empty)

- URL: `.../pulls` — verified 2026-10-13
- Empty state: `heading "No pull requests matched your search"` — verified 2026-10-13
- Tab counters: same `Open 0` / `Closed 0` format as Issues when authenticated — verified 2026-10-13

## Page: Code view (ui-sandbox)

- URL: `.../tree/main` — verified 2026-10-13
- `link "README.md, (File)"` — verified 2026-10-13

## Page: 404

- Non-existent repo → `img` alt: `404 "This is not the web page you are looking for"` — verified 2026-10-13

## Gotchas (verified as they are discovered)

- Login wall: anonymous writers are redirected to `/login` — auth-dependent sections must document their storageState prerequisite
- Tab counter format differs anonymous (`Open (0)`) vs authenticated (`Open 0`) — count parsers must accept both
- UI fixtures share one storageState file → assembly is `ParallelScope.None`; parallel login raced with state readers (failed 20s timeouts)

---

## Authoring Method

- **Structure + locators:** AI-drafted from live Playwright snapshots, dated per verification
- **Flows + cleanup:** human-reviewed against sandbox rules (`Rules/ui-testing.md`)
- **Risk framing:** human-owned
