# File Structure — Gredja

> Auto-maintained. Update after any structural change.

## Root

```
Gredja/
├── Gredja.slnx
├── Directory.Build.props
├── AGENTS.md
├── README.md
├── TestPlan.md
├── FILE_STRUCTURE.md
├── token-budget.md               # token budgets per workflow
├── allureConfig.json             # Allure Report configuration
├── .env                          # secrets (not tracked)
├── .gitignore
├── .graphifyignore
├── .claude/                      # AI agent configuration
│   └── skills/
│       ├── gredja-rules/
│       │   └── SKILL.md          # entry point for project rules (overview + links)
│       └── review-pr/
│           └── SKILL.md          # PR review skill (GitHub API based)
└── .mimocode/                    # MiMoCode hooks and scripts
    ├── hooks/
    │   └── read-memory.ts        # auto-loads project memory into session
    └── scripts/
        └── gh-pr-create.ps1      # wrapper for gh pr create

Scripts/
└── allure-report.ps1             # run tests + generate Allure report
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
│   └── Endpoints.cs              # BaseUrl + all endpoint constants
├── Helpers/
│   ├── AssertHelper.cs           # Generic assertions (ShouldBeOk, ShouldHaveValidFields<T>)
│   └── RequestHelper.cs          # HTTP request wrapper (Get, RestClient init) — WIP
├── Models/
│   ├── Generic/
│   │   └── IdNameModel.cs
│   ├── AddressModel.cs
│   ├── AuthRequest.cs
│   ├── CartModel.cs
│   ├── CartProductModel.cs
│   ├── CartRequest.cs
│   ├── GeolocationModel.cs
│   ├── ProductModel.cs
│   ├── ProductRequest.cs
│   ├── RatingModel.cs
│   ├── RequestDictionaryModel.cs
│   ├── UserModel.cs
│   ├── UserNameModel.cs
│   └── UserRequest.cs
```

## Api/

```
Api/
├── Api.csproj
├── Helpers/
│   └── ProductAssertHelper.cs    # Product-specific assertions
└── Tests/
    ├── GetAllProductsTests.cs       # GET /products — 11 tests
    └── GetProductByIdTests.cs       # GET /products/{id} — 11 tests (8 active + 3 Ignore)
```

## TestAdapter/

```
TestAdapter/
├── TestAdapter.csproj
└── Helpers/
    ├── AllureHelper.cs             # Shared utilities (FindProjectRoot, GetResultsDir)
    ├── AllureGlobalSetup.cs        # [SetUpFixture] — captures [Ignore] tests as skipped
    ├── AllureJsonWriter.cs         # JSON file writer for Allure results
    ├── AllureNUnitAttribute.cs     # Custom Allure adapter for NUnit 4.x (ITestAction)
    └── AllureTestResultBuilder.cs  # Builds Allure JSON dictionaries
```

## Ui/

```
Ui/
└── Ui.csproj
```

## Rules/

```
Rules/
├── assertions.md                 # FluentAssertions patterns
├── code.md                       # general code writing rules
├── comments.md                   # when to comment code
├── config.md                     # endpoints, configuration
├── git.md                        # remote, commits, secrets
├── models.md                     # Model/Request building rules
└── workflow.md                   # plan → approval → execute → report
```

## Other

```
Prompts/
├── prompts.md
└── templates/
    └── api-test-generation.md    # Reusable prompt template for endpoint test generation

graphify-out/
├── graph.json
├── model-dependencies.html
└── manifest.json
```
