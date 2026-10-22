# Observable Behaviour — Moodle demo (school.moodledemo.net)

> Verified 2026-10-19 (HTTP probes + `locator-dump --anonymous`); full language sweep
> 2026-10-21 (two clean passes over every switcher code). Official Moodle LMS 5.3
> demo ("Mount Orange"). **Hourly reset** — no state survives between runs.

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

## Authoring Method

- Structure + fields: live HTTP probes (2026-08)
- Edge cases + gotchas: live probes + first test run (2026-10-19)
- Risk framing: user-owned
