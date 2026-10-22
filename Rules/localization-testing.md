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

**Site identity (read-only).** The site under test is `https://school.moodledemo.net`, a Moodle demo instance with a **stable multilingual set**: German `de`, Spanish `es`, French `fr`, Italian `it`, Japanese `ja`, Dutch `nl`, Portuguese (Brazil) `pt_br`, Russian `ru`, Swedish `sv`, Turkish `tr` (see `Core/Config/MoodledemoEndpoints.cs`). The language menu is read-only for tests — no writes, no state changes, no sign-in. Each test context opens the site as an anonymous visitor; there is no storageState, no auth.

---

## L-LOC-1: language-agnostic locators

- **Locator construction**: build all locators from *visible* page elements (accessible role + accessible name), never from markup specifics (DOM classes, ids, data-attributes)
- **Root element**: only `<html>` may be addressed by DOM attribute (`lang`/`dir`) — the single exception
- **Locator input**: any string that enters a locator must come from the fixture's `ExpectedTitle` — the one place where the expected language form is decided
- **Why**: a locator like `page.GetByRole(AriaRole.Heading, new() { Name = "Site administration" })` is a hidden hardcoded English literal — it breaks on the first localized locale. `new() { Name = lang.FixtureTitle }` works on every locale

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
- A fixture entry defines: key (`ru`), `ExpectedLang` (`ru`), `ExpectedDir` (`ltr`), `FixtureTitle` (`Moodle demo: Sitename`), `ExpectedTitle` (`Moodle-Demo: Sitename`), `SearchPlaceholder`
- `FixtureTitle` is the **selector** — the string used in locators. `ExpectedTitle` is the **assertion** value — the string the page must actually show. They may differ per language (hyphenation, quoting, transliteration); the fixture records both
- **Why**: no `switch` statements, no per-language attributes, no compile-time language list. Adding a language = adding a JSON entry

## L-DAT-2: adding a language = data + entry, no code

- To add a new language: add one object to the JSON fixture set. No test changes, no attribute changes, no Page/Form changes
- Language-keyed data flows: fixture JSON → `ILocalizationPage` → Page/Form via `this` → test
- **Why**: the data-driven contract is what makes the test suite maintainable as the site's language set evolves

## L-DAT-3: L1 smoke before L2 forms

- L1 locale contract tests run first — fast, HTTP-only, catch a broken site per language in seconds
- L2 form tests run only on languages that pass L1
- **Why**: no point testing a form on a locale that is already down

---

## L-HTTP-1: L1 locale smoke is HTTP-first

- The L1 layer (`Tests/LocalesSmokeTests.cs`) verifies the locale contract — `html lang`, `dir`, page title — over plain HTTP requests (`RequestHelper` → `Core/Helpers/Http/`)
- No browser launch, no Playwright, no storageState — the smoke runs as an anonymous HTTP GET per language
- **Why**: the L1 smoke should be the fastest possible check that the site is up for each language. A browser launch would slow it down and add a Playwright dependency to a test that does not need one

## L-HTTP-2: L2 form tests are Playwright-only

- The L2 layer (`Tests/FormsSmokeTests.cs`) uses Playwright browser contexts — anonymous, no auth, no storageState
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

## L-SAF-3: no storageState, no auth

- All test contexts are anonymous — no storageState file, no GitHub UI auth, no Moodle login
- The language menu is accessible without authentication
- **Why**: authentication would add complexity and could trigger site-owner blocks; the language menu is public

---

## Structure: where things live

| Piece | Location | Role |
|---|---|---|
| Page/Form objects | `Ui/moodledemo/Pages/` | Per-component classes: `NavigationMenuPage`, `SiteSearchForm` |
| Locale contract interface | `Ui/moodledemo/Pages/ILocalizationPage.cs` | `ExpectedLang`/`ExpectedDir` contract |
| Fixture data | `Ui/moodledemo/Fixtures/expected-langs.json` | Language sets: key, `ExpectedLang`, `ExpectedDir`, `FixtureTitle`, `ExpectedTitle`, `SearchPlaceholder` |
| Support (base, HTTP factory) | `Ui/moodledemo/Tests/Support/` | `MoodleDemoTestBase`, anonymous request factory |
| Tests | `Ui/moodledemo/Tests/` | `LocalesSmokeTests` (L1), `FormsSmokeTests` (L2) |
| Config | `Core/Config/MoodledemoEndpoints.cs` | Base URL, `/lang` paths, language-key constants |
| Rules | `Rules/localization-testing.md` | This file |

## Naming

- Page/Form classes: suffix `Page`/`Form` — e.g. `NavigationMenuPage`, `SiteSearchForm` (`Rules/code.md`)
- Test methods: suffix `Tests` on the class; method naming: action + target + expectation — e.g. `LanguageMenu_HasExpectedLinks`
- Categories: `MoodleDemo` + `L1`/`L2` — e.g. `[Category("MoodleDemo")] [Category("L1")]`
- No abbreviations; booleans: `Is`/`Has`/`Can`/`Should` (`Rules/code.md`)
