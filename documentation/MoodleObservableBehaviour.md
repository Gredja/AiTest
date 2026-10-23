# Observable Behaviour — Moodle demo (school.moodledemo.net)

> Verified 2026-10-19 (HTTP probes + `locator-dump --anonymous`); full language sweep
> 2026-10-21 (two clean passes over every switcher code); L2 browser facts 2026-10-22
> (CDP probes + Playwright runs). Official Moodle LMS 5.3 demo ("Mount Orange").
> **Hourly reset** — no state survives between runs.

## What AI does

- L1 smoke over plain HTTP: status, `<html lang/dir>`, `<title>` for locale contract.
- Locale expectations come from `Ui/moodledemo/Fixtures/expected-langs.json`.
- Browser checks (Playwright) use language-agnostic locators only (`Rules/localization-testing.md`).

## Test Categories

- Service: `MoodleDemo`
- L1 locale/loads checks → `Smoke`

## Endpoint Priority

| Priority | Surface | Why |
|---|---|---|
| P0 | `GET /` (locale contract, all `?lang`) | Core multilingual promise |
| P1 | `GET /login/index.php` (locale + form markup) | Login surface, L3 gateway |
| P2 | Browser form visibility (stable selectors) | Page-object foundation for L2/L3 |

---

# READ OPERATIONS

---

## 1. GET / (home)

- Default (no `?lang`): 200, `<html lang="en" dir="ltr">`, `<title>Home | Mount Orange</title>`
  (verified 2026-10-19)
- `?lang=ru`: 200, `lang="ru" dir="ltr"`, `<title>В начало | Mount Orange</title>`
- `?lang=ar`: 200, `lang="ar" dir="rtl"`, `<title>الصفحة الرئيسية | Mount Orange</title>`
- `?lang=he`: 200, `lang="he" dir="rtl"`, `<title>ראשי | Mount Orange</title>`
- Title format: `"{translated} | Mount Orange"`
- Interactive elements (accessibility dump, anonymous): `[button] "Language"`,
  `[link] "Log in"`, `[menuitem] "Home"`, `[menuitem] "Calendar"`, `[textbox] "Search"`
  — all texts localized; never hardcode them in page-objects

## 2. GET /login/index.php

- Default: 200, `lang="en" dir="ltr"`, `<title>Log in to the site | Mount Orange</title>`,
  form present: `name="username"`, `name="password"`, `id="loginbtn"` (verified 2026-10-19)
- `?lang=he`: 200, `lang="he" dir="rtl"`, `<title>התחברות למערכת | Mount Orange</title>`,
  form markup still present — **locale persists onto the login page** (verified 2026-10-19)
- Accessibility dump (anonymous): `[textbox] "Username or email"`, `[textbox] "Password"`,
  `[button] "Log in"`, `[button] "Log in as guest"`, `[button] "… (en_us)"` language control,
  demo-role headings/links (Student/Lecturer/…)

## 3. Language fallback

- `?lang=xx_notreal` (unknown code) on a **fresh visit (no cookies)**: 200,
  `lang="en" dir="ltr"` — silent English fallback, no 500 (verified 2026-10-19)
- Returning session caveat: Moodle stores the language choice in the session cookie
  (`MoodleSession`); with an existing session an unknown `?lang` may keep the session
  language — HTTP test clients must disable cookies to model a fresh visitor
  (found as an order-dependence failure 2026-10-19, fixed with `UseCookies = false`)

## 4. Full language sweep (all switcher codes)

- The login-page switcher exposes **147 `?lang=` codes + default `en` = 148**; for every
  code `GET /?lang=code` and `GET /login/index.php?lang=code` return 200 with a complete
  locale contract (`<html lang/dir>` + `<title>`). Exact per-code values: the fixture,
  verified by two agreeing clean passes (2026-10-21).
- **`<html lang>` often differs from the URL code** (44 of 148): variants normalize to a
  display code — `en_us→en`, `pt_br→pt`, `zh_cn/zh_tw→zh`, `nn/sma/smj→no`, `ckb/kmr→ku`,
  `es_mx→es-mx` (dash, not underscore), `wo→fr`. Never derive the expected `lang` from
  the requested code — only from the fixture.
- **RTL set (9):** `ar, ckb, dv, fa, he, prs, ps, ug_ug, ur` — `dir="rtl"` on `<html>`.
  Proof that RTL must come from `dir`, not the code: `ug_lt` → `lang="ug" dir="ltr"`
  (Latin script) while `ug_ug` → `lang="ug" dir="rtl"` — identical `lang`, opposite `dir`.
- **~30 packs render an English home `<title>`** (`ba, bm, br, ee, ha, ig, it, ja, kab,
  lb, mh, my, or, rw, tk, tt, zu, …`) — installed packs with low string coverage;
  known gap, expected behavior, not a bug. L1 asserts the exact fixture title and does
  NOT require `title ≠ English`.
- **Transient degraded responses under rapid load (~5% per request):** connection
  drop/timeouts AND HTTP 200 pages of a foreign system titled
  `"Log in to the site | Moodle Apps Services"`. Random per request, never code-specific
  (a re-fetch of the same code is clean). L1 client retries up to 3 attempts, keyed on
  the `"| Mount Orange"` title marker + network exceptions (verified by the two discovery
  passes, 2026-10-21).

---

## Risk Framing

### Known Gotchas

- **Session cookie carries the language** — a shared HTTP client leaks locale state
  between tests; no-cookie-jar is mandatory for L1.
- **`xml:lang` vs `lang`** — parse the attribute preceded by whitespace, `xml:lang`
  must not satisfy the check.
- **Hourly reset** — never build assertions on data created in a previous run.
- **`<html lang>` is normalized** for variant codes (44/148 differ from the URL code) —
  derive expectations only from the fixture, never from the requested `?lang` value.
- **Transient noise under rapid load** — foreign 200 pages / connection drops appear
  randomly (~5%); treat as retryable, only a persistent failure is a finding.
- **Known gap (expected, not a bug):** cookie-policy notice and demo-accounts info block
  stay English in some locales; ~30 low-coverage packs keep an English home `<title>`.

### Auth Scope Requirements

- L1/L2 current scope: anonymous only. L3 demo accounts
  (student/teacher/manager/parent/privacyofficer, password `moodle26`) — planned,
  no 2FA on this site.

### Rate Limit / Availability

- External demo, availability and latency are not guaranteed — suite is informational,
  never a required check (`Rules/git.md` → CI).

---

## 5. L2 browser facts (verified 2026-10-22)

- **Fresh-visitor default language comes from `Accept-Language`**: no header → `en`,
  `en-US` → `en_us` (toggle), `de` → `de`, `ru` → `ru`. Playwright contexts are pinned to
  `Locale = "en-US"` so anonymous browser visits deterministically start as `en_us`
  (html lang still normalizes to `en`). HTTP clients send no `Accept-Language` → `en`.
- **Language switcher structure** (`#lang-menu-toggle` + `#lang-action-menu`):
  147 switchable links (`a[role=menuitem][href*='?lang=']`) + **one non-switchable marker
  `a[role=menuitem][aria-current='true'][href='#']` for the active language** — the active
  code is excluded from the links (verified for site-default, `?lang=` and
  Accept-Language choices). The toggle shows the active language as `"<name> (<code>)"`.
- **Switcher dropdown needs Bootstrap ESM**: the demo intermittently serves pages whose
  `/core/esm/{id}/...` modules 404 (stable per URL within a window) → bootstrap never
  binds → the menu cannot open by click/hover/JS-click (CDP-verified); a fresh reload
  usually gets a healthy variant.
- **`redirecterrordetected` modal**: a degraded page variant shows a centered YUI modal
  with that literal text over the login form — it blocks the submit click until the page
  is reloaded (observed 2026-10-22, both pre- and post-login).
- **Authenticated header hides the language switcher** — no `#lang-menu-toggle` on
  `/my/`; for logged-in users language changes go through `?lang=` URLs.
- **Language vs login (demo student account)**: `?lang=ru` chosen before login does
  **not** survive authentication — the dashboard renders in English (account/site
  language wins over the session choice). A language chosen **after** login via `?lang=`
  persists on subsequent lang-less URLs.
- **Login redirects stall in waves**: the POST → `/my/` redirect occasionally never
  reaches DCL for minutes (observed twice consecutively); a fresh attempt rides it out.
- **Known anomaly page confirmed visually**: the ~5% degraded HTTP 200s are the login
  page of another system, `"Log in to Moodle Apps Services"` (English form, no
  `| Mount Orange` marker in the title).
- **Hung/slow resources** (e.g. hero image) can keep the home page off `networkidle`
  indefinitely — idle waits must be bounded.
- **Login form renders with role-addressable fields** (5.4): username is the first
  accessible textbox, password the second (no `password` role in the a11y tree), the
  submit button's accessible name is the localized `loginLabels` string resolved from the
  page's own `<html lang>` — labels are read from the live page, not from the selector.

---

## 6. L2-deep + L3 facts (verified 2026-10-23)

- **Top-nav labels are localized** (probe: en/ru/de/fr/ar/he/ja/zh_cn/es_mx/pt_br):
  `nav a.nav-link` → `Home/Calendar/More`, `В начало/Календарь/Дополнительно`,
  `Startseite/Kalender/Mehr`, … **known gap:** `ja` home nav label stays `"Home"`
  (English fallback, not a bug — same class as the ~30 low-coverage home titles).
- **Guest footer contains a login link** (`footer a[href*='/login/index.php']`) on every
  locale — footer structure is locale-independent, only strings translate.
- **`?lang=` navigation is a full page reload** — form field values do NOT survive a
  language switch (probe: typed username/password are cleared after `?lang=ru`). This is
  expected multi-page-app behavior, **not a retention bug**. The login form stays usable
  (present + localized labels) after the switch.
- **RTL computed styles follow `<html dir>`**: `getComputedStyle(body).direction` and
  the nav's `direction` equal `rtl` for all RTL locales; `text-align` reports the logical
  `start` (maps to right in RTL). Verified for en (ltr baseline), ar, he.
- **Calendar month page** (`/calendar/view.php?view=month&lang=…`): locale contract holds
  (html lang/dir match the home fixture); month controls render (`◄ September | October
  2026 | November ►` en, `◄ сент. | окт. 2026 | нояб. ►` ru — ru uses **abbreviated**
  months, ja `2026年 10月`); **no horizontal overflow** (`scrollWidth == clientWidth`) for
  long-month languages (de) at 1440px. Arrow glyphs flip for RTL (ar/he show `►` first).
- **All five demo accounts log in** (student/teacher/manager/parent/privacyofficer,
  password `moodle26`): POST → 303 `testsession` → `/my/` Dashboard; `UserBar` shows the
  account name; `a[href*='/login/logout.php?sesskey=…']` present but **hidden inside the
  `#user-action-menu` dropdown** (the dropdown-menu is closed by default).
- **Logout mechanics**: the logout link is not visible until the dropdown opens; a direct
  `GotoAsync(logout.php?sesskey=…)` throws `net::ERR_ABORTED` (the 302 redirect chain).
  Working pattern: JS-click the hidden link (or navigate to its href with commit-wait),
  then wait for the link to become **Detached** — only then is the guest state back.
- **Post-login language**: confirms §5 — pre-login `?lang=ru` resets to `en` on the
  dashboard for every account (account/site language wins).

## 7. RTL layout contract (L2) — `RtlLayoutUiTests`

- For every RTL fixture entry: `<html dir="rtl">`, `getComputedStyle(body).direction`
  mirrors it, the `<title>` matches the fixture (script asserted via `titleScripts`).
  Direction facts: §6; title contract: §1 + fixture.

## 8. Localized login labels (L2) — `LoginLocalizationUiTests`

- For the 11 `loginLabels` codes the rendered username label, password label and submit
  button equal the fixture strings. `<html lang>` on the login page is the **base tag**
  (`zh_cn → zh`, `pt_br → pt`, §4) — never derive the label code from it directly without
  the LoginPage row mapping.

## B. Top navigation & guest footer (L2) — `NavFooterLocalizationUiTests`

- Top-nav labels localize per the `navLabels` fixture; `ja` home label is the documented
  English fallback. The guest footer carries a locale-independent login link. Facts: §6.

## C. RTL computed styles (L2) — `RtlStyleUiTests`

- Body and nav `direction` computed styles mirror `<html dir>` (rtl locales). Facts: §6.

## E. Calendar month page (L2) — `CalendarRegionUiTests`

- Locale contract holds, month controls render with a four-digit year, no horizontal
  overflow at 1440px for long-month languages. Facts: §6.

## F. Form retention (L2) — `FormRetentionUiTests`

- `?lang=` reload clears typed fields (expected multi-page behavior); the form stays
  present and localized. Facts: §6.

## L3. Demo-account login flows — `AccountUiTests`

- The five demo accounts log in, reach the dashboard and log out back to guest.
  Mechanics: §5 (post-login language) and §6 (login/logout details).

## Authoring Method

- Structure + fields: live HTTP probes (2026-08)
- Edge cases + gotchas: live probes + first test run (2026-10-19)
- Risk framing: user-owned
