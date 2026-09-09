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
├── .gitignore
├── .graphifyignore
├── .mimocode/
│   ├── mimocode.jsonc
│   ├── commands/
│   │   ├── api-test-gen.md
│   │   ├── commit.md
│   │   ├── review.md
│   │   ├── review-commit.md
│   │   ├── review-pr.md
│   │   └── test.md
│   ├── hooks/
│   │   └── safety-commit.ts
│   ├── reviews/                  # not tracked
│   ├── plans/                    # not tracked
│   ├── scripts/
│   │   └── gh-pr-create.ps1
│   └── skills/
│       ├── api-test-gen/SKILL.md
│       ├── commit/SKILL.md
│       ├── gredja-rules/SKILL.md
│       ├── review/SKILL.md
│       ├── review-commit/SKILL.md
│       ├── review-pr/SKILL.md
│       ├── test/SKILL.md
│       └── test-report/SKILL.md
└── Scripts/
    └── allure-report.ps1
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
│   ├── FakeStoreEndpoints.cs     # FakeStoreAPI endpoints
│   ├── JsonPlaceholderEndpoints.cs  # JSONPlaceholder endpoints
│   └── TestConfig.cs             # reads common settings from testsettings.json
├── Helpers/
│   ├── AssertHelper.cs           # Generic assertions
│   ├── JsonPlaceholderRequestHelper.cs  # RequestHelper for JSONPlaceholder
│   └── RequestHelper.cs          # HTTP request wrapper (Get/Post/Put/Patch/Delete)
├── Models/
│   ├── Generic/
│   │   ├── IdNameModel.cs
│   │   └── UserOwnedModel.cs
│   ├── FakeStore/
│   │   ├── AddressModel.cs
│   │   ├── AuthRequest.cs
│   │   ├── CartModel.cs
│   │   ├── CartProductModel.cs
│   │   ├── CartRequest.cs
│   │   ├── GeolocationModel.cs
│   │   ├── ProductModel.cs
│   │   ├── ProductRequest.cs
│   │   ├── RatingModel.cs
│   │   ├── UserModel.cs
│   │   ├── UserNameModel.cs
│   │   └── UserRequest.cs
│   ├── JsonPlaceholder/
│   │   ├── AlbumModel.cs
│   │   ├── CommentModel.cs
│   │   ├── CompanyModel.cs
│   │   ├── GeoModel.cs
│   │   ├── JsonPlaceholderAddressModel.cs
│   │   ├── JsonPlaceholderUserModel.cs
│   │   ├── PhotoModel.cs
│   │   ├── PostModel.cs
│   │   └── TodoModel.cs
│   └── RequestDictionaryModel.cs # shared — dynamic request params + ParamType enum
```

## Api/

```
Api/
├── Api.csproj
├── AllureGlobalSetup.cs
├── FakeStore/
│   ├── Helpers/
│   │   └── FakeStoreParamHelper.cs  # IdParam()
│   └── Tests/
│       ├── GetAllProductsTests.cs    # 6 tests
│       ├── GetAllUsersTests.cs       # 6 tests
│       ├── GetProductByIdTests.cs    # 7 tests (4 active + 3 Ignore)
│       └── GetUserByIdTests.cs       # 6 tests
└── JsonPlaceholder/
    ├── Helpers/
    │   └── JsonPlaceholderParamHelper.cs  # PostIdParam(), UserIdParam()
    └── Tests/
        ├── CreatePostTests.cs        # 4 tests
        ├── DeletePostTests.cs        # 2 tests
        ├── GetAllPostsTests.cs       # 6 tests
        ├── GetAllTodosTests.cs       # 7 tests
        ├── GetPostByIdTests.cs       # 7 tests
        ├── GetTodosByUserIdTests.cs  # 5 tests
        └── UpdatePostTests.cs        # 4 tests
```

## TestAdapter/

```
TestAdapter/
├── TestAdapter.csproj
└── Helpers/
    ├── AllureGlobalSetup.cs
    ├── AllureHelper.cs
    ├── AllureJsonWriter.cs
    ├── AllureNUnitAttribute.cs
    ├── AllureSkippedTestWriter.cs
    └── AllureTestResultBuilder.cs
```

## Ui/

```
Ui/
├── Ui.csproj
├── AllureGlobalSetup.cs
└── Tests/
    └── DummyTests.cs
```

## Rules/

```
Rules/
├── assertions.md
├── categories.md
├── code.md
├── comments.md
├── config.md
├── git.md
├── models.md
└── workflow.md
```

## Documentation/

```
documentation/
├── FILE_STRUCTURE.md
├── JSONPlaceholder-PLAN.md       # plan for adding JSONPlaceholder API
├── Katas/
│   ├── model-selection-note.md
│   ├── prompt-or-skill-template-api-test-gen.md
│   └── maturity-gap-analysis.md
├── PLAN.md
├── README.md
├── TestPlan.md
├── TODO.md
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
