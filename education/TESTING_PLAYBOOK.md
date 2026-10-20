# Testing Playbook

> A portable distillation of a testing methodology: how to design, structure, write, gate and
> maintain automated tests. Written to be fed to a new AI agent (or read by a new engineer) so it
> starts working at the same level. It is deliberately **project-agnostic** — it contains no
> bindings to any specific product, repo, or API. Project-specific facts live in that project's
> own rules/docs; this file carries the method itself.

---

## 0. How to use this playbook

1. Treat the sections below as binding working rules, the same way you would treat a project's
   `Rules/*.md`. Where the new project has its own explicit rule that conflicts with this file,
   **the project rule wins for execution**, and you must note the difference — if the project's
   way is better, propose folding it back here.
2. Contract with the human: **plan → approval → changes → report**. No structural changes
   without a short plan first; no commits without passing gates; final report in execution order.
3. Ask the **minimum** of clarifying questions — ideally one A/B choice, only when the answer
   actually forks the outcome. Prefer doing the obvious simple thing.
4. This playbook is a living artifact (see §13). Update it whenever methodology changes.

---

## 1. Working cycle (agent discipline)

- **Plan first.** For any non-trivial task: restate understanding, present a compact plan, get
  approval, then execute. For long tasks produce a written work plan and iterate it.
- **Gates before every commit.** At minimum: formatter check (`--verify-no-changes` style) and a
  fast, deterministic sanity subset of tests (call it `HealthCheck` — see §7). Any failure →
  report and stop; never commit red.
- **Commits:** English, `action + object` ("Add X tests", "Fix Y parsing"). Secrets, tokens,
  `.env`, auth states and test artifacts are never committed. Prefer committing through the
  project's commit skill/ritual so gates cannot be skipped.
- **Blocking waits go to the background.** Long test runs, CI polling, retries — delegate to a
  background subagent/job; keep the main thread responsive. A single quick API call is fine inline.
- **Hygiene:** finished plans are deleted immediately after implementation; artifacts older than
  7 days are purged by age-based cleanup; after structural changes sync documentation and any
  tracked backup copies in the same change.

---

## 2. Test design

### 2.1 Check-type categories (exactly one per test, boundaries are hard)

| Category | Meaning | Pass criterion | Name pattern |
|---|---|---|---|
| **HealthCheck** | The endpoint/resource is alive: status-code check only | Expected HTTP status | `*_ReturnsOk`, `*_ReturnsCreated` |
| **ContractCheck** | Response/request schema matches the expected models (round-trip + attribute validation) | Contract helper passes | `*_ReturnsExpectedFields`, `*_ResponseMatchesContract` |
| **Smoke** | Basic sanity of one response: not empty, correct count, content-type, echoed values | One obvious truth | `*_ReturnsNonEmptyList`, `*_ContentTypeIsJson` |
| **Regression** | A rule, invariant, or state transition: field formats, id echoes request, state machine, cross-step integrity | The invariant holds | `*_EachItemHasValidFields`, `*_IdMatchesRequested` |
| **Negative** | Error handling: non-existent/zero/negative id, wrong data, missing auth | Documented error appears | `*_NonExistentId_*`, `*_Missing{Field}_*` |
| **Performance** | Response time on GET list endpoints | Under configured budget | `*_ResponseTimeIsAcceptable` |

Class-level category = service/suite label (exactly one, on the class); check-type categories go
on methods only (exactly one per method; ignored tests keep theirs). Boundary: Smoke answers
"is the response sane?" (one look); Regression answers "does the data obey a rule?" — when a
test mixes both, the rule wins. Filters compose: by service, by check type, by name.

### 2.2 Seed methodology (how many tests, and which)

1. Start from **5 seeds** per target (endpoint / page): composition is 2 happy + 2 failure +
   1 edge — the obvious paths plus the most valuable edge you can name.
2. **Expand** the seeds into a review table: `# | Case | Category | Priority | Source seed`,
   then write the code. Target ≈ **15–20 unique cases per endpoint** before the ceiling rule
   decides you are done.
3. **Enforce 5+ active negatives** per target (ignored tests don't count). Two exceptions only:
   - the mock/fake API simply cannot produce that error → `[Ignore]` with explanation, or
     document why, don't force it;
   - a **ceiling rule**: beyond real signal, extra negatives are noise — write a
     `Ceiling: {what's exhausted} — N < 5 floor` note instead of padding.
4. **One negative test per required request field** (`*_Missing{Field}_*` for every required
   body field) — an empty-body test does not substitute.
5. Expansion happens in **reviewed batches** (PRs), never one giant dump. Audit coverage first,
   then prioritize by risk, not by count. "Test everything" is a rejected strategy — full
   exhaustive coverage is usually not worth the cost; say so explicitly when asked.

### 2.3 Readiness: behavior documentation precedes assertions

The core invariant: **Observable Behavior doc ↔ Test plan ↔ Rules — always in sync.**

- Before asserting anything, write down (or find) the observed behavior of the system: statuses,
  field types, error messages, empty states, UI element names — with a **`verified {date}`**
  marker on each claim. Undated claims are drafts and must not be asserted blindly.
- Every test references the behavior section it verifies (e.g. a `[Description("UI-7 …")]`
  style pointer), so a failing test leads a reader to the source of truth.
- **Live evidence beats documentation.** Re-probe a documented claim before asserting it —
  APIs drift, and a wrong message constant produces a red test, not a docs bug. Record the
  correction in the behavior doc in the same change.

### 2.4 Read-after-write visibility (eventual consistency)

After any write, the read path may lag (search indexing, list refresh, client-side cache):

- Never assert an immediate count right after a create — it races the in-flight write. Wait for
  the **post-condition of the action** first, then assert.
- When the docs say the system is eventually consistent, **poll** with a bounded helper
  (`WaitUntil(condition, timeout)`) and put `elapsed`/`attempts` into the failure message.
- Distinguish three page/network situations:
  - server-rendered navigation → loading finished is enough for an instant check;
  - content that hydrates later → wait inside the page object via the action that consumes it;
  - documented lag (list refresh) → reload/poll loop, not a single count.

### 2.5 Safe identifiers for "should not exist" cases

- Static safe IDs: **`0` and `-1`** are always invalid on REST-ish APIs — never hardcode a
  plausible-looking fake like `999` (it can collide with real data).
- Dynamic non-existent ID: **max existing id + 1**, computed at run time. If concurrent writers
  can create records during the same run (parallel suites), use **max + offset** where offset
  exceeds the worst-case concurrent creations (e.g. +100).

### 2.6 Guarantee-data pattern (presence is proven by creation, not assumption)

For "resource exists / list non-empty" scenarios:

```
GET → (optionally assert empty) → POST in setup → GET again → assert → DELETE/teardown
```

Never seed data by hand and hope; the test owns its fixture lifecycle and proves each step.

### 2.7 Test data rules

- **Fictional values only** — anything written to a shared/staging environment must be obviously
  fake and inoffensive (names, titles, bodies like `API created note …`).
- **Vary ≥ 2 dimensions** across iterations (length × content class, case × symbols, etc.) so
  data isn't a constant that masks bugs.
- **Obfuscate on rewrite**: when reusing a payload shape, transform it (suffix random, swap
  words) rather than copying literal values between tests.
- **Create → remove, never retain.** Test records must not accumulate.

### 2.8 Cleanup discipline

- Register a created resource **before any assert** (register-then-assert). An assertion failure
  must not leak the record: teardown runs regardless, a failed assert must not skip registration.
- Teardown is **the last member of the fixture/class**, runs per fixture (not per test) unless
  isolation demands otherwise, and is **warning-only**: cleanup failure must never turn a green
  run red, but must be loudly logged.
- If the API has **no delete** for a resource, use the nearest legal cleanup (close/archive/
  soft-delete) and document it — then restore any precondition other tests rely on.
- Shared environments: a leftover record can break "empty state" tests — cleanup exists to
  restore preconditions, not just to be tidy.

### 2.9 Negative testing posture

- Assert **status code + error shape** (message/field), not just "it failed".
- Error messages come from the behavior doc; live-probe before asserting.
- Cover the four negative families: bad id (0/-1/non-existent), missing required field, wrong
  type/boundary value, unauthorized/forbidden when auth matters.

---

## 3. Architecture of a test project

```
<repo>/
├── Api/            # single-call API tests, grouped by service → feature
├── E2E/            # multi-step scenario chains only (create → verify → mutate → cleanup)
├── Ui/             # browser tests: site-scoped folders; pages/ and tests/ separated
├── Core/
│   ├── Config/     # base URLs + endpoint paths (one home; never inline URLs in tests)
│   ├── Models/     # request/response DTOs per service domain
│   ├── Helpers/    # Http/ (request infra), Data/ (generators), Assertions/, Params/
│   └── Logging/    # action log + raw run results (reports stay, raw artifacts temp)
├── Rules/          # full written rules (entry doc carries only the summary)
├── documentation/  # observable behavior, plans, structure docs
├── Scripts/        # operational scripts (gates, runners, locator tooling, EOL normalize)
└── backups/        # tracked copies of setup docs — must never go stale
```

Principles:

- **One home per fact.** Endpoints in config, selectors in page objects, payload builders in
  data helpers, assertion verbs in one assertion helper. Duplication across layers is a defect.
- **Models are dumb data**: request models exclude read-only fields (id), response models include
  them; value types nullable only if the JSON field can be null; no constructors/validation/logic
  in models; JSON field names come from a single constants class per domain — never inline
  string literals.
- **Before creating ANY file, decide its place** by naming match + future growth. "Closest
  existing folder" is not a reason. Folder names must match their contents; rename when drift
  appears (structure audit on a schedule).
- Secrets: precedence chain `config file → environment variable → .env (gitignored)`. Example
  values in tracked example configs must stay fake. Never paste a real credential into code,
  docs, or a test.

---

## 4. API testing practices

- **Everything async**: `ExecuteAsync`-style calls; sync execution in async stacks hides deadlocks
  and blocks parallelism.
- **Central request helper** with base URL, auth header, logging handler and typed
  `Get/Post/Patch/Delete<TRequest, TResponse>` wrappers — tests declare data, not plumbing.
- Assert in layers: **status code → contract (required fields, types) → values**. Key verbs live
  in one assertion helper (`ShouldHaveStatusCode`, `ShouldHaveError`, `ShouldHaveValidContract`,
  `ShouldMatchRequest`) so the style can't drift per test file.
- Use a real assertion library with fluent semantics (FluentAssertions-style
  `.Should().Be/NotBeNull/GreaterThan/OnlyContain`) — never bare `Assert.That` sprawl; failure
  messages must name the intent.
- **Parameter builders** belong to a param helper (query/path fragments reused by positive and
  negative cases), not copied into each test.
- Read the endpoint's observable-behavior section before writing; a new negative case means the
  doc gets the new observation first (§2.3).
- **Wall-clock budgets on live services are informational**: when a response exceeds the
  configured budget, mark the test `Inconclusive` with the measured numbers instead of red —
  a slow external API is not a regression of the system under test.

---

## 5. UI testing practices

### 5.1 Page-object doctrine

- Tests contain **only** tests: data, calls into page-object business actions, assertions.
  A test never calls `.Click()` directly.
- A page-object method is a **business action** (`CreateIssue(title, body)`), never a raw
  click. Browser lifecycle, auth and artifacts belong to a fixture/base.
- Register every page object on a single browser wrapper object so tests discover pages in one
  place.
- Site-scoped folders: one folder per site under the UI project; future sites are siblings.

### 5.2 Locator rules

- **Role / text / label only**: `GetByRole`, `GetByText`, `GetByLabel`, `GetByPlaceholder`.
  Never CSS classes or XPath on a site you don't control — generated class names change between
  releases.
- **Reuse before extract**: shared chrome (header/nav/tabs) lives in a base page object and is
  reused everywhere; snapshot only page-unique elements.
- One locator per element. Locators are **named entities collected at the bottom of the page
  object file**; methods reference only those entities — no inline selectors in method bodies
  (parameterized filters over an existing named locator are fine).
- **Locators exist ONLY on Page/Form classes** — single source of truth. Helpers and shared code
  receive them as `ILocator` parameters; a helper needing many elements gets a small grouped
  object rather than a 10-parameter signature.
- A locator must be verified against a **live snapshot** before it is written (§5.3). A claim
  that "the button is called X" from memory or from stale HTML is not verification.

### 5.3 Live locator verification — the one-shot dump

Interactive browser sessions (CLI loops, headed walks) are **rejected** for locator extraction:
they are slow and stall ("too long, you hang"). Use a **one-shot script** that launches a
browser, dumps, and exits:

Design of such a tool (build it the same way in any project):

1. Launch the project's installed Chromium with `--remote-debugging-port=0` and a temp
   user-data-dir; read the assigned port from the `DevToolsActivePort` file.
2. Connect over **CDP** with a zero-dependency runtime (Node ≥ 22 built-in `WebSocket`/`fetch`,
   no npm installs, no browser-version mismatch).
3. Optional auth: inject cookies from the Playwright `storageState` file via
   `Network.setCookies`.
4. Navigate; wait for the load event; optionally poll for expected text
   (`--waitfor "heading you expect"`) to cover hydration. For state that exists only after
   an action, replay the clicks in the same one-shot run (`--click "accessible name"`,
   repeatable — the dump then shows the post-click accessibility tree); use `--waitfor-ax`
   when the expected text never enters innerText (image alt text) or renders later than
   the load event; add `--static-text` to print static text nodes for `GetByText` authoring.
5. Call `Accessibility.getFullAXTree`, filter to interactive roles
   (button/link/textbox/checkbox/tab/menuitem/heading/…), print `- [role] "accessible name"`
   plus flags (`level`, `disabled`, `checked`).
6. Kill the browser, delete the temp profile, exit. Seconds, no session to hang.

Use the dump to author `GetByRole(..., Name = ...)` locators **byte-for-byte from the output**.
When a role+name locator never resolves although the control is visible, re-check its
accessible name in a fresh dump — frameworks inject sr-only text (a save button may read
`Save ( enter )`); either match the full accessible name or use the visible text with an exact
match. For state that appears only after a click (dialogs, inline editors), reach it via a
deterministic URL if one exists (e.g. an `?expand=1`-style query), otherwise mark the claim as
a draft in the behavior doc until verified.

**Hard rule:** locator extraction uses only this script — never interactive sessions, never
guessing from memory.

### 5.4 Auth in browser tests

- Tests **read** a pre-made storage state; they never log in or out per test (a logout kills
  every later test sharing the session).
- One dedicated bootstrap test owns sign-in + state save (run it on demand when state expires);
  exactly one test may sign out, it runs with state disabled and never saves.
- Credentials come only from environment/`.env` — never from code or tracked config.
- A missing storage-state file must **fail fast** with an explicit precondition error naming
  the bootstrap test — never continue anonymously and fail later on the wrong screen.
- The credential-entry test itself must **not record traces/network snapshots** — the typed
  password would persist in the artifact zip; the failure screenshot (masked field) is enough.
- Use **separate identities** for different suites when writes collide (one account for API/E2E
  writes, another fresh account for the UI session).

### 5.5 Waits — the rule people get wrong

- Auto-waiting covers the actionability of the **next** action only — it never waits for the
  **outcome of the previous** one. After submit → redirect, append, status flip, leaving edit
  mode: the page-object method waits for its post-condition (`WaitForURL`, wait for the element
  that exists only after the outcome) **before returning**.
- Ambiguity rule: wait for something that **cannot exist before the outcome** (button swapped
  `Close` → `Reopen`), never for text that may already be on the page.
- Clicks that can be swallowed (menu/header still wiring up) → **bounded re-apply**: check the
  state *before* every retry so a late-rendering element isn't re-clicked shut. The retry
  interval must exceed the effect's render latency (a ~300 ms menu vs a 250 ms retry re-clicks
  an open menu and burns the timeout). The retry body must also swallow transient click
  failures (an open menu overlay intercepting pointer events, a navigation destroying the
  execution context) — one flaky attempt otherwise aborts the whole bounded wait with a raw
  timeout that no later retry can recover from.
- Never use bare fixed sleeps without a WHY comment documenting the measured latency.
- Polling helpers return a result object; assertions embed `attempts`/`elapsed` in the message
  so a red test tells you which timing assumption broke.
- Fixture default: a sane action/navigation timeout (e.g. 15 s) applied globally.

### 5.6 UI write → API cleanup

The UI rarely exposes the created id. Cleanup composition:

1. Perform the UI action.
2. **Read the id back via API** (search by the unique title/body the test wrote).
3. Register it for cleanup **before asserting anything**.
4. Teardown removes via API (close issue, delete comment, close PR + delete branches) —
   warning-only semantics. The API index can lag too → poll the read-back with the wait helper.

### 5.7 Failure artifacts & triage

- On failure the fixture saves a **full-page screenshot** and a **trace/zip with DOM snapshots**
  into a gitignored results folder with the test name; tests themselves never call
  screenshot/trace APIs — fixture code does.
- Triage order: failure message in the run log → screenshot → trace timeline (actions + DOM
  around every click). Age-based cleanup covers artifacts (7 days), no manual registry.
- Artifacts that leave the machine (reports, uploaded logs) are **redacted at the source**:
  credentials in query strings are masked, e-mail/PII in embedded response bodies replaced —
  headers carrying auth tokens are never logged at all.

---

## 6. Code quality bar (what a review must enforce)

- **Naming:** PascalCase for types/members/constants, camelCase locals/params, `_camelCase`
  private fields. Booleans read `Is/Has/Can/Should`. No abbreviations (`response`, not `resp`).
  Lambda parameters are readable singular nouns (`product => product.Id`).
- **Structure:** file-scoped namespaces; one type per file; `var` unless the type is genuinely
  unknowable (`null`, tuples); no unused usings (let the formatter fail on them); methods short,
  single-responsibility (~30 lines; tests may run longer only when splitting hides the
  scenario); max ~5 parameters.
- **Comments: zero by default.** A comment is a last resort — refactor for clarity first; keep
  only a non-obvious WHY (measured latency, protocol quirk, workaround) that the code cannot
  express. Narrating *what* code does is a review defect. Applies to scripts and configs too.
- **No magic numbers** — extract to named constants. No nested ternaries. `nameof()` in
  exception arguments. Specific exceptions, flow control without exceptions, no `null!`.
- **Null safety:** `?.`, `??` fallback, `is not null`.
- **LINQ/style:** `Any()` over `Count() > 0`; no needless materialization; `FirstOrDefault()`
  over `Where().First()`; synchronous side-effect loops over `List<T>` → `ForEach`; string
  interpolation; `IsNullOrEmpty` over length checks.
- **Dead code trap:** before deleting "unused" members, grep the project's rules and docs — a
  documented methodology helper with zero call sites is still part of the contract.
- **Assertions:** one fluent library everywhere; helper verbs centralized; never hand-rolled
  string compares for status/error/contract checks.
- **Config:** base URLs and endpoint paths live in config classes; a URL literal inside a test
  is a review failure.

---

## 7. Git & CI design

- **Pre-commit gates (local, mandatory):** formatter verify + the fast deterministic
  `HealthCheck` subset. Either fails → fix, never bypass with `--no-verify`.
- **PR gate (required checks):** format + HealthCheck only. **Never** put the live/long/nightly
  suite in required checks — it is informational by nature and must not block merges.
- **Nightly:** scheduled full run (or live subset) as a signal. Triage failures as
  *drift* (behavior changed → update docs/tests) vs *flake* (rerun green in seconds → quarantine
  or fix the timing assumption). Keep a known-flakes list; a single red among known flakes is
  triaged before being called a regression.
- **Two-pass runner:** pass 1 runs the suite in parallel (e.g. 8 workers), collects failures
  from the test logger output, pass 2 reruns just those sequentially; exit code = pass-2 verdict.
  This separates real failures from parallelism flakes in one command.
- **Baseline tracking:** keep the green counts in memory/docs (e.g. "gate: N/N, suite: M/M") so
  a drop is visible immediately.
- **Repo hygiene:** feature branches + PRs; English `action + object` commits; EOL normalization
  before `git add` (zero line-ending warnings); `.gitignore` covers auth states, results,
  artifacts, `.env`; tracked `backups/` copies of setup docs are updated in the same commit as
  the change they mirror.
- CI secrets live only in repository settings; the live suite never becomes a required check.

---

## 8. Coverage strategy — how many tests is "enough"

1. Audit what exists (map tests → documented behaviors; find gaps, not counts).
2. Prioritize by **risk**: writes, cross-entity flows, money/state transitions > CRUD reads.
3. Expand in **batches delivered as PRs**, each with green gates; never a 100-test dump.
4. Resist exhaustive matrices; a stated risk-based rationale beats a coverage percentage.
5. Track a **gate baseline** and known flakes; growth targets are decided with the human
   (e.g. "+40 API, +30 UI over 4 batches"), not invented unilaterally.

---

## 9. Onboarding checklist for a new codebase

1. Read the repo's entry doc (`AGENTS.md`/`README`) and its `Rules/` fully before the first
   change. Find: naming rules, gate commands, category definitions, cleanup policy.
2. Find the **behavior documentation** (observable behavior / contract docs). If absent, that is
   gap #1 — start it as you verify things.
3. **Read existing code first, before generating anything**: glob/grep for the endpoint's/page's
   existing tests, models, helpers. Extend what exists; never create a duplicate file. A
   generator that didn't read the codebase always produces a second copy of something.
4. Reproduce the **green baseline**: run formatter + fast gate + full suite once; record counts
   and known flakes. Only then change code — you must know what "broken by me" looks like.
5. Identify the commit ritual (skill/script) and its gates; use it, don't reinvent it.
6. Map the test project structure: where models/helpers/pages/config live; where locators are
   allowed to exist.
7. Pick the first small target, apply the seed methodology (§2.2), ship it as a reviewed batch.
8. Sync docs/backups/playbook with anything you learned (§13).

---

## 10. Template: Observable Behavior document (API)

Create `documentation/{Service}ObservableBehaviour.md` from this skeleton when it does not
exist. It is the single source of truth for test design — tests assert only what it states.

```markdown
# Observable Behaviour — {Service} API

**API Under Test:** {Service} API ({BaseUrl})
**Date:** {Date}
**Scope:** GET / POST / PUT / PATCH / DELETE
**Test Plan:** [{Service}TestPlan.md]({Service}TestPlan.md)

## What AI does

1. Reads this document first — before any code generation or review
2. Uses Positive bullets as assertions (field bullets → Should() calls)
3. Uses Negative bullets as test cases — one [Category("Negative")] per bullet; below the
   5-negative floor record `Ceiling: {what's exhausted} — N < 5 floor documented per ceiling rule`
4. Uses Validation Rules for model generation (types, nullability, bounds)
5. Uses Request Body table for write tests — required → mandatory tests, optional → conditional
6. Uses State transitions for E2E chains
7. Uses Endpoint Priority for coverage ordering (P0 first)
8. Never invents fields — only asserts what is documented; missing field = discrepancy report
9. Uses Seed methodology — 5 seeds → expand table → 5+ active negatives
10. Maps each Top-3 risk from the Test Plan to ≥1 test — an uncovered risk is a gap

## Test Categories
| Category | Applies to | Example name |
|---|---|---|
| HealthCheck | Status codes only | *_ReturnsOk |
| ContractCheck | Schema matches models | *_ReturnsExpectedFields |
| Smoke | Non-empty, count, content-type | *_ReturnsNonEmptyList |
| Regression | Field rules, invariants | *_EachItemHasValidFields |
| Negative | Error handling | *_NonExistentId_* |
| Performance | Response time (GET lists) | *_ResponseTimeIsAcceptable |

## Endpoint Priority (by business value)
| Priority | Endpoints | Rationale |
|---|---|---|

# READ OPERATIONS

## 1. GET /{path}
- Response status is 200 OK
- Response body is a JSON object
- Object has: `field` (type), `field` (type)
- Content-Type is application/json
- Response time < {SLA}

**Negative:**
- Non-existent resource → 404, body has `message` (string: "…")

# WRITE OPERATIONS

## 2. POST /{path}
- Request body: `field` (type, required), `field` (type, optional)
- Response status is 201 Created
- Object has: `id` (integer > 0), `field` (matches request)

**Negative:**
- No auth → 401
- Missing required field → 422

## 3. PATCH /{path}/{id}   (200 OK, subset body)
## 4. PUT /{path}/{id}      (200 OK, full body, all required)
## 5. DELETE /{path}/{id}   (204 No Content, empty body)
   …same bullet structure, each with its Negative block…

## Response Time SLA
| Category | Endpoints | SLA | Rationale |

## Validation Rules
### Path Segments / Query Parameters / Headers   (| Parameter | Valid | Invalid | API behavior |)
### Request Body (Write Operations)              (| Endpoint | Required | Optional | Invalid → Error |)
### Response Field Constraints                   (| Field | Constraint | Type |)
### Data Type Summary                            (| API type | language mapping | Nullable? | Notes |)

## Cross-cutting
- Content-Types, auth header shape, error body shape, pagination defaults, rate-limit headers

## Risk Framing
### Known Gotchas / ### Rate Limit Impact / ### Data Dependencies
### Flake Risks / ### Auth Scope Requirements / ### Write Operation Risks

## Authoring Method
- Structure + fields: drafted from API docs
- Edge cases + gotchas: reviewed from real incidents
- Risk framing: human-owned
- **Verified claims are dated:** behavior confirmed by a live probe carries `(verified {Date})`;
  an undated claim is an unverified draft and must not be asserted blindly
```

Workflow with this doc: read → probe live to confirm/deny each claim you will assert → fix the
doc first → write tests that reference section numbers (`[Description("3.2 …")]`).

## 11. Template: Observable Behavior document (UI)

Create `documentation/{Service}UiObservableBehaviour.md`. One numbered section per page
(`UI-N` in document order — test `[Description]` attributes point at these numbers), only
snapshot-verified facts:

```markdown
# Observable Behaviour — {Service} UI

> Sections are added per page when locators are verified against a live one-shot dump (§5.3).
> Every locator bullet carries a `verified {date}` marker — undated claims are drafts.

## What AI does
- Reads the relevant page section before generating/reviewing UI tests
- Uses locator bullets verbatim for GetByRole/GetByText/GetByLabel
- References the section number in every [Description]
- Never logs in/out inside feature tests — storageState only

## Page: {Page name} (UI-{N}, {scope note})
- URL: `{pattern}` — verified {date}
- `GetByRole(Heading, "{title}") level=1` — verified {date}
- `GetByRole(Button, "{name}")` (exact name) — verified {date}
- Accessible-name gotcha: the button reads `"{label} ( shortcut hint )"` — match the full
  accessible name or use exact visible text — verified {date}
- State: after submit the control swaps `{A}` ⇄ `{B}` — the swap is the wait target
- Empty state: `GetByRole(Heading, "{text}")` — verified {date}
- Gotcha: list refresh after a write is async — poll with reload — verified {date}

## Gotchas (verified as they are discovered)
- (login walls, counter format differences, hydration latency with measured ms, shared
  storageState and parallelism constraints, empty-state preconditions other tests break)

## Authoring Method
- Structure + locators: from live one-shot dump, dated per verification
- Flows + cleanup: reviewed against the project's UI rules
- Risk framing: human-owned
```

Rules while writing: locator claims only from a dump taken for THIS page on the claimed date;
in-dialog elements that the dump cannot reach (click-only states) are marked
`draft — not re-verified` until you verify them (prefer a deterministic URL that opens the
state, e.g. an `?expand=1`-style query). Every new UI test batch must touch its section in the
same change (document sync, §2.3).

---

## 12. Templates: test classes

Language shown is C# / NUnit / FluentAssertions (the stack this method was forged on) — the
shapes port directly to other stacks. Rules embedded below: one service category on class, one
check-type on method; zero comments; `var`; async throughout; register-then-assert; teardown
last and warning-only; no helper methods inside test classes (they belong to base/helpers).

### 12.1 API test class

```csharp
namespace Api.{Service}.{Feature};

[TestFixture]
[AllureNUnit]
[Category("{Service}")]
public class GetAll{Endpoint}Tests : {Service}RequestHelper
{
    private static readonly {Endpoint}ModelRequest TestBody = new()
    {
        Field = "fictional value",
        Count = 7
    };

    [Test]
    [Category("HealthCheck")]
    [Description("1.1 Status code is 200")]
    public async Task GetAll{Endpoint}_ReturnsOk()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("1.2 Response matches expected model schema")]
    public async Task GetAll{Endpoint}_ResponseMatchesModel()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("1.3 Returns expected count")]
    public async Task GetAll{Endpoint}_ReturnsExpectedCount()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount({Service}Endpoints.Expected{Endpoint}Count);
    }

    [Test]
    [Category("Regression")]
    [Description("1.4 Each item has valid required fields")]
    public async Task GetAll{Endpoint}_EachItemHasValidFields()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        foreach (var item in response.Data!)
        {
            item.ShouldHaveValidFields();
        }
    }

    [Test]
    [Category("Negative")]
    [Description("1.5 Non-existent id → 404")]
    public async Task Get{Endpoint}_NonExistentId_ReturnsNotFound()
    {
        var response = await Get<{Endpoint}Model>(
            {Service}Endpoints.{Endpoint}ById, IdParam(await NextNonExistentIdAsync()));
        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var response = await Get<List<{Endpoint}Model>>({Service}Endpoints.{Endpoint});
        _allItems = response.Data!;
        _maxId = _allItems.Max(item => item.Id);
        if (_allItems.Count == 0)
        {
            var create = await Post<{Endpoint}ModelRequest, {Endpoint}Model>(
                {Service}Endpoints.{Endpoint}, TestBody);
            _createdId = create.Data!.Id;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_createdId.HasValue)
        {
            await Delete<object>($"{ {Service}Endpoints.{Endpoint}}/{_createdId}");
        }
    }

    private List<{Endpoint}Model> _allItems = [];
    private int _maxId;
    private int? _createdId;
}
```

Notes: dynamic non-existent id = `maxId + 1` (+ concurrency offset when parallel writers share
the run) — never a literal like 999; write payloads vary per run via a shared data generator
(random string, length constants from one constants file); repeated payloads are
`static readonly` fields; guarantee-data pattern lives in `[OneTimeSetUp]`.

### 12.2 E2E chain test class (multi-step writes)

```csharp
namespace E2E.{Service}.Tests;

[TestFixture]
[AllureNUnit]
[Category("{Service}E2E")]
public class {Endpoint}E2ETests : {Service}E2ETestBase
{
    private readonly List<int> _createdIds = [];

    [Test]
    [Category("Regression")]
    [Description("E2E-1 Create → read back → mutate → verify")]
    public async Task Create_Mutate_VerifyLifecycle()
    {
        var create = await Post<{Create}ModelRequest, {Endpoint}Model>(
            {Service}Endpoints.{Endpoint}, NewPayload());
        create.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdIds.Add(create.Data!.Id);              // register-then-assert

        var readBack = await Get<{Endpoint}Model>(
            {Service}Endpoints.{Endpoint}ById, IdParam(create.Data.Id));
        readBack.ShouldHaveStatusCode(HttpStatusCode.OK);
        readBack.Data.ShouldMatchRequest(create.Data);

        var mutate = await Patch<{Update}ModelRequest, {Endpoint}Model>(
            {Service}Endpoints.{Endpoint}ById,
            UpdatedPayload(), [.. IdParam(create.Data.Id)]);
        mutate.ShouldHaveStatusCode(HttpStatusCode.OK);

        var verify = await Get<{Endpoint}Model>(
            {Service}Endpoints.{Endpoint}ById, IdParam(create.Data.Id));
        verify.Data!.Field.Should().Be(UpdatedPayload().Field);
    }

    [OneTimeTearDown]                                // LAST class member
    public async Task OneTimeTearDown()
    {
        foreach (var id in _createdIds)              // cleanup chain in base/helper, not here
        {
            await Cleanup.Delete{Endpoint}Async(id); // warning-only — never fails the run
        }
    }
}
```

Notes: cleanup order matters (children before parents: comments → issues, branches → PRs);
complex chains live in the base class / cleanup helper — never as private methods in the test
file; each chain owns its resources; sandbox-only writes.

### 12.3 UI page object + UI test class

```csharp
namespace Ui.{Site}.Pages.{Area};

internal sealed class {Area}Page(IPage page)
{
    private static readonly TimeSpan _outcomeTimeout =
        TimeSpan.FromMilliseconds({Site}UiTestBase.UiTimeoutMs);

    private readonly IPage _page = page;

    internal async Task OpenAsync() =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/{area-path}");

    internal async Task CreateAsync(string title, string body)
    {
        await _titleBox.FillAsync(title);
        await _bodyBox.FillAsync(body);
        await _submitButton.ClickAsync();
        await _itemHeadings.Filter(new() { HasTextString = title }).First.WaitForAsync();
    }

    internal async Task<WaitResult<bool>> WaitForItemAsync(string title) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await _page.ReloadAsync();
                return await _itemHeadings.Filter(new() { HasTextString = title }).CountAsync() > 0;
            },
            found => found,
            _outcomeTimeout);

    private readonly ILocator _titleBox = page.GetByRole(AriaRole.Textbox, new() { Name = "Add a title" });
    private readonly ILocator _bodyBox = page.GetByRole(AriaRole.Textbox, new() { Name = "Body" });
    private readonly ILocator _submitButton = page.GetByRole(AriaRole.Button, new() { Name = "Create ( enter )" });
    private readonly ILocator _itemHeadings = page.GetByRole(AriaRole.Heading);
}
```

```csharp
namespace Ui.{Site}.Tests.{Area};

[TestFixture]
[AllureNUnit]
[Category("{Site}Ui")]
public class {Area}{Flow}Tests : {Site}UiTestBase
{
    private readonly List<int> _createdIds = [];

    [Test]
    [Category("Smoke")]
    [Description("UI-{N} {flow}: {observable outcome}")]
    public async Task {Area}_{Flow}_{Outcome}()
    {
        var title = $"UI {DataGenerator.RandomString(TestDataConstants.TitleRandomLength)}";

        await Browser.{Area}Page.OpenAsync();
        await Browser.{Area}Page.CreateAsync(title, body);

        var id = await ReadBackIdViaApiAsync(title);  // API read-back is the id oracle
        _createdIds.Add(id);                          // register-then-assert

        var created = await ApiHelper.GetAsync(id);   // hybrid verify: act via UI, assert via API
        created.ShouldHaveStatusCode(HttpStatusCode.OK);
        created.Data!.Title.Should().Be(title);

        (await Browser.{Area}Page.WaitForItemAsync(title)).IsSuccess.Should().BeTrue(
            $"item must appear (attempts=…, elapsed=…)");
    }

    [OneTimeTearDown]                                 // LAST class member
    public async Task OneTimeTearDown()
    {
        foreach (var id in _createdIds)
        {
            await Cleanup.CloseAsync(id);             // warning-only
        }
    }
}
```

Notes: locators are the named fields at the **bottom** of the page object and nowhere else;
every action method returns only after its post-condition; tests contain no `.Click()`, no
screenshot/trace calls (fixture captures artifacts), no URL literals; read-back id via API
before asserting; UI-only asserts are reserved for what only exists visually (badges, URLs,
empty states).

---

## 13. Update protocol for this playbook

- **When:** at every commit, alongside the project's backups sync — if the change touched test
  rules, generation skills, gate scripts, category semantics, wait/cleanup policy, or corrected
  a methodology belief, fold the delta into this file in the same commit (or the immediately
  following one if the change was large).
- **Skill changes are a mandatory trigger (hard rule, 2026-10-19):** any edit to an agent
  skill's procedure (new steps, new stage, changed gate order, new check category) MUST be
  reflected here as the portable version of that procedure (see §14 for the current map) —
  in the same flow as the skill edit. Adding/removing a skill → add/remove its portable
  workflow row here too.
- **Actualize at every commit (hard rule, 2026-10-19):** the commit gate includes a check
  that this playbook still matches the live procedures — stale method is a commit blocker,
  same standing as a stale setup backup.
- **How:** edit the affected section; keep it project-agnostic — move concrete project facts to
  that project's own docs, keep only the method here. Keep `verified`-style honesty: don't
  record wishful process as if it were practice.
- **Why:** this file is what gets carried to the next project and the next agent — stale method
  is worse than no method.

---

## 14. Skill workflows (portable procedures)

Agent skills are just named, reusable versions of these procedures. Names are project-local;
the step ORDER below is portable. Every "gate" = whatever the project defines as its fast
truth-check (typically formatter verify + a fast test subset) — never skip one, never
substitute a red gate with a re-run-hope.

### 14.1 Commit gate (safe commit of a change set)
1. Status + diff — know exactly what is about to ship.
2. Message: action + object, English; user approves unless pre-authorized.
3. Gate: formatter verify → fix or stop.
4. Review the diff against the project's rules (naming, comments, magic values, config homes).
5. Gate: fast deterministic test subset (smoke/health) — any red → stop, no bypass flags.
6. Sync duty: backups/education/structure docs that mirror what changed — same flow, same commit.
7. EOL-normalize the worktree, stage everything, confirm zero line-ending warnings, no secrets staged.
8. Commit → push (never to the default branch without explicit confirmation).

### 14.2 Branch finalize (PR → merge)
1. Preconditions: not on the default branch; tree clean (else 14.1 first).
2. Push; open a PR (title ≤70 chars action+object; body = commit summary + verification numbers).
3. Wait for CI gates via a BACKGROUND watcher (poll check-runs; never a blocking main-thread loop).
4. Any check red → stop with names/conclusions; all green → merge via API (no force options).
5. Delete the merged remote branch; verify it is gone; remind the human to delete the local one.
6. The agent never switches to the default branch and never deletes local branches itself.

### 14.3 CI/nightly verdict (read-only check-in)
1. Fetch the latest scheduled run of the project's informational suite (token from env, never printed).
2. Not completed → bounded poll, then report "in progress"; completed → one-line verdict with run number, outcome, URL.
3. Failure → dig per-job/per-step; distinguish persistent failures from flaky races (rerun-in-script semantics).
4. Never re-dispatch, cancel, or delete runs from this flow.

### 14.4 Full audit → fix plan (diagnosis stage — NO code edits)
1. Load the project's rule files in full (they are the audit's authority) AND keep a second
   layer running: the agent's own checks (correctness, races, data-loss risks — issues the
   written rules do not cover). Both layers always report: rule findings cite the exact
   rule file/section, agent findings cite `agent-check (<why>)`.
2. Fan out READ-ONLY subagents in parallel, one per category (naming/types, style, principles,
   models/config, tests/assertions, comments, structure, security/log-hygiene, async/concurrency,
   determinism/reruns, doc-sync, dead code — enable the set the project cares about).
3. Each agent returns a table: severity, exact file:line, rule (or agent-check), current, suggested.
4. Main agent produces a FIX PLAN FILE with per-item rows: what is wrong in plain words,
   what the fix (or the SKIP reason) is, batch grouping, OWNER (who executes), STATUS
   (pending). Group batches from independent/safe to broad (comments → core → api → tests →
   ui → adapters → structure moves).
5. Present the plan table to the human; ask desired scope. STOP — zero fixes in this stage.
6. The plan file is the cross-session state; statuses are updated only by the fix stage.

### 14.5 Fix-the-plan (treatment stage — separate invocation)
1. Re-confirm scope with the human; read the plan file.
2. Process batches in the plan's order: apply only approved pending items.
3. Gate after EVERY batch: build + formatter + fast subset + the suites the batch touched.
   Red gate → fix or revert the batch's last change; never proceed on red; never fix-forward blindly.
4. One commit per batch, via the commit gate (14.1). Structural moves: one move per flow,
   with docs/structure sync in the same flow.
5. Update plan statuses after each batch; SKIP items keep their reason visible.
6. Close: delete the plan when every item is done/skipped; final report = per-batch table,
   gate numbers, commit list. Optional verify: a quick re-audit of changed files only.

### 14.6 Read-only codebase review (no changes)
1. Full-file scan against the same rule set as 14.4, but findings only — no plan file needed
   unless the human asks for one.
2. Report by severity with exact locations; suggest fixes; never apply anything.

### 14.7 Uncommitted-changes review
1. Scope = working tree + staged diff (modified, added, deleted).
2. Review those hunks against the rules and the surrounding code's patterns.
3. Findings only; blocking issues stated as blockers.

### 14.8 Pull-request review
1. Scope = the PR diff vs its base; read the PR description for intent.
2. Check rules compliance, test coverage of the change, and gate results.
3. Findings only; do not push to the PR branch.

### 14.9 API test generation (new endpoint/surface)
1. Read the surface's Observable Behavior doc first; re-verify claims that look stale.
2. Design seeds (happy path + contract + fields) → expand → enforce a minimum set of active
   negatives (documented error messages, invalid ids, auth walls).
3. Generate tests in the project's existing style (categories, naming, shared helpers — extend,
   never duplicate files); writes use fictional, varied data; dynamic non-existent ids derived
   from the live max, not invented constants.
4. Gate (14.1 gates); cross-verify writes via API read-back; cleanup via register-then-assert.
5. Sync the OB doc / plan / rules if behavior or structure changed (same flow).

### 14.10 E2E scenario generation (multi-step write chains)
1. Chain shape: create → verify → mutate → cleanup; the chain lives in the E2E project, not the
   read-only suite.
2. Setup via API, assertions against the real oracle, teardown that runs on any failure path
   (partial-failure safe: register before assert).
3. Gate; sandbox must be left as found.

### 14.11 UI test generation (browser)
1. Author locators ONLY from a live one-shot accessibility dump of the page (never from memory,
   never CSS/XPath on the product); post-action states need post-click dumps or deterministic URLs.
2. Page-objects own all locators (named fields, bottom of file); tests orchestrate and assert only.
3. Auth via a pre-made storage state; exactly one bootstrap sign-in test saves it; exactly one
   sign-out test may run stateless; feature tests never touch credentials.
4. Waits: the action's OUTCOME is awaited before any assert (post-condition element, URL, or
   bounded re-apply with state-check-before-retry; transient click failures are retried, not fatal).
5. UI writes → API cleanup; register-then-assert; teardown last.
6. Gate + the UI suite; new pages get OB sections with dated, verified locators.

### 14.12 Coverage run
1. Run the suite with instrumentation; collect per-file line coverage.
2. Map endpoints/surfaces to their tests; report gaps by risk, not by percentage-chasing.
3. Findings only — expansion goes through 14.9/14.10/14.11.

### 14.13 Full test run (+ optional rich report)
1. Run every category; collect per-suite pass/fail numbers.
2. Write the machine-readable run report artifact (age-based cleanup applies).
3. Optional: generate the HTML report; never treat the long live suite as a required merge gate.

### 14.14 Documentation sync after structural change
1. Trigger: files added/moved/renamed/deleted; skills/rules/config/env docs touched; OB changed.
2. Update: structure map, setup backup, entry docs, this playbook (portable delta only),
   skill path tables — in the same flow as the change, even without a separate commit.
3. Fact-verify every edited claim against the code (env names, commands, paths).

### 14.15 Rules showcase (onboarding a human/agent to a codebase)
1. Point at the entry doc and the rule files; do not paraphrase rules from memory.
2. Pair each rule group with the command that enforces it (gates, linters, suites).
