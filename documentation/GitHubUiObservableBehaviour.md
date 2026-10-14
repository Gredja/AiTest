# Observable Behaviour — GitHub UI (Web)

**Project:** Gredja (.NET 10.0)
**Surface under test:** GitHub web UI (https://github.com) — sandbox repo `gredjapl/ui-sandbox` (`TestConfig.UiTestRepo`)
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
- Gotcha: the user-menu click right after the login redirect can be swallowed (menu never opens), and the menu itself renders ~300ms after the click — `HeaderPage` waits for the `User navigation` dialog and re-clicks until it is up, at a 1s interval (verified 2026-10-14)

## Logout / signed-out header (UI-13)

- User menu → `link "Sign out"` (exact name; the item is a link with `href="/logout"`, name via `aria-labelledby`) — verified 2026-10-14
- `/logout` is **an account chooser**, not the sign-out: `heading "Select account to sign out"`, per-account `button "Sign out of {username}"` (`input[type=submit]`, name from `aria-label`, form POSTs to `/logout`) and a red `button "Sign out from all accounts"` (`destroy_all=true`) — verified 2026-10-14
- Never click "Sign out from all accounts": it destroys every session of the account, including the one stored in the shared storageState
- After the per-account sign out: `https://github.com/` renders the logged-out header — `link "Sign in"` (exact name), no avatar / no `data-login` — verified 2026-10-14
- The test MUST run with `IsStorageStateEnabled => false` and never call `SaveStorageStateAsync` — a logout in the shared session would break every other UI test in the run (`Rules/ui-testing.md` → Auth)

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

## Page: Issue detail (ui-sandbox)

- URL: `.../issues/{number}` — verified 2026-10-13
- Title: `heading` containing the issue title; status badge `GetByText("Open"/"Closed", Exact = true)` — verified 2026-10-13
- `button "Close issue"` ⇄ `button "Reopen issue"` — the swap happens after the network round-trip, so it is what the page-object waits for — verified 2026-10-14
- `button "Edit issue title"` → inline editor: title `textbox` (the first textbox on the page) — verified 2026-10-13
- Save: accessible name is **`Save ( enter )`** (sr-only shortcut hint), so `GetByRole(Button, Name = "Save")` never matches — use `GetByText("Save", Exact = true)` — verified 2026-10-14
- Comment form: `placeholder "Use Markdown to format your comment"` · `button "Comment"` (exact name); the comment renders only after its POST returns — verified 2026-10-13
- Create submit redirects to `/issues/{number}` — the URL changes before the title heading renders, so waiting on the heading covers both — verified 2026-10-14

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
- Issue actions are async: submit / close / reopen / save / comment respond before the UI reflects them — an instant `CountAsync()` right after the click observed a POST still in flight (failed 2026-10-13); the page-object waits for its post-condition, see `Rules/ui-testing.md` → Waits
- The issues list is indexed asynchronously: a list opened right after a create can still miss the new issue (verified 2026-10-14) — poll with a reload (`WaitHelper.WaitUntilAsync`), not a single count
- `IssuesList_EmptyState_Verify` needs **0 open** issues in ui-sandbox: cleanup closes issues (no delete API), so a leftover probe or an issue created by a test that failed before registration keeps it red (verified 2026-10-14)

---

## Authoring Method

- **Structure + locators:** AI-drafted from live Playwright snapshots, dated per verification
- **Flows + cleanup:** human-reviewed against sandbox rules (`Rules/ui-testing.md`)
- **Risk framing:** human-owned
