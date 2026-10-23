# Rules: Localization Testing

How to test the Moodle demo multilingual site (`https://school.moodledemo.net`): language-agnostic locators, locale contract, HTTP-first L1, data-driven languages, and demo-environment safety.

**Scope.** Covers the `Ui/moodledemo/` sibling project only. The `Ui/github/` UI tests have their own rules — `Rules/ui-testing.md` — and do not fall under these. Where a naming convention is needed for page/form classes, follow `Rules/code.md`; where a rule is not stated here, it is not required by the localization workflow.

---

## What is tested, and in what order

| Layer | What it catches | Tool |
|---|---|---|
| L1 | Site/locale is *down* for a language: wrong `lang`/`dir`, wrong page title, HTTP errors | HTTP only (`moodledemo` category) |
| L2 | *Components* are not translated: menu, navigation, search, language switcher | Playwright, anonymous contexts |

L1 is a **smoke** — it runs first, catches a broken site per language in seconds, and L2 only needs to run on languages that passed L1. L1 checks nothing visual — a wrong-looking but correctly-titled page passes L1 and is caught by L2.

**Site identity (read-only).** The site under test is `https://school.moodledemo.net`, a Moodle demo instance whose switcher exposes **147 `?lang=` codes + default `en`** — the language set lives solely in `Ui/moodledemo/Fixtures/expected-langs.json` (`MoodledemoEndpoints` holds only the base URL and paths, no per-language constants). The language menu is read-only for tests — no writes, no state changes, no sign-in. L1/L2 contexts open the site as an anonymous visitor (no storageState, no auth); the documented exception is L3 (`AccountUiTests`), which signs in with the shared demo accounts.

---

## L-LOC-1: language-agnostic locators

- **Locator construction**: build all locators from *visible* page elements (accessible role + accessible name), never from markup specifics (DOM classes, ids, data-attributes)
- **Root element**: only `<html>` may be addressed by DOM attribute (`lang`/`dir`) — the single exception
- **Locator input**: any string that enters a locator must come from the fixture — the one place where the expected language form is decided
- **Why**: a locator like `page.GetByRole(AriaRole.Heading, new() { Name = "Site administration" })` is a hidden hardcoded English literal — it breaks on the first localized locale. `new() { Name = fixtureTitle }` works on every locale

## L-LOC-2: no per-language branching

- Test code must not branch on language (`if lang == "de"`, per-locale locator dictionaries, language-indexed selectors)
- The *only* per-language data lives in the JSON fixture
- **Why**: any language-specific branch becomes technical debt that multiplies with each new locale

## L-LOC-3: expected-language forms live in fixtures, never in code

- Expected values (`html lang`, `dir`, page title, header strings) live in JSON fixtures — **never** in test code, Page/Form classes, attributes, or `TestCase` arguments
- Test methods are **data-driven**: they receive a language key from the fixture and look up everything else — no hardcoded language keys in `[TestCase]` bodies
- **Why**: adding a language must be a data change only — edit a fixture, no test recompilation, no code review of new `[TestCase]` entries

## L-LOC-4: locator names never carry language

- Locator *input* (`Name` in `GetByRole`) comes from the fixture — never a hardcoded English or target-language string
- Locator *structure* (role, nesting, generic class names) is written in English and shared across all languages
- **Why**: the moment a locator embeds "Site administration" it is English-only; the moment it embedes the German "Website-Administration" it is German-only. Fixture-driven input is language-neutral

## L-DAT-1: language sets live in JSON fixtures

- The set of supported languages, their `html lang` attribute, `dir` attribute, expected page titles, and expected header strings — all live in JSON fixtures (`Ui/moodledemo/Fixtures/expected-langs.json`)
- A fixture entry defines: `code` (`ru`), `lang` (`ru`), `dir` (`ltr`), `title` (`В начало | Mount Orange`); sections: `home`, `loginPage`, `loginLabels`, `navLabels`, `scenarioCodes`, `calendarPageCodes`, `titleScripts`
- **Why**: no `switch` statements, no per-language attributes, no compile-time language list. Adding a language = adding a JSON entry

## L-DAT-2: adding a language = data + entry, no code

- To add a new language: add one object to the JSON fixture set. No test changes, no attribute changes, no Page/Form changes
- Language-keyed data flows: fixture JSON → `MoodledemoFixtures` → Page/test lookups
- **Why**: the data-driven contract is what makes the test suite maintainable as the site's language set evolves

## L-DAT-3: L1 smoke before L2 forms

- L1 locale contract tests run first — fast, HTTP-only, catch a broken site per language in seconds
- L2 form tests run only on languages that pass L1
- **Why**: no point testing a form on a locale that is already down

---

## L-HTTP-1: L1 locale smoke is HTTP-first

- The L1 layer (`Tests/L1SmokeTests.cs`) verifies the locale contract — `html lang`, `dir`, page title — over plain HTTP requests (`RequestHelper` → `Core/Helpers/Http/`)
- No browser launch, no Playwright, no storageState — the smoke runs as an anonymous HTTP GET per language
- **Why**: the L1 smoke should be the fastest possible check that the site is up for each language. A browser launch would slow it down and add a Playwright dependency to a test that does not need one

## L-HTTP-2: L2 form tests are Playwright-only

- The L2 layer (the `Tests/*UiTests.cs` fixtures) uses Playwright browser contexts — anonymous, no auth, no storageState
- Each test navigates to the language-specific URL, waits for the page, then fills and submits the search form
- **Why**: L2 tests the *visual* and *interactive* behavior — form fill, submit, navigation — which requires a real browser

## L-HTTP-3: no writes to the demo site

- Tests do not create, modify, or delete anything on the demo site — the language menu is read-only for tests
- No sign-in, no storageState, no authenticated contexts
- **Why**: the site is a public demo; write operations would pollute it for other users and could be blocked by the site owner

---

## L-SAF-1: demo site is read-only — no writes, no state changes

- Tests never create, modify, or delete anything on the demo site — the language menu is read-only for tests
- No sign-in, no storageState, no authenticated contexts, no cookies that persist across tests
- **Why**: the site is a public demo; write operations would pollute it for other users and could be blocked by the site owner

## L-SAF-2: no rate-limiting, no parallel hammering

- Tests do not run in parallel against the demo site — the L1 smoke and L2 form tests run sequentially
- No aggressive polling, no rapid-fire requests that could trigger rate-limiting
- **Why**: the demo site is shared with other users; aggressive testing could degrade it for everyone

## L-SAF-3: no storageState — L1/L2 anonymous, L3 documented exception

- L1 and L2 contexts are anonymous — no storageState file, no GitHub UI auth; the language menu is accessible without authentication
- **L3 exception (documented)**: `AccountUiTests` signs in with the five shared demo accounts (credentials from `TestConfig`) and logs out — still no storageState file is ever saved or reused; credential-entry fixtures disable DOM-snapshot tracing
- **Why**: authentication for L1/L2 would add complexity and could trigger site-owner blocks; L3 exists precisely to verify the localized login flows

## L-SAF-4: shared demo accounts are flaky by nature

- The five demo accounts are public and shared with every other visitor of the site — a concurrent external session (or the hourly reset) can break a login/logout assert at any moment
- Accepted risk for this suite (informational, never a required check): treat a single login failure as a rerun candidate before filing it as a finding
- **Why**: the demo offers no per-test identities; documenting the risk keeps red runs honest instead of mysterious

## Artifacts

- On failure the fixture saves screenshots (always) and a Playwright trace zip (only for non-credential fixtures) under `%TEMP%\GredjaTestRun\moodledemo-artifacts/`
- Triage order: failure message in `%TEMP%\GredjaTestRun\test-results-*.log` → artifacts in `moodledemo-artifacts/`

---

## Structure: where things live

| Piece | Location | Role |
|---|---|---|
| Page/Form objects | `Ui/moodledemo/Pages/` | Per-component classes: `HomePage`, `LoginPage` |
| Fixture data | `Ui/moodledemo/Fixtures/expected-langs.json` | Language sets + `loginLabels`, `navLabels`, `scenarioCodes`, `calendarPageCodes`, `titleScripts` |
| Support (base, fixtures, HTTP) | `Ui/moodledemo/Tests/Support/` | `MoodledemoTestBase`, `MoodledemoBrowser`, `MoodledemoHttp`, `MoodledemoFixtures`, `ScenarioCodes`, readers/asserts |
| Tests | `Ui/moodledemo/Tests/` | `L1SmokeTests` (L1), `*UiTests.cs` (L2/L3) |
| Config | `Core/Config/MoodledemoEndpoints.cs` | Base URL, `/lang` paths (no per-language constants — the fixture owns the language set) |
| Rules | `Rules/localization-testing.md` | This file |

## Naming

- Page/Form classes: suffix `Page`/`Form` — e.g. `HomePage`, `LoginPage` (`Rules/code.md`)
- Test methods: suffix `Tests` on the class; method naming: action + target + expectation — e.g. `LanguageMenu_HasExpectedLinks`
- Categories: `MoodleDemo` on the class + exactly one check-type per `Rules/categories.md` (`Smoke`/`Regression`/`Negative`) on the method — L1/L2/L3 are prose layer names, not categories
- No abbreviations; booleans: `Is`/`Has`/`Can`/`Should` (`Rules/code.md`)
