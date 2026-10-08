# File Structure — Gredja

> Auto-maintained. Update after any structural change.

## Root

```
Gredja/
├── README.md                     # project overview (rendered by GitHub as the homepage)
├── AGENTS.md
├── metrics.md                      # AI usage tracking (Gap 1)
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
├── allure-results/              # not tracked
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
│       └── update-docs/SKILL.md
├── Scripts/
│   ├── allure-report.ps1
│   ├── normalize-eol.ps1         # worktree LF→CRLF per .gitattributes — run before `git add -A`
│   └── test-coverage.ps1
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
│   ├── JsonPlaceholderEndpoints.cs
│   └── TestConfig.cs
├── Helpers/
│   ├── Assertions/
│   │   └── AssertHelper.cs        # ShouldHaveValidContract, ShouldHaveValidFields, etc.
│   ├── Data/
│   │   └── DataGenerator.cs       # RandomString/RandomInt for varying write payloads
│   ├── GitHub/
│   │   ├── GitHubParamHelper.cs
│   │   └── GitHubTestBase.cs
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
│   └── TestRunRow.cs
├── Models/
│   ├── Generic/
│   │   ├── ErrorMessageModelResponse.cs
│   │   ├── IdModel.cs
│   │   ├── JsonFields.cs
│   │   ├── ParamType.cs
│   │   ├── RequestDictionaryModel.cs
│   │   └── UserOwnedModel.cs
│   ├── JsonPlaceholder/
│   │   ├── Address.cs
│   │   ├── AlbumModelResponse.cs
│   │   ├── CommentModelResponse.cs
│   │   ├── Company.cs
│   │   ├── Geo.cs
│   │   ├── JsonFields.cs
│   │   ├── PhotoModelResponse.cs
│   │   ├── PostModelRequest.cs
│   │   ├── PostModelResponse.cs
│   │   ├── TodoModelRequest.cs
│   │   ├── TodoModelResponse.cs
│   │   └── UserModelResponse.cs
│   ├── GitHub/
│   │   ├── BranchCommit.cs
│   │   ├── BranchModelResponse.cs
│   │   ├── CommentModelResponse.cs
│   │   ├── CommitAuthor.cs
│   │   ├── CommitInfo.cs
│   │   ├── CommitModelResponse.cs
│   │   ├── ContributorModelResponse.cs
│   │   ├── CreateCommentModelRequest.cs
│   │   ├── CreateIssueModelRequest.cs
│   │   ├── IssueModelResponse.cs
│   │   ├── JsonFields.cs
│   │   ├── Label.cs
│   │   ├── PullRequestBranch.cs
│   │   ├── PullRequestCommitModelResponse.cs
│   │   ├── PullRequestFileModelResponse.cs
│   │   ├── PullRequestModelResponse.cs
│   │   ├── RateLimitModelResponse.cs
│   │   ├── RateLimitResources.cs
│   │   ├── RateLimitSection.cs
│   │   ├── ReleaseModelResponse.cs
│   │   ├── RepositoryModelResponse.cs
│   │   ├── TagModelResponse.cs
│   │   ├── TopicsModelResponse.cs
│   │   ├── UpdateIssueModelRequest.cs
│   │   └── UserModelResponse.cs
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
│       │   └── AuthNegativeTests.cs
│       ├── AuthRepos/
│       │   └── GetAuthenticatedUserReposTests.cs
│       ├── Branches/
│       │   ├── GetBranchByNameTests.cs
│       │   └── GetBranchesTests.cs
│       ├── Contributors/
│       │   └── GetContributorsTests.cs
│       ├── IssueComments/
│       │   └── GetIssueCommentsTests.cs
│       ├── Issues/
│       │   ├── CreateIssueNegativeTests.cs
│       │   ├── CreateIssueVisibilityTests.cs
│       │   ├── GetIssueByIdTests.cs
│       │   └── GetIssuesTests.cs
│       ├── Languages/
│       │   └── GetLanguagesTests.cs
│       ├── PublicRepos/
│       │   └── GetPublicReposTests.cs
│       ├── PullRequests/
│       │   ├── GetPullRequestCommitsTests.cs
│       │   ├── GetPullRequestFilesTests.cs
│       │   └── GetPullRequestsTests.cs
│       ├── RateLimit/
│       │   └── GetRateLimitTests.cs
│       ├── Repos/
│       │   ├── GetRepositoryTests.cs
│       │   └── SampleTests.cs
│       ├── Tags/
│       │   └── GetTagsTests.cs
│       ├── Topics/
│       │   └── GetTopicsTests.cs
│       ├── UserRepos/
│       │   └── GetUserReposTests.cs
│       └── Users/
│           └── GetUserTests.cs
└── TestReportSetup.cs            # [assembly: TestOutcome] + NUnit teardown → TestRunReport-*.md
```

## AllureAdapter/

```
AllureAdapter/
├── AllureAdapter.csproj
└── Helpers/
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
│       └── IssueLifecycleTests.cs
└── TestReportSetup.cs            # [assembly: TestOutcome] + NUnit teardown → TestRunReport-*.md
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
├── models.md
├── test-practices.md
└── workflow.md
```

## Documentation/

```
documentation/
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
├── JsonPlaceholderObservableBehaviour.md
├── JsonPlaceholderTestPlan.md
├── ObservableBehaviourTemplate.md
├── Katas/
│   ├── 00-test-plan.md
│   ├── 01-test-cases.md
│   ├── 02-test-data.json
│   ├── 02-data-method.md
│   ├── model-selection-note.md
│   ├── prompt-or-skill-template-api-test-gen.md
│   └── maturity-gap-analysis.md
├── README.md
└── token-budget.md
```
