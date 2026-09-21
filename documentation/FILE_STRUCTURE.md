# File Structure — Gredja

> Auto-maintained. Update after any structural change.

## Root

```
Gredja/
├── AGENTS.md
├── Gredja.slnx
├── Directory.Build.props
├── allureConfig.json
├── testsettings.json
├── .env                          # secrets (not tracked)
├── .gitattributes                # line ending normalization
├── .gitignore
├── .graphifyignore
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
│   └── test-coverage.ps1
├── black-white-cat/
└── backups/
    ├── SETUP.md
    ├── mimocode-project.jsonc
    └── Prompts-templates/
        └── api-test-generation.md
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
│   ├── FakeStoreEndpoints.cs
│   ├── GitHubEndpoints.cs
│   ├── JsonPlaceholderEndpoints.cs
│   └── TestConfig.cs
├── Helpers/
│   ├── AssertHelper.cs           # ShouldHaveValidContract, ShouldHaveValidFields, etc.
│   ├── GitHub/
│   │   ├── GitHubParamHelper.cs
│   │   └── GitHubTestBase.cs
│   ├── GitHubRequestHelper.cs
│   ├── JsonPlaceholderRequestHelper.cs
│   └── RequestHelper.cs
├── Models/
│   ├── Generic/
│   │   ├── IdModel.cs
│   │   └── UserOwnedModel.cs
│   ├── FakeStore/
│   │   ├── Address.cs
│   │   ├── AuthModelRequest.cs
│   │   ├── CartModelResponse.cs
│   │   ├── CartModelRequest.cs
│   │   ├── CartProduct.cs
│   │   ├── Geolocation.cs
│   │   ├── ProductModelResponse.cs
│   │   ├── ProductModelRequest.cs
│   │   ├── Rating.cs
│   │   ├── UserModelResponse.cs
│   │   ├── UserModelRequest.cs
│   │   └── UserName.cs
│   ├── JsonPlaceholder/
│   │   ├── AlbumModelResponse.cs
│   │   ├── Comment.cs
│   │   ├── Company.cs
│   │   ├── Geo.cs
│   │   ├── JsonPlaceholderAddress.cs
│   │   ├── JsonPlaceholderUser.cs
│   │   ├── PhotoModelResponse.cs
│   │   ├── PostModelResponse.cs
│   │   ├── PostModelRequest.cs
│   │   └── TodoModelResponse.cs
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
│   │   ├── Label.cs
│   │   ├── PullRequestBranch.cs
│   │   ├── PullRequestModelResponse.cs
│   │   ├── RateLimitModelResponse.cs
│   │   ├── RateLimitResources.cs
│   │   ├── RateLimitSection.cs
│   │   ├── ReleaseModelResponse.cs
│   │   ├── RepositoryModelResponse.cs
│   │   ├── TagModelResponse.cs
│   │   └── UserModelResponse.cs
│   ├── ParamType.cs
│   └── RequestDictionaryModel.cs
```

## Api/

```
Api/
├── Api.csproj
├── AllureGlobalSetup.cs
├── FakeStore/
│   ├── Helpers/
│   │   └── FakeStoreParamHelper.cs
│   └── Tests/
│       ├── Products/
│       │   ├── GetAllProductsTests.cs
│       │   └── GetProductByIdTests.cs
│       └── Users/
│           ├── GetAllUsersTests.cs
│           └── GetUserByIdTests.cs
├── JsonPlaceholder/
│   ├── Helpers/
│   │   └── JsonPlaceholderParamHelper.cs
│   └── Tests/
│       ├── Posts/
│       │   ├── CreatePostTests.cs
│       │   ├── DeletePostTests.cs
│       │   ├── GetAllPostsTests.cs
│       │   ├── GetPostByIdTests.cs
│       │   └── UpdatePostTests.cs
│       └── Todos/
│           ├── GetAllTodosTests.cs
│           └── GetTodosByUserIdTests.cs
└── GitHub/
    └── Tests/
        ├── AuthRepos/
        │   └── GetAuthenticatedUserReposTests.cs
        ├── Branches/
        │   └── GetBranchesTests.cs
        ├── IssueComments/
        │   └── GetIssueCommentsTests.cs
        ├── Issues/
        │   ├── GetIssueByIdTests.cs
        │   └── GetIssuesTests.cs
        ├── PublicRepos/
        │   └── GetPublicReposTests.cs
        ├── PullRequests/
        │   └── GetPullRequestsTests.cs
        ├── RateLimit/
        │   └── GetRateLimitTests.cs
        ├── Repos/
        │   └── GetRepositoryTests.cs
        ├── UserRepos/
        │   └── GetUserReposTests.cs
        └── Users/
            └── GetUserTests.cs
```

## TestAdapter/

```
TestAdapter/
├── TestAdapter.csproj
└── Helpers/
    ├── AllureConstants.cs
    ├── AllureHelper.cs
    ├── AllureJsonWriter.cs
    ├── AllureNUnitAttribute.cs
    ├── AllureSkippedTestWriter.cs
    ├── AllureTestResultBuilder.cs
    └── TestResultParams.cs
```

## E2E/

```
E2E/
├── E2E.csproj
├── AllureGlobalSetup.cs
└── GitHub/
    ├── GitHubE2ETestBase.cs
    └── Tests/
        └── SampleTests.cs
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
├── FILE_STRUCTURE.md
├── GitHubObservableBehaviour.md
├── GitHubTestPlan.md
├── GitHubTestingStructure.md
├── ObservableBehaviourTemplate.md
├── Katas/
│   ├── model-selection-note.md
│   ├── prompt-or-skill-template-api-test-gen.md
│   └── maturity-gap-analysis.md
├── README.md
└── token-budget.md
```

## Other

```
Prompts/
├── prompts.md
└── templates/
    └── api-test-generation.md

graphify-out/
├── graph.json
├── model-dependencies.html
└── manifest.json
```
