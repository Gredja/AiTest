# File Structure — Gredja

> Auto-maintained. Update after any structural change.

## Root

```
Gredja/
├── Gredja.slnx
├── Directory.Build.props
├── AGENTS.md
├── allureConfig.json
├── testsettings.json
├── .env                          # secrets (not tracked)
├── .gitignore
├── .graphifyignore
├── .mimocode/
│   ├── mimocode.jsonc
│   ├── hooks/
│   │   └── safety-commit.ts
│   ├── scripts/
│   │   └── gh-pr-create.ps1
│   └── skills/
│       ├── api-test-gen/SKILL.md
│       ├── commit/SKILL.md
│       ├── gredja-rules/SKILL.md
│       ├── pr/SKILL.md
│       ├── review/SKILL.md
│       ├── review-pr/SKILL.md
│       └── test/SKILL.md
└── Scripts/
    ├── allure-categories.json
    ├── allure-report.ps1
    └── generate-behaviors.ps1
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
│   ├── Endpoints.cs              # FakeStoreAPI endpoints
│   └── TestConfig.cs             # reads testsettings.json
├── Helpers/
│   ├── AssertHelper.cs           # Generic assertions
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
│   └── RequestDictionaryModel.cs # shared — dynamic request params
```

## Api/

```
Api/
├── Api.csproj
├── .runsettings
├── AllureGlobalSetup.cs
├── Helpers/
│   ├── ProductAssertHelper.cs
│   └── UserAssertHelper.cs
└── Tests/
    ├── GetAllProductsTests.cs
    ├── GetAllUsersTests.cs
    ├── GetProductByIdTests.cs
    └── GetUserByIdTests.cs
```

## TestAdapter/

```
TestAdapter/
├── TestAdapter.csproj
├── AllureBddAttributes.cs
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
├── .runsettings
├── AllureGlobalSetup.cs
└── Tests/
    └── DummyTests.cs
```

## Rules/

```
Rules/
├── assertions.md
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
├── AGENTS.md
├── FILE_STRUCTURE.md
├── JSONPlaceholder-PLAN.md       # plan for adding JSONPlaceholder API
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
