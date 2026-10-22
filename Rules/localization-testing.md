# Rules: Localization Testing (Moodle demo)

Scope: `Ui/moodledemo/` — multilingual UI testing of `https://school.moodledemo.net`
(official Moodle demo, hourly reset). Site-scoped sibling of `Ui/github/`.

## Locators (rule L-LOC-1 — language-agnostic)

- Priority: (1) stable semantic `id`/`name`/`href` (never hashed/generated classes),
  (2) role + scope without text, (3) localized text — ONLY from a fixture, never as a
  literal in a page-object.
- Known stable facts (verified 2026-10-19): `input[name='username']`,
  `input[name='password']`, `#loginbtn`, `a[href*='?lang=']`.
- Locator verification: `node Scripts/locator-dump.mjs <url> --anonymous` (the hard
  locator rule of `Rules/ui-testing.md` applies to this site too).

## Locale contract (L1 = HTTP level)

- The locale lives on the `<html>` tag: `lang` + `dir` attributes (read `lang`, never
  `xml:lang`).
- RTL is determined by `dir="rtl"` on `<html>` — never inferred from the language code
  (`ug_lt` is `lang="ug" dir="ltr"`, `ug_ug` is `lang="ug" dir="rtl"`).
- The expected `lang` is NEVER derived from the requested `?lang` code — variants
  normalize (`pt_br→pt`, `es_mx→es-mx`, `en_us→en`); only the fixture is the source.
- `?lang={code}` selects the language for ANY page (`/`, `/login/index.php`, …).
- Title format: `"{translated} | Mount Orange"`; exact expectations live in
  `Ui/moodledemo/Fixtures/expected-langs.json` — tests never hardcode foreign strings.
- Unknown `?lang` value on a fresh visit: HTTP 200 with an English fallback — never a
  server error. A returning session may keep its own language (session cookie).

## Fixture provenance

- `expected-langs.json` = observed contract of all 148 switcher codes (both pages),
  verified by two agreeing clean passes 2026-10-21.
- Regenerate with `Scripts/moodledemo-discover-langs.ps1 -OutFile <path>` — the script
  re-enumerates the switcher and retries transient noise by itself; review the diff
  before committing.
- ~30 low-coverage packs legitimately show an English home title (known gap) — L1
  asserts the exact fixture title, never `title ≠ English`.

## Test design

- HTTP-first: L1 smoke asserts status + locale attributes + title over plain HTTP
  (no browser); Playwright only for L2/L3 (form visibility, persistence, accounts).
- Data-driven only: one test method per check, `[TestCaseSource]` over the fixture list;
  a new language = one fixture row. No duplicated test methods per language.
- HTTP clients must run **without a cookie jar** (`UseCookies = false`): Moodle persists
  the language choice in a session cookie, which would leak state between tests
  (order dependence).
- Transient noise (timeouts, connection drops, foreign HTTP 200 pages without the
  `"| Mount Orange"` title marker) is retryable: the L1 client makes up to 3 attempts;
  a still-bad last attempt surfaces to the test assertion — only persistent failures
  are findings.

## Demo-environment safety

- The site resets hourly → never rely on state created by a previous run.
- Shared demo accounts (`moodle26`): never change passwords; clean up after any write.
- Suite is informational: availability of an external demo is not a release gate.

## Known gap (expected, NOT a bug)

- Cookie-policy notice and the demo-accounts info block stay English in some locales —
  documented demo-site behavior, do not assert their translation.

## Categories

- Service category: `[Category("MoodleDemo")]` (class level), check-type categories per
  `Rules/categories.md` (L1 loads = `Smoke`).
