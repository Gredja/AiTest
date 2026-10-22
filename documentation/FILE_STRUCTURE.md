# File Structure — Gredja

> Auto-maintained. Update after any structural change.

## Root

```
Gredja/
├── README.md                     # project overview (rendered by GitHub as the homepage)
├── AGENTS.md
├── Gredja.slnx
├── Directory.Build.props          # File.TestLogger config (VSTestLogger, temp workspace dir)
├── .runsettings                  # VS Test Explorer: parallel sources → one report per run (CLI ignores it)
├── allureConfig.json
├── testsettings.json               # not tracked — local overrides (falls back to the example)
├── testsettings.example.json       # tracked template — build/tests work from a clean clone
├── global.json                     # pinned SDK (10.0.401, rollForward: latestFeature)
├── .editorconfig                   # IDE0005/IDE0007/IDE0161 — dotnet format enforces Rules/code.md
├── .env                          # secrets (not tracked)
├── .gitattributes                # line ending normalization
├── .gitignore
├── .graphifyignore
├── .github/
│   └── workflows/              # CI: pr-checks.yml (required PR-gate) + nightly-tests.yml (cron, main only)
├── .auth/                       # not tracked — Playwright storageState
├── .playwright-cli/             # not tracked — local CLI cache
├── allure-results/              # not tracked
├── research/                    # not tracked — deep-research workspaces (gitignored); one-off, archived
├── output/                      # not tracked — one-off generator/CLI dumps (safe to clean)
├── TestResults/                 # not tracked — ONLY TestRunReport-*.md (raw logs in %TEMP%\GredjaTestRun)
├── .mimocode/
│   ├── .gitignore
│   ├── mimocode.jsonc
│   ├── hooks/
│   │   └── safety-commit.ts
│   ├── node_modules/             # not tracked
│   ├── package.json
│   ├── package-lock.json
│   ├── reviews/                  # not tracked
│   ├── plans/                    # not tracked
│   ├── scripts/
│   │   └── gh-pr-create.ps1
│   └── skills/
│       ├── api-test-gen/SKILL.md
│       ├── coverage/SKILL.md
│       ├── e2e-test-gen/SKILL.md
│       ├── audit/SKILL.md
│       ├── commit/SKILL.md
│       ├── gredja-rules/SKILL.md
│       ├── review/SKILL.md
│       ├── review-commit/SKILL.md
│       ├── review-pr/SKILL.md
│       ├── test/SKILL.md
│       ├── test-report/SKILL.md
│       ├── ui-test-gen/SKILL.md
│       ├── merge/SKILL.md
│       ├── nightly-check/SKILL.md
│       └── update-docs/SKILL.md
├── Scripts/
│   ├── allure-report.ps1
│   ├── install-playwright.ps1    # playwright install chromium (+ deps)
│   ├── locator-dump.mjs          # one-shot CDP dump of role+accessible name (THE locator-extraction tool, hard rule)
│   ├── moodledemo-discover-langs.ps1  # regenerate moodledemo expected-langs.json from the live language switcher
│   ├── normalize-eol.ps1         # worktree LF→CRLF per .gitattributes — run before `git add -A`
│   ├── test-coverage.ps1
│   └── test-parallel-rerun.ps1   # two-pass: 8 workers → failed tests rerun in 1 (used by /test and nightly)
├── education/
│   └── TESTING_PLAYBOOK.md       # portable project-agnostic testing playbook (EN) — methodology deltas folded at every commit
└── backups/
    ├── SETUP.md
    └── mimocode-project.jsonc
```

## Core/

```
Core/
├── Core.csproj
├── Attributes/
│   ├── PositiveIdAttribute.cs
│   ├── RequiredFieldAttribute.cs
│   └── ValueRangeAttribute.cs
├── Config/
│   ├── GitHubEndpoints.cs
│   ├── GitHubErrors.cs            # documented error message constants (verified vs live API)
│   ├── GitHubHeaders.cs            # X-RateLimit-* header names (shared Api/E2E)
│   ├── JsonPlaceholderEndpoints.cs
│   ├── MoodledemoEndpoints.cs
│   └── TestConfig.cs
├── Helpers/
│   ├── Assertions/
│   │   └── AssertHelper.cs        # ShouldHaveValidContract, ShouldHaveValidFields, etc.
│   ├── Data/
│   │   ├── DataGenerator.cs       # RandomString/RandomIntExclusive for varying write payloads
│   │   └── TestDataConstants.cs    # shared test-data lengths/values — one meaning, one declaration
│   ├── GitHub/
│   │   ├── GitHubCleanupHelper.cs # shared cleanup core (Api/E2E delegation + Ui composition)
│   │   ├── GitHubParamHelper.cs
│   │   ├── GitHubTestBase.cs          # API/E2E test base — R3 exception: stays in Helpers
│   │   └── UiSandboxCleanupHelper.cs # UI sandbox (gredjapl/ui-sandbox) via GITHUB_UI_PAT
│   ├── Http/
│   │   ├── GitHubRequestHelper.cs
│   │   ├── JsonPlaceholderRequestHelper.cs
│   │   └── RequestHelper.cs
│   ├── Params/
│   │   └── ParamHelper.cs         # shared IdParam + UrlSegment/Query primitives for param helpers
│   └── Waiting/
│       ├── WaitHelper.cs          # generic async WaitUntilAsync polling
│       └── WaitResult.cs          # IsSuccess/Elapsed/Attempts/LastValue
├── Logging/
│   ├── ActionLogHandler.cs        # DelegatingHandler: logs every HTTP request/response
│   ├── ActionLogger.cs            # Serilog bootstrap → %TEMP%\GredjaTestRun\actions-*.log
│   ├── ActionLogParser.cs         # picks failed-test actions for the report
│   ├── SharedFile.cs              # read-with-ShareWrite helpers
│   ├── TestNameEnricher.cs        # adds NUnit test name to every log line
│   ├── TestOutcome.cs             # own test result (name/status/duration/error)
│   ├── TestOutcomeAttribute.cs    # assembly-level ITestAction → test-results-*.log
│   ├── TestOutcomeReader.cs       # reads test-results-*.log of the current run
│   ├── TestOutcomeFields.cs       # shared JSON property names of the outcome log line
│   ├── TestOutcomeWriter.cs       # appends one JSON line per finished test
│   ├── TestStatusNames.cs         # shared wire status strings (passed/skipped/failed)
│   ├── TestRunWorkspace.cs        # temp workspace %TEMP%\GredjaTestRun (raw logs)
│   └── TestSourceCategoryReader.cs # fixture/category from test sources
├── Reporting/
│   ├── TestRunReportGenerator.cs  # writes TestResults/TestRunReport-*.md (teardown)
│   ├── ArtifactRetention.cs       # age-based cleanup of allure-results/TestResults artifacts
│   └── TestRunRow.cs
├── Models/
│   ├── Generic/
│   │   ├── Responses/
│   │   │   └── ErrorMessageModelResponse.cs
│   │   ├── IdModel.cs
│   │   ├── JsonFields.cs
│   │   ├── ParamType.cs
│   │   ├── RequestDictionaryModel.cs
│   │   └── UserOwnedModel.cs
│   ├── JsonPlaceholder/
│   │   ├── Requests/
│   │   │   ├── PostModelRequest.cs
│   │   │   └── TodoModelRequest.cs
│   │   ├── Responses/
│   │   │   ├── AlbumModelResponse.cs
│   │   │   ├── CommentModelResponse.cs
│   │   │   ├── PhotoModelResponse.cs
│   │   │   ├── PostModelResponse.cs
│   │   │   ├── TodoModelResponse.cs
│   │   │   └── UserModelResponse.cs
│   │   ├── Address.cs
│   │   ├── Company.cs
│   │   ├── Geo.cs
│   │   └── JsonFields.cs
│   ├── GitHub/
│   │   ├── Requests/
│   │   │   ├── CreateCollaboratorModelRequest.cs
│   │   │   ├── CreateCommentModelRequest.cs
│   │   │   ├── CreateGitBlobModelRequest.cs
│   │   │   ├── CreateGitCommitModelRequest.cs
│   │   │   ├── CreateGitRefModelRequest.cs
│   │   │   ├── CreateGitTreeModelRequest.cs
│   │   │   ├── CreateIssueModelRequest.cs
│   │   │   ├── CreatePullRequestModelRequest.cs
│   │   │   ├── CreateReleaseModelRequest.cs
│   │   │   ├── UpdateIssueModelRequest.cs
│   │   │   ├── UpdatePullRequestModelRequest.cs
│   │   │   └── UpdateStateTitleModelRequest.cs   # shared base PATCH State+Title
│   │   ├── Responses/
│   │   │   ├── BranchModelResponse.cs
│   │   │   ├── CollaboratorModelResponse.cs
│   │   │   ├── CommentModelResponse.cs
│   │   │   ├── CommitModelResponse.cs
│   │   │   ├── ContributorModelResponse.cs
│   │   │   ├── GitCommitModelResponse.cs
│   │   │   ├── GitRefModelResponse.cs
│   │   │   ├── GitShaModelResponse.cs
│   │   │   ├── InvitationModelResponse.cs
│   │   │   ├── IssueModelResponse.cs
│   │   │   ├── PermissionModelResponse.cs
│   │   │   ├── PullRequestCommitModelResponse.cs
│   │   │   ├── PullRequestFileModelResponse.cs
│   │   │   ├── PullRequestMergeModelResponse.cs
│   │   │   ├── PullRequestModelResponse.cs
│   │   │   ├── RateLimitModelResponse.cs
│   │   │   ├── ReleaseModelResponse.cs
│   │   │   ├── RepositoryModelResponse.cs
│   │   │   ├── SearchModelResponse.cs
│   │   │   ├── TagModelResponse.cs
│   │   │   ├── TopicsModelResponse.cs
│   │   │   └── UserModelResponse.cs
│   │   ├── BranchCommit.cs
│   │   ├── CollaboratorPermissions.cs
│   │   ├── CommitAuthor.cs
│   │   ├── CommitInfo.cs
│   │   ├── GitCommitTree.cs
│   │   ├── GitRefObject.cs
│   │   ├── GitTreeEntry.cs
│   │   ├── IssuePullRequest.cs
│   │   ├── JsonFields.cs
│   │   ├── Label.cs
│   │   ├── PullRequestBranch.cs
│   │   ├── RateLimitResources.cs
│   │   └── RateLimitSection.cs
```

## Api/

```
Api/
├── Api.csproj
├── AllureGlobalSetup.cs
├── AssemblyInfo.cs               # [assembly: Parallelizable(Fixtures)] + LevelOfParallelism(1)
├── JsonPlaceholder/
│   ├── Helpers/
│   │   ├── JsonPlaceholderParamHelper.cs
│   │   └── JsonPlaceholderTestData.cs
│   └── Tests/
│       ├── Albums/
│       │   ├── GetAllAlbumsTests.cs
│       │   ├── GetAlbumByIdTests.cs
│       │   └── GetAlbumsByUserTests.cs
│       ├── Comments/
│       │   ├── GetAllCommentsTests.cs
│       │   ├── GetCommentByIdTests.cs
│       │   └── GetCommentsByPostTests.cs
│       ├── Photos/
│       │   ├── GetAllPhotosTests.cs
│       │   ├── GetPhotoByIdTests.cs
│       │   └── GetPhotosByAlbumTests.cs
│       ├── Posts/
│       │   ├── CreatePostTests.cs
│       │   ├── DeletePostTests.cs
│       │   ├── GetAllPostsTests.cs
│       │   ├── GetPostByIdTests.cs
│       │   └── UpdatePostTests.cs
│       ├── Todos/
│       │   ├── GetAllTodosTests.cs
│       │   └── GetTodosByUserIdTests.cs
│       └── Users/
│           ├── GetAllUsersTests.cs
│           ├── GetUserByIdTests.cs
│           ├── GetUserPostsTests.cs
│           ├── GetUserTodosTests.cs
│           └── GetUserAlbumsTests.cs
├── GitHub/
│   └── Tests/
│       ├── Auth/
│       │   ├── AuthNegativeTests.cs
│       │   └── GetAuthenticatedUserTests.cs
│       ├── AuthRepos/
│       │   └── GetAuthenticatedUserReposTests.cs
│       ├── Branches/
│       │   ├── GetBranchByNameTests.cs
│       │   └── GetBranchesTests.cs
│       ├── Collaborators/
│       │   ├── CreateInvitationTests.cs
│       │   ├── DeleteInvitationTests.cs
│       │   ├── GetCollaboratorPermissionTests.cs
│       │   ├── GetCollaboratorsTests.cs
│       │   └── GetInvitationsTests.cs
│       ├── Commits/
│       │   └── GetCommitsTests.cs
│       ├── Contributors/
│       │   └── GetContributorsTests.cs
│       ├── GitCommits/
│       │   ├── CreateGitCommitTests.cs
│       │   ├── CreateGitTreeTests.cs
│       │   └── GetGitCommitTests.cs
│       ├── GitRefs/
│       │   ├── CreateRefTests.cs
│       │   ├── DeleteRefTests.cs
│       │   └── GetGitRefByNameTests.cs
│       ├── IssueComments/
│       │   ├── CreateCommentTests.cs
│       │   ├── DeleteCommentTests.cs
│       │   └── GetIssueCommentsTests.cs
│       ├── Issues/
│       │   ├── CreateIssueNegativeTests.cs
│       │   ├── CreateIssueVisibilityTests.cs
│       │   ├── GetIssueByIdTests.cs
│       │   ├── GetIssuesTests.cs
│       │   └── UpdateIssueTests.cs
│       ├── Languages/
│       │   └── GetLanguagesTests.cs
│       ├── PublicRepos/
│       │   └── GetPublicReposTests.cs
│       ├── PullRequests/
│       │   ├── CreatePullRequestTests.cs
│       │   ├── GetPullRequestByIdTests.cs
│       │   ├── GetPullRequestCommitsTests.cs
│       │   ├── GetPullRequestFilesTests.cs
│       │   ├── GetPullRequestsTests.cs
│       │   ├── MergePullRequestTests.cs
│       │   └── UpdatePullRequestTests.cs
│       ├── RateLimit/
│       │   └── GetRateLimitTests.cs
│       ├── Releases/
│       │   ├── GetReleaseDetailTests.cs
│       │   └── GetReleasesTests.cs
│       ├── Repos/
│       │   ├── GetRepositoryTests.cs
│       │   └── GetReposTests.cs
│       ├── Search/
│       │   ├── SearchIssuesTests.cs
│       │   ├── SearchRepositoriesTests.cs
│       │   └── SearchUsersTests.cs
│       ├── Tags/
│       │   └── GetTagsTests.cs
│       ├── Topics/
│       │   └── GetTopicsTests.cs
│       ├── UserRepos/
│       │   └── GetUserReposTests.cs
│       └── Users/
│           ├── GetUserTests.cs
│           └── GetUsersTests.cs
└── TestReportSetup.cs            # [assembly: TestOutcome] + NUnit teardown → TestRunReport-*.md; duplicated per test assembly (SetUpFixture)
```

## AllureAdapter/

```
AllureAdapter/
├── AllureAdapter.csproj
└── Helpers/                      # flat infra layout by design (constants/attribute/models in one folder)
    ├── AllureConstants.cs
    ├── AllureHelper.cs
    ├── AllureJsonWriter.cs
    ├── AllureNUnitAttribute.cs
    ├── AllureSkippedTestWriter.cs
    ├── AllureTestResultBuilder.cs
    ├── ContainerInfo.cs
    └── TestResultParams.cs
```

## E2E/

```
E2E/
├── E2E.csproj
├── AllureGlobalSetup.cs
├── AssemblyInfo.cs               # [assembly: Parallelizable(Fixtures)] + LevelOfParallelism(1)
├── GitHub/
│   ├── GitHubE2ETestBase.cs
│   └── Tests/
│       ├── CollaboratorPermissionTests.cs
│       ├── CommentChainTests.cs
│       ├── FailedWriteTests.cs
│       ├── GitRefLifecycleTests.cs
│       ├── IssueLifecycleTests.cs
│       ├── IssueStateMachineTests.cs
│       ├── PullRequestFlowTests.cs
│       ├── RateLimitDrainTests.cs
│       └── RepositoryHealthTests.cs
└── TestReportSetup.cs            # [assembly: TestOutcome] + NUnit teardown → TestRunReport-*.md; duplicated per test assembly (SetUpFixture)
```

## Ui/

```
Ui/
├── Ui.csproj                     # Microsoft.Playwright + NUnit, refs Core + AllureAdapter
├── AssemblyInfo.cs               # [assembly: Parallelizable(None)] — sequential (shared storageState)
├── TestReportSetup.cs            # [assembly: TestOutcome] + NUnit teardown → TestRunReport-*.md
├── Data/                         # static test files (upload payloads) — reserved, currently empty
├── github/                       # all GitHub UI tests (site-scoped; future sites — sibling folders)
│   ├── Pages/                      # pages + forms — single home of locators
│   │   ├── Common/                   # cross-area pages (header, nav): LoginPage, HeaderPage, ProfilePage, AccountSettingsPage
│   │   └── GitHubStream/
│   │       ├── Issues/               # IssueCreatePage, IssueDetailPage, CommentForm
│   │       ├── Pulls/                # PullDetailPage, PullCreatePage
│   │       └── Repository/           # RepositoryPageBase (shared readme element), RepositoryPage, IssuesPage, PullsPage, BranchesPage, CodePage, NotFoundPage
│   └── Tests/
│       ├── Support/                  # test infrastructure: GitHubUiTestBase, BrowserPlaywright, TabCountParser, RetryHelper
│       └── GitHubStream/
│           ├── Issues/               # IssuesUiTests, IssueUiTests
│           ├── Login/                # LoginSignInTests, AuthStateUiTests, LogoutSignOutTests, AnonAccessUiTests
│           ├── Pulls/                # PullsUiTests, PullFlowUiTests
│           └── Repository/           # RepositoryUiTests, BranchesUiTests
└── moodledemo/                   # Moodle demo multilingual site (site-scoped sibling of github/)
    ├── Pages/                      # HomePage, LoginPage (language-agnostic locators, L-LOC-1)
    ├── Fixtures/
    │   └── expected-langs.json      # code → { lang, dir, title } + loginLabels + navLabels + calendarPageCodes + titleScripts — the only source of localized strings
    └── Tests/
        ├── Support/                # MoodledemoTestBase, MoodledemoBrowser, MoodledemoHttp, HtmlLocaleReader, PageLocaleReader, SiteNavigation, TitleScriptAssert, MoodledemoFixtures, LocaleExpectation, LoginLabelExpectation, NavLabelExpectation
        ├── L1SmokeTests.cs          # HTTP-first locale contract + title-script check (data-driven, no browser)
        ├── LanguageSwitcherUiTests.cs   # language menu: list, switch, en vs en_us
        ├── LanguagePersistenceUiTests.cs # session language survives navigation / resets after login
        ├── LoginLocalizationUiTests.cs  # localized login labels from fixture
        ├── RtlLayoutUiTests.cs      # RTL locales: html dir + computed body direction + title
        ├── RtlStyleUiTests.cs       # RTL computed styles: body + nav direction mirror html dir
        ├── NavFooterLocalizationUiTests.cs # top-nav labels from fixture + guest footer login link
        ├── CalendarRegionUiTests.cs # calendar month page locale + no horizontal overflow
        ├── FormRetentionUiTests.cs  # ?lang= reload resets form (expected) + form stays usable
        ├── AccountUiTests.cs        # L3: 5 demo accounts login → dashboard → logout → guest
        └── LoginFormUiTests.cs      # Playwright: login form via stable selectors
```

## Rules/

```
Rules/
├── assertions.md
├── categories.md
├── code.md
├── code-principles.md
├── code-style.md
├── comments.md
├── config.md
├── git.md
├── localization-testing.md
├── models.md
├── test-practices.md
├── ui-testing.md
└── workflow.md
```

## Documentation/

```
documentation/
├── AuditReport.md                # Full audit report 2026-10-09 (findings, fixes not applied)
├── BugReportTemplate.md
├── Bugs/
│   └── JsonPlaceholder/
│       ├── JP-001-create-post-accepts-empty-body.md
│       ├── JP-002-delete-post-does-not-actually-delete.md
│       ├── JP-003-delete-post-returns-200-for-non-existent-id.md
│       ├── JP-004-patch-post-does-not-merge-with-existing-data.md
│       ├── JP-005-put-post-accepts-missing-required-fields.md
│       └── JP-006-patch-post-returns-200-for-any-id.md
├── FILE_STRUCTURE.md
├── GeneralPlan.md                # Permanent user backlog (never deleted)
├── GitHubObservableBehaviour.md
├── GitHubTestingStructure.md
├── GitHubUiObservableBehaviour.md
├── JsonPlaceholderObservableBehaviour.md
├── JsonPlaceholderTestPlan.md
├── MoodleObservableBehaviour.md
├── ObservableBehaviourTemplate.md
├── Katas/
│   ├── 00-test-plan.md
│   ├── 01-test-cases.md
│   ├── 02-test-data.json
│   ├── 02-data-method.md
│   ├── model-selection-note.md
│   ├── prompt-or-skill-template-api-test-gen.md
│   ├── metrics.md                    # AI usage tracking (Gap 1)
│   └── maturity-gap-analysis.md
├── README.md
└── token-budget.md
```
