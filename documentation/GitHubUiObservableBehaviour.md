# Observable Behaviour — GitHub UI (Web)

**Project:** Gredja (.NET 10.0)
**Surface under test:** GitHub web UI (https://github.com) — sandbox repo `gredjapl/ui-sandbox` (`TestConfig.UiTestRepo`)
**Date:** 2026-10-09
**Last verified:** 2026-10-19 (locator/measurement probes through UI-23)
**Scope:** UI pages and flows (Playwright, Chromium)
**Test plan history:** retired (lifecycle — plans deleted after implementation); durable status lives in this document and `Rules/ui-testing.md`

> Sections are added **per page** when locators are verified against a **live snapshot**
> (workflow: `.mimocode/skills/ui-test-gen` → Step 1.2 / Step 5).
> Every locator bullet carries a `verified {date}` marker — undated claims are drafts.
> Every section is numbered `UI-N` in document order — tests reference the number in `[Description]`.

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

## Logout / signed-out header (UI-2)

- User menu → `link "Sign out"` (exact name; the item is a link with `href="/logout"`, name via `aria-labelledby`) — verified 2026-10-14
- `/logout` is **an account chooser**, not the sign-out: `heading "Select account to sign out"`, per-account `button "Sign out of {username}"` (`input[type=submit]`, name from `aria-label`, form POSTs to `/logout`) and a red `button "Sign out from all accounts"` (`destroy_all=true`) — verified 2026-10-14
- Never click "Sign out from all accounts": it destroys every session of the account, including the one stored in the shared storageState
- After the per-account sign out: `https://github.com/` renders the logged-out header — `link "Sign in"` (exact name), no avatar / no `data-login` — verified 2026-10-14
- The test MUST run with `IsStorageStateEnabled => false` and never call `SaveStorageStateAsync` — a logout in the shared session would break every other UI test in the run (`Rules/ui-testing.md` → Auth)

## Page: Repository landing (UI-3, ui-sandbox)

- URL: `https://github.com/gredjapl/ui-sandbox` — verified 2026-10-13
- `button "main branch"` (default-branch switcher, text `main`) — verified 2026-10-13
- `link "README.md, (File)"` in the file tree — verified 2026-10-13
- TestRepo path comes from `TestConfig.UiTestRepo` — never hardcode the repo in page-objects

## Page: Issues list (UI-4, ui-sandbox, empty)

- URL: `.../issues` — verified 2026-10-13
- `heading "Issues" level=1` · `link "New issue"` — verified 2026-10-13
- Empty state: `heading "No results matched your search"` — verified 2026-10-13
- Tab counter renders `Open (0)` both anonymous and authenticated (the 2026-10-13 claim of `Open 0` authenticated did not reproduce on re-check 2026-10-19) — parse with `Open\D*(\d+)` regex

## Page: Issue create form (UI-5, ui-sandbox)

- URL: `.../issues/new` (requires auth; anonymous → `/login?return_to=...`) — verified 2026-10-13
- `heading "Create new issue" level=1` — verified 2026-10-13
- `textbox "Add a title"` (placeholder `Title`, aria-required) — verified 2026-10-13
- `textbox "Markdown value"` (description textarea, placeholder `Type your description here…`) — verified 2026-10-13
- Submit: accessible name is **`Create ( control enter )`** (shortcut hint in name) — `GetByRole(Button, Name = "Create ( control enter )")` — verified 2026-10-13
- `button "Cancel"` — verified 2026-10-13

## Page: Issue detail (UI-6, ui-sandbox)

- URL: `.../issues/{number}` — verified 2026-10-13
- Title: `heading` containing the issue title; status badge `GetByText("Open"/"Closed", Exact = true)` — verified 2026-10-13
- `button "Close issue"` ⇄ `button "Reopen issue"` — the swap happens after the network round-trip, so it is what the page-object waits for — verified 2026-10-14
- `button "Edit issue title"` → inline editor: title `textbox` (the first textbox on the page) — verified 2026-10-13
- Save: accessible name is **`Save ( enter )`** (sr-only shortcut hint), so `GetByRole(Button, Name = "Save")` never matches — use `GetByText("Save", Exact = true)` — verified 2026-10-14
- Comment form: `placeholder "Use Markdown to format your comment"` · `button "Comment"` (exact name); the comment renders only after its POST returns — verified 2026-10-13
- Create submit redirects to `/issues/{number}` — the URL changes before the title heading renders, so waiting on the heading covers both — verified 2026-10-14

## Page: Pull requests list (UI-7, ui-sandbox, empty)

- URL: `.../pulls` — verified 2026-10-13
- Empty state: `heading "No pull requests matched your search"` — verified 2026-10-13
- Tab counters authenticated: `Open (0)` / `Closed (36)` — re-verified 2026-10-17 (parens present; 2026-10-13 claim of `Open 0` did not reproduce)
- `heading "All pull requests" level=1` · `link "New pull request"` — verified 2026-10-17

## Page: Code view (UI-8, ui-sandbox)

- URL: `.../tree/main` — verified 2026-10-13
- `link "README.md, (File)"` — verified 2026-10-13

## Page: 404 (UI-9)

- Non-existent repo → `img` alt: `404 "This is not the web page you are looking for"` — verified 2026-10-13

## Page: Pull request detail (UI-10, ui-sandbox)

- URL: `.../pull/{number}` — verified 2026-10-17
- Title: `heading "{title} - #{number}" level=1` — verified 2026-10-17
- Base/head branches: `link "{base branch}"` and `link "{head branch}"` in the header — verified 2026-10-17
- Tabs: `link "Conversation"` · `link "Commits (N)"` · `link "Files changed"` — verified 2026-10-17
- `button "Edit title"` — verified 2026-10-17
- Open state: `button "Close pull request"` and `button "Convert to draft"` (also a sidebar trigger with the same name — use `.First`) — verified 2026-10-17
- Closed state: `button "Reopen pull request"` replaces the Close button (state swap after the round-trip, same pattern as UI-6) — verified 2026-10-17
- `button "Show options"` (comment/description overflow menu; the `menuitem "Edit comment"` inside appears only after the click — draft, not re-verified via locator-dump) — button verified 2026-10-17
- Comment form: `textbox "Comment"` · `button "Comment"` (exact name, disabled until the body has text) — verified 2026-10-17

## Page: Pull request create / compare form (UI-11, ui-sandbox)

- URL: `.../compare/{base}...{head}` → `heading "Comparing changes" level=1` · `button "Create pull request"` — verified 2026-10-17
- Expanded form (`?expand=1` or after the create click): `heading "Open a pull request" level=1` · `textbox "Add a title *"` (asterisk is part of the accessible name) · `textbox "Comment"` (description textarea) — verified 2026-10-17
- Submit redirects to `.../pull/{number}` — the page-object waits for the URL to contain `/pull/`, then for the title heading

## Page: Pull request files changed (UI-12, ui-sandbox)

- URL: `.../pull/{number}/files` — verified 2026-10-17
- `heading "File tree" level=2`; changed file appears as `treeitem "{file name}" level=1` — verified 2026-10-17

## Page: Branches (UI-13, ui-sandbox)

- URL: `.../branches` — verified 2026-10-17
- `heading "Branches" level=1` · `button "New branch"` · `textbox "Search"` — verified 2026-10-17
- Section tabs: `link "Overview" | "Yours" | "Active" | "Stale" | "All"` — verified 2026-10-17
- Row: branch `link "{branch name}"` · `button "Delete branch"` — verified 2026-10-17
- Gotcha: a branch can be listed in two sections (Your + Active), each row with its own Delete button — pick `.First` or scope to the row (verified 2026-10-17)
- The create dialog fields (`textbox "New branch name"`, `button "Create new branch"`) appear only after `New branch` is clicked — draft, not re-verified via locator-dump
- Delete needs list reload-polling (async index) — the row may linger showing "Deleted" (see Gotchas)

## Page: Compare view commits (UI-14, ui-sandbox)

- URL: `.../compare/{base}...{head}` — verified 2026-10-17
- Commits section: `heading "Commits on {date}" level=3` (filter text `Commits on`) — verified 2026-10-17

## Page: Profile (UI-15)

- URL: `https://github.com/{username}` (user menu → `link "Profile"`) — verified 2026-10-17
- `heading "{username}" level=1` — verified 2026-10-17

## Page: Account settings (UI-16)

- URL: `https://github.com/settings/profile` (user menu → `link "Settings"` exact) — verified 2026-10-17
- `heading "{username} ({username}) settings" level=1` — match the `({username}) settings` part, the display name before it can differ — verified 2026-10-17

## Page: Issues list search & tabs (UI-17, ui-sandbox)

- Tab bar on the default view: `link "Open (N)"` · `link "Closed"` (exact name) — verified 2026-10-19
- Query bar: `combobox "Search issues"` · `button "Search"` (exact; distinct from the header `button "Open quick search dialog…"`) — verified 2026-10-19
- Results: each issue title renders as `heading level=3` and as `link` — verified 2026-10-19
- The results-count heading loses its space in the accessible name (`"260results"`) — do not assert on it — verified 2026-10-19
- Gotcha: the Closed-tab click before the list app hydrates falls back to the link's `href="#"` (fragment navigation, no load event at all) — bounded re-apply until the URL contains the filter (verified 2026-10-19)

## Page: Issue labels picker (UI-18, ui-sandbox)

- Sidebar trigger: `button "Edit Labels"` — case-sensitive exact match; the sidebar also carries a lowercase `link "Edit labels"` (verified 2026-10-19)
- Picker: one `option "{label name}"` per label; selection shows the `selected` state — verified 2026-10-19
- The picker COMMITS on close, not on the option click: while it is open the API still reports the old label set — poll the API only after `button "Edit Labels"` is clicked again (verified 2026-10-19)
- Applied chip in the sidebar: `link "{label} {label description}"` (e.g. `link "bug Something isn't working"`) — verified 2026-10-19
- Gotcha: the option renders before its React handler is wired — the first click can be swallowed silently while the `selected` state still flips; bounded re-apply on the selected state (verified 2026-10-19)
- Permission: the Gredja API token cannot manage labels on the sandbox (403; a `labels` array on issue create is silently dropped) — label tests are UI-write with API read-back only (verified 2026-10-19)

## Page: Comment actions menu (UI-19, issue detail)

- Trigger: `button "Actions for {username}'s comment, {time}"` — the name carries a live timestamp; match the stable `Actions for` prefix — verified 2026-10-19
- The issue body's own trigger is `button "Issue body actions"` (does not match the prefix) — verified 2026-10-19
- Menu items: `menuitem "Edit"` · `menuitem "Delete"` (exact names; siblings: Copy link, Copy Markdown, Quote reply, Reference in a new issue, Pin, Hide, Report content, Block user) — verified 2026-10-19
- Edit mode: `textbox "Markdown value"` (the bottom new-comment form keeps `textbox "Add a comment"`) · `button "Update comment"` — verified 2026-10-19
- Delete: confirm dialog with `heading "Delete comment"` · `button "Cancel"` · `button "Delete"` (exact) — verified 2026-10-19
- Gotcha: menu items lazy-load over HTTP and their handlers wire late; an open menu overlay intercepts further trigger clicks — bounded re-apply, transient click failures retried (verified 2026-10-19)

## Page: Issue create validation & cancel (UI-20)

- Empty title + `Create` → the form stays and shows `Text "Title can not be empty"` under the title field — verified 2026-10-19
- `button "Cancel"` performs a session-history back, not a server navigation — open the issues list before the form in a fresh context, otherwise "back" lands on the blank initial page (verified 2026-10-19)

## Page: Repo read-only views (UI-21, ui-sandbox)

- Landing About: the API repo description renders TWICE on the landing page — About sidebar first, README section second — assert an occurrence count ≥ 2 (verified 2026-10-19)
- README blob (`.../blob/main/README.md`): the README's own H1 renders as `heading "ui-sandbox" level=1` — verified 2026-10-19
- Commits (`.../commits`): `heading "Commits" level=1` · `heading "Commits on {date}" level=3` · commit title as `heading level=4` (e.g. `heading "Initial commit"`) — verified 2026-10-19
- Folder navigation (`.../tree/{branch}/{folder}`): files list as `link` with the file name — verified 2026-10-19

## Page: Anonymous gates (UI-22)

- `.../issues/new` anonymous → server redirect to `/login` (`heading "Sign in to GitHub" level=1`) — verified 2026-10-19
- Pulls list `link "New pull request"` anonymous → compare landing `heading "Compare changes" level=1` + `link "Sign in"` — NOT a /login redirect (verified 2026-10-19; supersedes the retired plan's RedirectsToLogin expectation)

## Page: Non-existent issue / pull (UI-23)

- `.../issues/{maxId+offset}` and `.../pull/{maxId+offset}` render the same 404 illustration as a missing repo: `image "404 “This is not the web page you are looking for”"` — verified 2026-10-19
- Gotcha: the illustration renders LATER than page load (client-side) — a count right after `GotoAsync` sees the page without it; wait in the page-object (verified 2026-10-19)

## Gotchas (verified as they are discovered)

- Login wall: anonymous writers are redirected to `/login` — auth-dependent sections must document their storageState prerequisite
- Tab counters render the parenthesised form `Open (0)` in both anonymous and authenticated views (the earlier anonymous-vs-authenticated format split did not reproduce on re-check 2026-10-19) — count parsers must accept both shapes anyway
- UI fixtures share one storageState file → assembly is `ParallelScope.None`; parallel login raced with state readers (failed 20s timeouts)
- Issue actions are async: submit / close / reopen / save / comment respond before the UI reflects them — an instant `CountAsync()` right after the click observed a POST still in flight (failed 2026-10-13); the page-object waits for its post-condition, see `Rules/ui-testing.md` → Waits
- The issues list is indexed asynchronously: a list opened right after a create can still miss the new issue (verified 2026-10-14) — poll with a reload (`WaitHelper.WaitUntilAsync`), not a single count
- The branches list delete click right after a reload is swallowed silently — no request at all until the app hydrates, up to ~5s (verified 2026-10-19) — re-apply the click bounded until the row shows `Deleted`, then reload-poll until the branch leaves the list
- `IssuesList_EmptyState_Verify` needs **0 open** issues in ui-sandbox: cleanup closes issues (no delete API), so a leftover probe or an issue created by a test that failed before registration keeps it red (verified 2026-10-14)

---

## Authoring Method

- **Structure + locators:** AI-drafted from live Playwright snapshots, dated per verification
- **Flows + cleanup:** human-reviewed against sandbox rules (`Rules/ui-testing.md`)
- **Risk framing:** human-owned
