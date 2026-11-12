# Full project audit — 2026-10-09 (historical)

> **Historical record (2026-10-09).** Most findings below were fixed in subsequent sessions; the original "start the next session from this file" directive is retired. Kept for traceability — do not treat open items here as a live backlog (the live backlog is GeneralPlan.md).

**Method:** 3 parallel explore agents (Rules → code/docs → findings). Coverage: ~200 `.cs` (excluding bin/obj) in Api/, E2E/, Ui/, Core/, AllureAdapter/ + AGENTS.md, Rules/*.md, documentation/*, .mimocode/skills/*, backups/SETUP.md, testsettings*.json, .gitignore.
**Total:** ~73 major, ~38 minor, ~13 style (finding lines; one major topic "status-first" spans 35 files).

## Priorities for the next session

1. **Old UI authorization model** (major, 4 files): `[Explicit] StorageStateBootstrap` and "tests never log in" are obsolete — there is in fact an automated test `LoginSignInTests`. Affected: `AGENTS.md`, `Rules/ui-testing.md`, `.mimocode/skills/ui-test-gen/SKILL.md`, `documentation/GitHubUiObservableBehaviour.md` (What AI does #7). In addition, Rules/skills describe cleanup via `CleanupIssueAsync` in the Ui fixture, which does not exist.
2. **Status-first before `.Data`** (major, 35 files / ~100 methods): dereferencing `response.Data` without a prior `ShouldHaveStatusCode` — on a 500 you get an NRE instead of a clear failure. One pattern, widespread.
3. **Stale guidance in generator skills** (major): `api-test-gen` — non-existent paths/namespaces and `Model`/`Request` suffixes instead of `ModelResponse`/`ModelRequest`; `e2e-test-gen` — per-test `[TearDown]`-cleanup instead of fixture-registry and helpers in the test class; `review` — non-existent paths in the file list; `audit` — magic strings threshold 3+ instead of 2+.
4. **UI test template in ui-test-gen does not compile** (major): calls `Get<>`/`ShouldHaveStatusCode`/`CleanupIssueAsync`, which do not exist in `GitHubUiTestBase`.
5. **Rules contradiction**: `models.md:82` ("no validation attributes") ↔ `assertions.md` (ContractCheck lives on these attributes) + widespread use in `Core/Models/**`.
6. **`[Description]` numbering ↔ OB** (major): GitHub API — almost all groups do not match the OB section numbering; "12.x" collision for Auth and Contributors; JP — three different numbering schemes; E2E-"E2E-N" are not described anywhere **(superseded: E2E-N is now documented 1:1 in `GitHubTestingStructure.md` §6; the remaining legacy drift is tolerated per `Rules/test-practices.md` → Document sync)**.
7. **Code-style findings**: long methods (>30), magic `8` in `RandomString(8)` (16 places), `"probe"`/`"audit-"`, booleans without `Is/Has` (`UiHeadless`, model `Admin/Push/...`), `null!` in the Ui fixture, bare `catch`.
8. **Docs/structure**: omissions in `FILE_STRUCTURE.md` (ui-testing.md, ui-test-gen, GitHubUi OB, install-playwright.ps1, non-existent `BaseHelpers/`), junk root `TestAdapter/`, duplicate `testsettings.json` in `.gitignore`.

Full tables of findings — below, across the three audit directions.

---

# I. Code: naming / code-style / code-principles (180 .cs)

Criteria: **major** — violation of an explicit rule/bug risk; **minor** — suboptimal/borderline interpretation; **style** — cosmetics. Exceptions accounted for: dictionary keys in `AllureTestResultBuilder`, `[Category(...)]`, fallback values (`"Tests"`, `"Unknown"`, `"-"`).

## Major

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| major | Core\Helpers\GitHub\GitHubTestBase.cs:209 | code-style.md → Error handling ("Catch specific exceptions") | Bare `catch { ... throw; }` in `CreateScratchPullRequestAsync` — catches any exception, the type is not specified | List the specific ones (`HttpRequestException`, `TaskCanceledException` etc.) or `catch (Exception) when (...)` with an explicit justifying comment |
| major | Ui\Helper\GitHubUiTestBase.cs:17, 18, 19, 82 | code-style.md → Null safety ("Avoid null!") | 4 assignments of `null!` for `Context`, `Page`, `Browser` (+ `null!` on reset in `CloseContextAsync`) — bypasses null-safety checking, NRE on lifecycle change | Initialize in `OpenContextAsync` before use, keep nullable and check `is null` (the guard already exists in `CloseContextAsync`), or throw an early `InvalidOperationException` |
| major | Core\Helpers\Waiting\WaitHelper.cs:7, 8 | code.md → Access modifiers ("narrowest possible") | `public static readonly TimeSpan DefaultTimeout/DefaultInterval` are used only inside `WaitHelper` (lines 16–17) | Make them `private static readonly` |
| major | Core\Helpers\GitHub\GitHubTestBase.cs:122 | code.md → Methods (max ~30 lines) | `CreateScratchPullRequestAsync` ≈103 lines (122–225) — blob→tree→commit→ref→PR in one method, including try/catch-cleanup | Extract the steps into separate private methods (`CreateScratchCommitAsync`, `CreateScratchRefsAsync`, …) |
| major | Core\Reporting\TestRunReportGenerator.cs:30 | code.md → Methods | `Generate()` = 43 lines (30–73) | Extract report section building and writing into separate helpers (partially already there — `BuildHeader/BuildTests/...`) |
| major | Core\Helpers\Assertions\AssertHelper.cs:75 | code.md → Methods | `ShouldMatchRequest` ≈33 lines (75–109) | Extract the property-pair comparison into a separate method |
| major | E2E\GitHub\Tests\IssueStateMachineTests.cs:23 (90), IssueLifecycleTests.cs:24 (52), CollaboratorPermissionTests.cs:36 (62), FailedWriteTests.cs:34 (44), GitRefLifecycleTests.cs:34 (41), CommentChainTests.cs:26 (40), RateLimitDrainTests.cs:25 (35) | code.md → Methods (max ~30) | 7 E2E tests longer than 30 lines (figures — body length) | Split the chains into named step helpers in the base class (create/verify/cleanup) |
| major | CreateRefTests.cs:38,53,70,92,114,131,153,175,186; DeleteRefTests.cs:37,55; CreatePullRequestTests.cs:180; GitHubTestBase.cs:124,125; FailedWriteTests.cs:36; GitRefLifecycleTests.cs:36 (16 places) | code-principles.md → "No magic numbers" | Magic number `8` in `DataGenerator.RandomString(8)` in 16 places (same meaning — length of the random suffix); the project already has an example constant `TitleRandomLength = 8` | `private const int RandomNameLength = 8;` in base classes/tests (or a shared constant helper) |
| major | Api\GitHub\Tests\PullRequests\CreatePullRequestTests.cs:82,105,125,126,143,198,216; Api\GitHub\Tests\Issues\CreateIssueNegativeTests.cs:39,56 | code-style.md → Magic strings (2+ in code, one meaning) | Literal `"probe"` ×7 and `"Probe"` ×4 inline as test data (the same placeholder title) | `private const string ProbeTitle = "probe";` in each test class (or a shared constant class) |
| major | AllureAdapter\Helpers\AllureTestResultBuilder.cs:91 | code-style.md → Loops (sync side-effect over `List<T>` → `ForEach`) | `foreach (var category in resultParams.Categories)` with a side-effect body `labels.Add(...)`, source is `List<string>?`, body is a single statement | `resultParams.Categories.ForEach(category => labels.Add(new() { [NameKey] = "tag", [ValueKey] = category }));` |
| major | Core\Config\TestConfig.cs:41, 58; Core\Logging\TestRunWorkspace.cs:35 | code.md → Naming (bool: `Is`/`Has`/`Can`/`Should`) | Boolean members without a prefix: property `UiHeadless` (41), method `GetRequiredBool` (58), method `TryOwnRun` (35) | `IsUiHeadless`; for `GetRequiredBool`/`TryOwnRun` — a deliberate exception or renaming (for the `Try` pattern, record the exception in Rules) |
| major | Core\Models\GitHub\Responses\InvitationModelResponse.cs:20; Core\Models\GitHub\CollaboratorPermissions.cs:5,7,9,11,13 | code.md → Naming (bool prefix) | Boolean contract properties without `Is/Has/...`: `Expired`, `Admin`, `Maintain`, `Push`, `Triage`, `Pull` | Rename (`IsExpired` etc.) + `[JsonPropertyName]` from `JsonFields`; if the contract matters more — record the exception in Rules |

## Minor

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| minor | AllureNUnitAttribute.cs:134; AllureSkippedTestWriter.cs:63; GetReleasesTests.cs:42; GetTagsTests.cs:41; TestRunReportGenerator.cs:151 (+ `.Count == 0`: TestRunReportGenerator.cs:48,241,289; GetIssueCommentsTests.cs:32) | code-style.md → LINQ ("Prefer `Any()` over `Count() > 0`") | Emptiness is checked via the `Count` property of `List/Queue`. Literally the rule targets the `Count()` method; for collections `Count` is O(1) — borderline interpretation | For consistency — `Any()` / `!...Any()`; or record in Rules that the `Count` property is acceptable |
| minor | AllureAdapter\Helpers\AllureNUnitAttribute.cs:130; AllureSkippedTestWriter.cs:59 | code-style.md → Strings (IsNullOrEmpty/IsNullOrWhiteSpace) | String checks via `value.Length > 0` / `category.Length > 0` instead of `!string.IsNullOrWhiteSpace(...)` | Replace with `!string.IsNullOrWhiteSpace(...)` in `.Where(...)` lambdas |
| minor | Core\Helpers\GitHub\GitHubTestBase.cs:124,125; Api\GitHub\Tests\PullRequests\CreatePullRequestTests.cs:180 | code-style.md → Magic strings (2+ in code, one meaning) | Literal `"audit-"` (scratch branch prefix) repeats in 2+ files with one meaning; meanwhile `RefsHeadsPrefix`/`HeadsPrefix` are already extracted into constants | `private const string ScratchBranchPrefix = "audit-";` in `GitHubTestBase` |
| minor | Group-wise: ~293 uses of `response.Data!` (all Api/E2E tests, GitHubTestBase.cs:130,146,154,166,181,187) | code-style.md → Null safety (spirit of the rule) | Null-forgiving `!` after `ShouldHaveStatusCode` instead of an explicit `Data is not null` check; safe only as long as the status assert throws | Leave as is, but record the pattern in Rules, or add `Data.Should().NotBeNull()` after 2xx asserts |
| minor | Api\GitHub\Tests\Auth\AuthNegativeTests.cs and others: `private const ... = "abc"` (15 files: GetCommitsTests:18, GetIssueCommentsTests:149, GetIssueByIdTests:19, GetIssuesTests:191, GetLanguagesTests:18, GetPublicReposTests:65, GetReleasesTests:17, GetUserTests:18, GetAlbumByIdTests:20, GetCommentByIdTests:20, GetPhotoByIdTests:20, DeletePostTests:19, GetPostByIdTests:20, UpdatePostTests:24, GetUserByIdTests:20) | code-style.md → Magic strings (2+ in code, one meaning) | The same invalid value `"abc"` is extracted into a separate constant in each file with different names (`InvalidPerPage`/`NonNumericId`/`InvalidQueryValue`) — borderline against the rule "2+ times in code" vs "single-use — in its own class" | Decide in Rules: either a shared `InvalidSegment` in one place, or explicitly lock in "single-use per class" |

## Style

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| style | AllureAdapter\Helpers\AllureNUnitAttribute.cs:134; Core\Helpers\Assertions\AssertHelper.cs:118; Ui\Helper\GitHubUiTestBase.cs:111 | code-principles.md → "Empty line before return" | `return` without a blank line after a regular statement (`var distinct = ...; return ...`, `...Be(...); return;`, `var statePath = ...; return ...`) | Add a blank line before `return` |
| style | Api\TestReportSetup.cs:7–8; E2E\TestReportSetup.cs:7–8 | code.md → File Layout ("One blank line between members") | Double blank line after `namespace ...;` before the comment | Keep a single blank line |
| style | Core\Helpers\Http\GitHubRequestHelper.cs:5–8; Core\Helpers\Http\JsonPlaceholderRequestHelper.cs:5–8; E2E\GitHub\Tests\CollaboratorPermissionTests.cs:101–104; Ui\Helper\UiHelper\CommonPages\LoginPage.cs:12–15 | code-principles.md → Expression-bodied members | Single-line bodies in braces: constructors with a single call, `OneTimeTearDown` with a single `await`, `OpenAsync` with a single `await` | `public GitHubRequestHelper() => UseGitHub();`, `public async Task OneTimeTearDown() => await CleanupInvitationAsync(_createdInvitationId);`, `internal async Task OpenAsync() => await _page.GotoAsync(...);` |
| style | CreateRefTests.cs:202–204; CreateCommentTests.cs:111–112; CreateIssueVisibilityTests.cs:50–51; GetIssuesTests.cs:173–174; CreatePullRequestTests.cs:240–241; MergePullRequestTests.cs:68–69; CreatePostTests.cs:208–209; E2E: FailedWriteTests.cs:53–54, GitRefLifecycleTests.cs:53–54, IssueStateMachineTests.cs:42–43 (10 files total) | code-style.md → Strings (concatenation) | Because-messages of asserts are glued from interpolations with `+` across several lines (`$"..." +$"..." +$"..."`) — a borderline case: interpolation is there, concatenation too | A single multi-line `$"..."` (C# 11 raw string `"""..."""`) or assemble the template in a private helper |
| style | Core\Helpers\GitHub\GitHubTestBase.cs (EOF) | code.md → File Layout ("No trailing whitespace"/file cleanliness) | The file does not end with a line break (the only such file out of 180) | Add `\n` at the end of the file |

## Checked and no violations found (summary I)

- **Async**: only `ExecuteAsync`/`ExecuteAsync<T>`; no `Execute(`, `async void`, `.Result`/`.Wait()`/`GetAwaiter().GetResult()` synchronization.
- **Error handling**: no `catch (Exception)`; `try/catch` in TestRunReportGenerator/TestRunWorkspace/RunCleanupAsync uses concrete types and logging; exceptions are not used for flow control; no `nameof` violations.
- **LINQ**: no `Count() > 0`, `Where(...).FirstOrDefault()`; `.ToList()` in chains — only materialization before reuse — acceptable.
- **Loops**: all `foreach` over `List<T>` in synchronous bodies are either async-teardown or iterators/`yield` or lazy sources — per the rule's exceptions; the only extraction — AllureTestResultBuilder:91 (major above).
- **Regex**: all patterns in `private static readonly Regex` with a describing comment, `@"..."` and `RegexOptions.Compiled`; no inline `Regex.IsMatch/Match/Replace`.
- **Structure/naming**: file-scoped namespaces everywhere; one type per file; no explicit types replaceable by `var`; no single-letter lambdas; no abbreviations; no bare `if/foreach/while` without `{}`; no nested ternaries; `using NUnit.Framework` absent in Api/E2E (global import); no trailing whitespace; `private` fields in `_camelCase`, constants PascalCase; no parameters >5; guard clauses applied; no SOLID placement violations found.
- **Magic numbers**: other numeric literals are either in `const` fields or in `[Description]`/`HttpStatusCode` — except `RandomString(8)` (major above).

_Note: the dead `using` check was performed heuristically; the final verdict on IDE0005 comes only from `dotnet format --verify-no-changes`._

---

# II. Code: models / assertions / test-practices / categories / comments (~200 .cs)

**Method:** the rules were read in full; all `.cs` files were reviewed with a combined scan (scripts over attributes/methods/patterns) + manual verification of sample findings by reading files.

## MAJOR

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| major | `Api/GitHub/Tests/Issues/CreateIssueVisibilityTests.cs:33`; `Api/GitHub/Tests/IssueComments/CreateCommentTests.cs:67-83` | test-practices → Read-after-write ("Mandatory for every POST flow", "Baseline GET + POST go in [OneTimeSetUp]; tests only read and assert") | The mandatory canonical pair `*_ReturnsCreatedRecord` (Smoke) + `*_CreatedRecordMatchesRequest` (Regression) with baseline in `[OneTimeSetUp]` is missing for GitHub POST flows (CreateIssue, CreateRef, CreatePullRequest); in CreateComment the baseline GET (67) + POST (71) run inside `[Test]`, and `MatchesRequest` (48) — Smoke and against the POST response, not the GET read-back. Partially covered by E2E chains (CommentChainTests.cs:62 HaveCount, IssueLifecycle, PullRequestFlow) with different names/categories; for JSONPlaceholder the exception is documented (JP-002) | Either add the missing tests in canonical form (baseline+POST in OneTimeSetUp, split into Smoke/Regression), or record in Rules/test-practices.md the deviation "the pattern is applied in E2E chains" (the rule currently contradicts the project's practice) |
| major | see the table below (35 files, ~100 methods — ONE topic) | test-practices: "Status check before every `.Data` dereference" (p. 12) | Test bodies dereference `response.Data` (incl. `Data!`, `.Should()`, aggregates) before any `ShouldHaveStatusCode`/`ShouldHaveError`; on a 500 the failure will be an NRE/blurry message instead of "expected 200, got 500". `RequestHelper` does not check the status itself. The scan was confirmed by manual reads (CreatePostTests.cs:55, GetRateLimitTests.cs:45, GetIssuesTests.cs:73, GetAlbumsByUserTests.cs:50 etc.) | Before the first access to `.Data` add `response.ShouldHaveStatusCode(...)` (for negatives — `ShouldHaveError`) |

### Files in the "status-first" topic (major, one pattern)

| File:line |
|---|
| `Api/GitHub/Tests/AuthRepos/GetAuthenticatedUserReposTests.cs:46` |
| `Api/GitHub/Tests/Branches/GetBranchByNameTests.cs:67,78` |
| `Api/GitHub/Tests/Branches/GetBranchesTests.cs:51,110,122` |
| `Api/GitHub/Tests/IssueComments/GetIssueCommentsTests.cs:105,116` |
| `Api/GitHub/Tests/Issues/GetIssueByIdTests.cs:74,125,138` |
| `Api/GitHub/Tests/Issues/GetIssuesTests.cs:73,84,96` |
| `Api/GitHub/Tests/PublicRepos/GetPublicReposTests.cs:49` |
| `Api/GitHub/Tests/PullRequests/GetPullRequestCommitsTests.cs:67,80,114` |
| `Api/GitHub/Tests/PullRequests/GetPullRequestFilesTests.cs:68,107,118` |
| `Api/GitHub/Tests/PullRequests/GetPullRequestsTests.cs:51,62,74` |
| `Api/GitHub/Tests/RateLimit/GetRateLimitTests.cs:45,55,78,92` |
| `Api/GitHub/Tests/Repos/GetRepositoryTests.cs:54,114,128` |
| `Api/GitHub/Tests/UserRepos/GetUserReposTests.cs:51` |
| `Api/GitHub/Tests/Users/GetUserTests.cs:48,92,103` |
| `Api/JsonPlaceholder/Tests/Albums/GetAlbumByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Albums/GetAlbumsByUserTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Albums/GetAllAlbumsTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Comments/GetAllCommentsTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Comments/GetCommentByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Comments/GetCommentsByPostTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Photos/GetAllPhotosTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Photos/GetPhotoByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Photos/GetPhotosByAlbumTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Posts/CreatePostTests.cs:45,55,67,77,87` |
| `Api/JsonPlaceholder/Tests/Posts/DeletePostTests.cs:41` |
| `Api/JsonPlaceholder/Tests/Posts/GetAllPostsTests.cs:85,118,129` |
| `Api/JsonPlaceholder/Tests/Posts/GetPostByIdTests.cs:81,92,169` |
| `Api/JsonPlaceholder/Tests/Posts/UpdatePostTests.cs:52,74,85,96,119` |
| `Api/JsonPlaceholder/Tests/Todos/GetAllTodosTests.cs:85,118` |
| `Api/JsonPlaceholder/Tests/Todos/GetTodosByUserIdTests.cs:69,80,91,122` |
| `Api/JsonPlaceholder/Tests/Users/GetAllUsersTests.cs:68,89,100` |
| `Api/JsonPlaceholder/Tests/Users/GetUserAlbumsTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Users/GetUserByIdTests.cs:66,77,155` |
| `Api/JsonPlaceholder/Tests/Users/GetUserPostsTests.cs:50,61,72` |
| `Api/JsonPlaceholder/Tests/Users/GetUserTodosTests.cs:50,61,72` |

*Note: `E2E/GitHub/Tests/CommentChainTests.cs:36` (`_createdIssueNumber = issue.Data.Number` inside `if (Data is not null)`) — this is the prescribed register-then-assert, NOT a violation. E2E and Ui are otherwise clean on this rule.*

## MINOR

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| minor | `Core/Helpers/GitHub/GitHubParamHelper.cs:52` | models.md → JSON field names (wire keys of test helpers use JsonFields; copies are banned) | `ParamHelper.Query("state", ...)` — inline literal, while `JsonFields.State` already exists | Use `JsonFields.State` |
| minor | `Core/Helpers/GitHub/GitHubParamHelper.cs:29,34`; `Api/JsonPlaceholder/Helpers/JsonPlaceholderParamHelper.cs:13,16` | models.md → JSON field names ("New wire name → add the constant first") | Wire-query keys `"page"`, `"per_page"`, `"postId"`, `"albumId"` are set inline; no constants in JsonFields | Add constants to the relevant `JsonFields.cs`, replace in the helpers |
| minor | `Core/Helpers/Params/ParamHelper.cs:7`; `Core/Helpers/GitHub/GitHubTestBase.cs:18` | models.md → "`TitleKey`-style aliases are banned; the constant IS the key" | Aliases `IdKey = "id"` and `SinceParamKey = "since"` live in the helpers; `id`/`since` — wire names | Move `"id"` (Generic) and `"since"` (GitHub) into `JsonFields`, use directly (as already done for `UserIdParam`) |
| minor | `Api/GitHub/Tests/Issues/CreateIssueNegativeTests.cs:17-18` | assertions.md: "message constants live in `Core/Config/GitHubErrors.cs`" | Constants of documented errors (`MissingTitleErrorMessage`, `LabelsNullErrorMessage`) are declared locally in the test class | Move to `Core/Config/GitHubErrors.cs` |
| minor | `Api/GitHub/Tests/GitRefs/CreateRefTests.cs:51`; `Api/GitHub/Tests/Languages/GetLanguagesTests.cs:34` | categories.md + assertions.md (ContractCheck → `ShouldHaveValidContract()`) | Tests with `[Category("ContractCheck")]` do not call `ShouldHaveValidContract()` — manual comparisons/own round-trip | For CreateRef: change the category or add `ShouldHaveValidContract`; for Languages — formalize the justification (Dictionary) in the rule or use the helper |
| minor | `Api/GitHub/Tests/IssueComments/CreateCommentTests.cs:48`; `Api/GitHub/Tests/GitRefs/CreateRefTests.cs:51` | categories.md → Category-to-test mapping / Boundary (ShouldMatchRequest-invariant → Regression) | `CreateComment_MatchesRequest` — `Smoke`; `CreateRef_MatchesRequest` — `ContractCheck`. In the project, analogues (`CreatePost_ShouldMatchRequest`, `UpdatePost_Put_ShouldMatchRequest`) — `Regression` | Reclassify both as `Regression` |
| minor | `Api/GitHub/Tests/GitRefs/CreateRefTests.cs:77`; `E2E/GitHub/Tests/CollaboratorPermissionTests.cs:46,51,81` | test-practices Step 3 / code: magic strings → `{Service}Endpoints.cs` | Inline literals in assertion values: `"commit"`, `"admin"`, `"read"` (analogues `StateOpen`/`StateClosed` already extracted) | Extract into `GitHubEndpoints` (or `GitHubErrors`) |
| minor | `Api/GitHub/Tests/PullRequests/GetPullRequestsTests.cs:30` | test-practices → Guarantee Data ("If POST is unavailable — use [Ignore]") | `[Ignore("Creating PR requires branch + commit — too complex for Setup")]` — the reason is obsolete: `GitHubTestBase.CreateScratchPullRequestAsync` exists and is used by neighboring tests; active neighbors (`HasValidFields`:51 etc.) rely on that data | Remove Ignore and wire the scratch-pipeline into `[OneTimeSetUp]` (guarantee data), or update the reason with an actual reference to the limitation |
| minor | `Api/GitHub/Tests/IssueComments/CreateCommentTests.cs:21-24` | test-practices → Test data for write ("Vary ≥2 dimensions … If one dataset is intentionally enough, document why") | A single `static readonly _testComment` — one body for all write tests of the class; no comment/documentation on why one dataset is enough (only the per-run `RandomString` varies) | Vary the body across tests (or record the justification in the class) |

## STYLE

| Severity | File:line | Rule | Issue | Suggested fix |
|---|---|---|---|---|
| style | `Ui/Helper/BrowserHelpers/BrowserPw.cs:6-8` | comments.md (no comments by default; only regex/TODO/non-obvious WHY) | The header describes structure/what the code does ("every page-object receives the wrapper in its constructor; pages are exposed here as properties…") — borderline against the "what the code does" prohibition | Trim to WHY (why the pattern is needed) or remove |
| style | `Rules/models.md:82` ↔ `Rules/assertions.md:18-19` + all `Core/Models/**` | models.md "Don't add validation attributes" vs assertions.md, which relies on `[RequiredField]`/`[PositiveId]`/`[ValueRange]` | Direct rule contradiction: models make widespread use of validation attributes (ContractCheck infrastructure), which models.md forbids | Sync models.md (allow contract-check attributes) — a document edit, not code |

## Checked — no violations found (summary II)

- **models.md:** suffixes/layout (Requests/Responses/root, Generic) — clean; constructors/methods/`init`/inline `[JsonPropertyName]`/attributes on one line — 0 (scan of all 60 files `Core/Models/**`); namespaces `Core.Models.{Service}` — OK.
- **assertions.md:** NUnit `Assert.*`/`Assert.Multiple` — 0; `.Should().Be(HttpStatusCode…)` instead of `ShouldHaveStatusCode` — only inside `AssertHelper` (legitimate); `JsonElement`/`TryGetProperty` in tests — 0; `ShouldMatchRequest` applied in PATCH echo and create-comment; nested-shape exceptions accounted for; guard `if (Count > 0)` only for documented-empty with Smoke tests of the empty state; aggregate rule — OK.
- **categories.md:** scripted check of all `*Tests.cs`: exactly one service category per class, exactly one check-type per `[Test]` — 0 deviations; `[Ignore]` tests are categorized.
- **test-practices.md:** `[OneTimeTearDown]` — last member in all 23 classes with teardown; no status-assert and `.Data` in teardown; helper methods in `*Tests.cs` — 0; `finally`-cleanup — 0; all `[OneTimeSetUp]` — status immediately after the call; register-then-assert observed; dynamic non-existent IDs: GitHub → `maxId + NonExistentIdOffset` (100), JP → `maxId + 1`, static 999/1000 — 0; WaitHelper — only where needed, diagnostics present in messages; fake-API tests `[Ignore]` with references to JP-001/002/004; write data — fictional, via `DataGenerator.RandomString`; cleanup calls only in OTD/setup.
- **comments.md:** 65 single-line comments checked — regex explanations and non-obvious WHY are acceptable; `// ====`, AAA markup, `TODO`/`FIXME`, `/* */`, `///` — 0. The only borderline — BrowserPw (see STYLE).

---

# III. Document and configuration consistency

## 1. AGENTS.md ↔ Rules/*.md

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `AGENTS.md:9` (Project Structure → Ui/) ↔ `Rules/ui-testing.md:34-36` | The claim "authorization via storageState (bootstrap = `[Explicit]` test)" does not match the code: in `Ui/` there is not a single `[Explicit]` attribute and no `StorageStateBootstrap` test (repo grep — 0 matches in code). In fact there is an automated Smoke sign-in test `Ui/Test/GitHubStream/Login/LoginSignInTests.cs` (`IsStorageStateEnabled => false`, `SaveStorageStateAsync()` after assert). Two locally-"old" phrasings in AGENTS and Rules conflict with reality | Rephrase in both places: the bootstrap state is refreshed by the automated test `LoginSignInTests.Login_SignIn_SavesStorageState` (or return the `[Explicit]` test to the code and keep the text) |
| major | `AGENTS.md` (Git: "Before committing: `dotnet format --verify-no-changes` + `dotnet test`") ↔ `AGENTS.md` (AI Onboarding: "`/commit` — formatting + HealthCheck + commit") ↔ `.mimocode/skills/commit/SKILL.md:53-59` | Within AGENTS there are two different pre-commit gate phrasings: full `dotnet test` vs only `--filter Category=HealthCheck` (the skill actually runs HealthCheck). A reader will conclude that the whole suite passes before commit | Lock in one variant (e.g. "format + HealthCheck gate, full run — `/test`") in the AGENTS Git section and Rules |
| minor | `AGENTS.md:17` ("0 and -1 — safe static IDs") ↔ `Rules/test-practices.md:29` ("Never hardcode 999, 1000, or **any static number**") | The absolute phrasing of the rule conflicts with the allowed static invalid values 0/-1 (the same `test-practices.md:31` allows explicit 0/negative) — an opportunity for mutually opposite edits at review | Clarify item 29: statically hardcoded *potentially existing* IDs are forbidden; 0/-1 is the exception |

## 2. Rules/ui-testing.md and ui-test-gen/SKILL.md ↔ reality of Ui/

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `Rules/ui-testing.md:34` | "Tests … **never log in** or log out" — `LoginSignInTests` signs in with the pair `TestConfig.UiLogin/UiPassword` on every run | Describe the actual model: regular tests read the storageState, a separate login test works in an anonymous context |
| major | `Rules/ui-testing.md:35` | "The one-time **manual** login is the `[Explicit]` bootstrap test (`dotnet test --filter StorageStateBootstrap`)" — no such test exists in the project; the filter finds nothing | Replace with the actual state-refresh path (`dotnet test --filter LoginSignInTests`) |
| major | `Rules/ui-testing.md:36` | "`GITHUB_UI_PASSWORD` lives in `.env` **only if a password flow is ever added**" — the password flow has already been added (`LoginPage.SignInAsync`, mandatory env `GITHUB_UI_PASSWORD` in `TestConfig.cs:44-45`, requirement in `backups/SETUP.md` Part 4) | Remove "if ever added", directly require `GITHUB_UI_EMAIL`/`GITHUB_UI_PASSWORD` |
| major | `Rules/ui-testing.md:46-47` | Cleanup "via Core API helpers (`CleanupIssueAsync`, `CleanupCommentAsync` → `RunCleanupAsync`)" — these members do not exist in `Ui/`: `GitHubUiTestBase` does not inherit `GitHubTestBase`/`RequestHelper` (grep over `Ui/` — 0 matches). The rule describes non-existent wiring | Document the actual mechanism (inheritance from `GitHubTestBase` or composition) or add the wiring to the fixture |
| major | `.mimocode/skills/ui-test-gen/SKILL.md:17` | Hard rule 2: "No login/logout in tests … login is the `[Explicit]` bootstrap test only" — incorrect for the same reason | Update for `LoginSignInTests` |
| major | `.mimocode/skills/ui-test-gen/SKILL.md:205,207` (Troubleshooting) | "run `[Explicit]` bootstrap test", "`dotnet test --filter StorageStateBootstrap`" — the filter finds no tests; the agent will walk the user through a non-existent command | Replace with the actual sign-in test |
| major | `.mimocode/skills/ui-test-gen/SKILL.md:136-158` (Step 4 template) | The UI test template calls `Get<List<IssueModelResponse>>(...)`, `ShouldHaveStatusCode`, `TestRepoParam()`, `CleanupIssueAsync(id)` — none of these members exist in the Ui fixture → the generated code will not compile | Either wire the API infrastructure into `GitHubUiTestBase` or rewrite the template against the real members |
| minor | `Rules/ui-testing.md:15`, `documentation/FILE_STRUCTURE.md:326` | The structure section lists `Ui/Helper/UiHelper/BaseHelpers/` — the directory does not exist on disk (only `CommonPages/`, `GitHubStream/` with `.gitkeep` exist) | Remove the line or create the directory |
| minor | `Rules/ui-testing.md:60`, `ui-test-gen/SKILL.md:72` | Examples `[Description("UI-3.2 Create issue via UI")]` — `GitHubUiObservableBehaviour.md` has no UI-3.x section; only UI-1 is numbered, the other pages are unnumbered | Introduce a continuous UI-2… numbering or fix the examples to the existing sections |
| minor | `ui-test-gen/SKILL.md:54` | Preconditions reference `.mimocode/plans/ui-playwright-github.md` — `plans/` is gitignored, in a clone the link is dead | Reference a tracked document or remove the path |

No findings: skill file conventions (`Ui/Test/GitHubStream/{area}/{area}{flow}Tests.cs`, namespace `Ui.Test.GitHubStream.Login`, `[Category("GitHubUi")]`) match the disk; `UiTimeoutMs=15s`, `TestResults/ui-artifacts/{name}.png|.zip`, `CommonPages/{LoginPage,HeaderPage}` — match.

## 3. documentation/FILE_STRUCTURE.md ↔ file tree

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| minor | `documentation/FILE_STRUCTURE.md:335-347` (Rules/) | Missing entry `ui-testing.md` (12 files on disk, 11 in the tree) | Add a line |
| minor | `FILE_STRUCTURE.md:38-50` (.mimocode/skills/) | Missing `ui-test-gen/SKILL.md` (13 skills on disk, 12 in the tree) | Add a line |
| minor | `FILE_STRUCTURE.md:352-378` (documentation/) | Missing `GitHubUiObservableBehaviour.md` — the main OB for UI tests is not reflected | Add a line |
| minor | `FILE_STRUCTURE.md:51-54` (Scripts/) | Missing `install-playwright.ps1` (present on disk; mentioned in ui-test-gen Preconditions) | Add a line |
| minor | `FILE_STRUCTURE.md:315-330` (Ui/) | Non-existent `BaseHelpers/` listed; the actual folders `CommonPages/` (2 files) and `GitHubStream/` (only `.gitkeep`) are described as "page-objects: {Area}/{Area}Page.cs" — GitHubStream has no page-objects yet | Bring the tree in line with fact |
| minor | `FILE_STRUCTURE.md` (Root) | Root entries not reflected: `TestAdapter/` (on disk only `obj/`, no `.csproj` — a junk directory), `.auth/`, `.playwright-cli/` (meanwhile `allure-results/`, `TestResults/` are listed) | Document or remove `TestAdapter/` (workflow rule #6 on deleting gitignored files) |
| style | `FILE_STRUCTURE.md:26-37` (.mimocode/) | `hooks/safety-commit.ts`, `package.json`, `package-lock.json`, `.mimocode/.gitignore` are shown as regular entries although not tracked by git (verified with `git ls-files`: they are absent; ignore is via `.mimocode/.gitignore`); the "not tracked" marker is only on `node_modules`, `plans`, `reviews` | Mark "not tracked" consistently |

Exact matches: sections Core/, Api/, E2E/, AllureAdapter/, Bugs/, Katas/ — full match with disk.

## 4. documentation/*ObservableBehaviour.md ↔ tests

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `documentation/GitHubUiObservableBehaviour.md:25` (What AI does #7) | "Never logs in/out inside tests — storageState only; login = `[Explicit]` bootstrap" contradicts §"Page: Login (UI-1)" of the same file ("the login test overrides `IsStorageStateEnabled => false`") and `LoginSignInTests` itself — internal ambiguity in the OB | Keep a single factual phrasing |
| major | `GitHubObservableBehaviour.md` ↔ `Api/GitHub/Tests/**` | `[Description]` numbering does not match the OB section numbering: `GetIssuesTests` "3.x" vs §6; `GetIssueByIdTests` "10.x" vs §7; `GetIssueCommentsTests` "11.x" vs §8; `GetBranchesTests` "4.x" vs §10; `GetPullRequestsTests` "5.x" vs §9; `GetAuthenticatedUserReposTests` "9.x" vs §5; `GetUserTests` "2.x" vs §3; `GetPublicReposTests` "8.x" vs §2; `GetUserReposTests` "7.x" vs §4; `GetRateLimitTests` "6.x" vs §16 (only §1/1a/3a/11/12-15b and write-§17-26 match) | Renumber Descriptions per OB (or lock in the canonical numbering in Rules/test-practices) |
| major | `Api/GitHub/Tests/Auth/AuthNegativeTests.cs` ↔ `Api/GitHub/Tests/Contributors/GetContributorsTests.cs` | Collision: both use the "12.x" prefix — `12.1` means either "GET /user/repos invalid token 401" or "GET /contributors 200" (OB §12 = contributors) — references in Description are ambiguous | Renumber one of the groups |
| minor | `GitHubUiObservableBehaviour.md:40-54` | Sections "Repository landing", "Issues list", "Issue detail", "Issue create form" are unnumbered (no UI-2…UI-5), although Rules/skills require `UI-N`-style references | Number the sections when adding verification |
| minor | `GitHubUiObservableBehaviour.md:7`, `GitHubTestingStructure.md:23` | References to `.mimocode/plans/*` — the directory is gitignored, in a clean clone the links die | Reference tracked docs |
| minor | `E2E/GitHub/Tests/*` (`E2E-7`, `E2E-8`, `E2E-9`) | `[Description("E2E-N …")]` prefixes are not described in any document (grep `E2E-\d` over documentation/ — 0) — numbering goes nowhere **(superseded: documented 1:1 in `GitHubTestingStructure.md` §6 since the E2E-10 close-out)** | Add E2E-N sections to the document or change the format |
| minor | `JsonPlaceholderObservableBehaviour.md` ↔ `Api/JsonPlaceholder/Tests/**` | Three mismatched numbering schemes: Descriptions restart in each group (`GetAllComments` "1.1", `CreatePost` "4.1", `GetAllTodos` "7.1"), JP OB — global §1-22 (POST /posts = §19), TestPlan inventory — 1.1-6.5 (POST /posts = 1.3) | Lock in a canonical (e.g. Description = OB section number) |

No findings: statuses `draft — no verified locators yet` are on all unverified UI OB sections; markers `verified {date}` are on all UI-1 locators; `[Description("UI-1 Ordinary login…")]` in `LoginSignInTests.cs:19` correctly references the section "Page: Login (UI-1)".

## 5. testsettings.example.json ↔ testsettings.json

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| — | `testsettings*.json` | **No findings**: keys of all sections (`MaxResponseTimeMs`, `SkipSslValidation`, `JsonPlaceholder`×6, `GitHub`×5, `Ui`×4) are identical, the `Ui` section is present in both | — |

## 6. backups/SETUP.md ↔ reality

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| minor | `backups/SETUP.md` Part 6 (`Scripts/` tree) | "Scripts (allure-report.ps1, test-coverage.ps1)" — `install-playwright.ps1` and `normalize-eol.ps1` are not mentioned (the latter is mandatory per `/commit` Step 7) | Add both scripts |
| minor | `backups/SETUP.md` Part 6 (`Core/Config/` tree) | Only `JsonPlaceholderEndpoints.cs`, `GitHubEndpoints.cs` listed — no `TestConfig.cs` (which itself is referenced in Part 4) and `GitHubErrors.cs` | Add them |
| minor | `backups/SETUP.md` Part 7.2 (skill table) | 11 of 13 skills: `/audit` and `/review` missing | Add rows |
| style | `backups/SETUP.md` Part 6 (`documentation/` tree) | No `Katas/`, `token-budget.md`; the root tree lacks `backups/`, `TestResults/` | Complete the tree |
| style | `backups/SETUP.md` Part 3.2 | "The project folder **already has** `.mimocode/mimocode.jsonc`" — the file is git-tracked, but `hooks/`, `package.json`, `package-lock.json` next to it are untracked; the phrasing "already has" misleads on a clone | Clarify what is tracked and what is restored from `backups/` |

No findings: environment variables `GITHUB_PAT`, `GITHUB_UI_EMAIL`, `GITHUB_UI_PASSWORD` are mentioned and match `Core/Config/TestConfig.cs:32,44,45`; token reading order (JSON → env → `.env`) matches the code; `GitHubUiObservableBehaviour.md` is present both in the Part 6 tree and in the OB listing of Part 12 (synchronized in commit 504ef48); commands (`dotnet test`, HealthCheck filter, `/test`, `/commit`) match the skills.

## 7. .mimocode/skills/* ↔ Rules (One canonical source, `Rules/test-practices.md:84`)

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| major | `.mimocode/skills/audit/SKILL.md:40` | "Magic strings (**3+** occurrences → const)" contradicts `Rules/code-style.md:38` ("**2+** times") — the audit will miss violations | Reference the rule, do not duplicate the threshold |
| major | `.mimocode/skills/e2e-test-gen/SKILL.md:126-170` | Mandatory template with `[SetUp]`/`[TearDown]` per test and cleanup in per-test `TearDown` contradicts `Rules/test-practices.md:44-48` and AGENTS (cleanup only in `[OneTimeTearDown]` as the last member, fixture-registry, register-then-assert); the actual E2E code (`IssueLifecycleTests`) uses `[OneTimeTearDown]` | Rewrite the template for the fixture-registry |
| major | `.mimocode/skills/e2e-test-gen/SKILL.md:174-187` (Step 5) | "add helper methods (`CleanupComments`)" in the test class contradicts `Rules/test-practices.md:17` ("No helper methods inside `*Tests.cs`") and hard rule 5 of ui-test-gen itself | Remove or move to base/helpers |
| major | `.mimocode/skills/api-test-gen/SKILL.md:48,120` | Param helper path `Api/Helpers/{Service}ParamHelper.cs` — the `Api/Helpers/` directory does not exist (in fact `Api/JsonPlaceholder/Helpers/JsonPlaceholderParamHelper.cs`; for GitHub — `Core/Helpers/GitHub/GitHubParamHelper.cs`) | Fix the path convention |
| major | `.mimocode/skills/api-test-gen/SKILL.md:49,148` | Namespace `Api.{Service}.Tests` — actual namespaces follow subfolders: `Api.JsonPlaceholder.Posts`, `Api.GitHub.Issues` etc. | Fix to `Api.{Service}.{Category}` |
| major | `.mimocode/skills/api-test-gen/SKILL.md:81-83,131,283` | Model suffixes "`Model` for response, `Request` for request" + example `PostModel` vs the canon `ModelResponse`/`ModelRequest` (`Rules/models.md`, AGENTS, gredja-rules) — generation will violate the naming rule immediately | Bring in line with `ModelResponse`/`ModelRequest` |
| major | `.mimocode/skills/review/SKILL.md` (Step 2, file list) | Task "Read ALL .cs files", but the list contains non-existent `Ui/Tests/*.cs` and `Ui/AllureGlobalSetup.cs` (in fact `Ui/Test/**`, `Ui/TestReportSetup.cs`) and does not contain `Api/GitHub/Tests/**`, `E2E/**` — "full review" silently skips half the repo | Fix the paths and complete the list |
| minor | `.mimocode/skills/api-test-gen/SKILL.md:268-269` | "`[SetUp]`/`[TearDown]` are for E2E tests" — shifts the contradiction with Rules (cleanup — only `[OneTimeTearDown]`) onto e2e-test-gen | Remove or reference the current pattern |
| minor | `.mimocode/skills/api-test-gen/SKILL.md:214,224` | `{Service}Endpoints.MaxResponseTimeMs` / `{Service}Endpoints.Expected{Endpoint}Count` — `MaxResponseTimeMs` lives in `TestConfig` (testsettings), not in Endpoints classes | Fix the references |
| minor | `.mimocode/skills/api-test-gen/SKILL.md:47` | Tests path without a category subfolder: `Api/{Service}/Tests/{Endpoint}Tests.cs` vs actual `Api/{Service}/Tests/{Category}/*.cs` | Clarify the pattern |
| minor | `.mimocode/skills/e2e-test-gen/SKILL.md:216-217` | The table "api-test-gen (read-only) / **GET only**" is stale: `Api/` contains write tests (CreatePost, CreateIssue*, UpdateIssue, Create/DeleteComment, PR-create/update/merge, git refs) | Remove "GET only" |
| minor | `.mimocode/skills/gredja-rules/SKILL.md:17-28` | The Rule Files table skips `code-style.md`, `code-principles.md`, `test-practices.md`, `categories.md`, `ui-testing.md` — the entry point does not lead to the full rule set | Complete the table |
| minor | `.mimocode/skills/gredja-rules/SKILL.md` (Key Rules) | "`Id` **always non-nullable**" vs `Rules/models.md:33-35` (value types — `?` if the JSON field can be null/absent) | Soften the phrasing |
| minor | `.mimocode/skills/review-pr/SKILL.md:31` | "Models: Model/Request suffix" — vague/stale relative to `ModelResponse`/`ModelRequest` | Clarify |
| style | `review/`, `review-commit/`, `audit/`, `gredja-rules/` | Inline copies of Rules phrasings inside skills — drift has already happened (audit 3+ vs 2+); the rule "One canonical source per guidance topic" (test-practices.md:84) is violated by a second source of rules | Replace copies with `Rules/*.md` references |

## 8. .gitignore ↔ root artifacts

| Severity | File | Issue | Suggested fix |
|---|---|---|---|
| style | `.gitignore:41` and `.gitignore:48` | `testsettings.json` is duplicated: the "Secrets" section and a separate line `/testsettings.json` | Keep one entry |
| style | `.mimocode/.gitignore` | Ignores `hooks/`, `package.json`, `package-lock.json`, `.gitignore` — meanwhile `documentation/FILE_STRUCTURE.md` shows them as regular entries (see §3) | Sync the "not tracked" marks |

Checked — **all target paths are covered**: `.playwright-cli/` (line 51), `.auth/` (54), `TestResults/` (17, incl. `Api/TestResults`, `E2E/TestResults`), `.env` (33), `testsettings.json` — `git check-ignore` confirms; also covered `allure-results/`, `bin/obj`, `.mimocode/plans|reviews`, `.mimocode/.cron-lock`.

---

**Section III statistics:** 24 major, 24 minor, 6 style. The main cross-cutting defect — the "old" UI authorization model (`[Explicit] StorageStateBootstrap`, "tests never log in"), stuck simultaneously in `AGENTS.md`, `Rules/ui-testing.md`, `ui-test-gen/SKILL.md` and `GitHubUiObservableBehaviour.md`, while the automated `LoginSignInTests` exists on disk; the second most dangerous cluster — stale paths/namespaces/suffixes in `api-test-gen` and the per-test cleanup template in `e2e-test-gen`, contradicting Rules and the actual code.


