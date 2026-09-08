# File Structure — Gredja

> Auto-maintained. Update after any structural change.

## Root

```
Gredja/
├── Gredja.slnx
├── Directory.Build.props
├── AGENTS.md
├── allureConfig.json             # Allure Report configuration
├── testsettings.json             # .NET test settings
├── .env                          # secrets (not tracked)
├── .gitignore
├── .graphifyignore
├── .mimocode/                    # MiMoCode skills, hooks, scripts
│   ├── mimocode.jsonc
│   ├── hooks/
│   │   └── safety-commit.ts      # pre-commit safety hook
│   ├── scripts/
│   │   └── gh-pr-create.ps1      # wrapper for gh pr create
│   └── skills/
│       ├── api-test-gen/
│       │   └── SKILL.md          # API test generation for FakeStoreAPI
│       ├── commit/
│       │   └── SKILL.md          # commit with safety checks + Allure
│       ├── gredja-rules/
│       │   └── SKILL.md          # entry point for project rules
│       ├── pr/
│       │   └── SKILL.md          # create pull request
│       ├── review/
│       │   └── SKILL.md          # review local changes
│       ├── review-pr/
│       │   └── SKILL.md          # PR review (GitHub API based)
│       └── test/
│           └── SKILL.md          # run tests + Allure report
└── Scripts/
    ├── allure-categories.json    # Allure severity categories
    ├── allure-report.ps1         # run tests + generate Allure report
    └── generate-behaviors.ps1    # generate BDD behaviors for Allure
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
│   ├── Endpoints.cs              # BaseUrl + all endpoint constants
│   └── TestConfig.cs             # test configuration
├── Helpers/
│   ├── AssertHelper.cs           # Generic assertions (ShouldBeOk, ShouldHaveValidFields<T>)
│   └── RequestHelper.cs          # HTTP request wrapper (Get, RestClient init)
├── Models/
│   ├── Generic/
│   │   └── IdNameModel.cs
│   ├── AddressModel.cs
│   ├── AuthRequest.cs
│   ├── CartModel.cs
│   ├── CartProductModel.cs
│   ├── CartRequest.cs
│   ├── GeolocationModel.cs
│   ├── LoginErrorResponse.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
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
├── .runsettings                  # NUnit run settings
├── AllureGlobalSetup.cs          # [SetUpFixture] — Allure setup
├── Helpers/
│   ├── CartAssertHelper.cs       # Cart-specific assertions
│   ├── ProductAssertHelper.cs    # Product-specific assertions
│   └── UserAssertHelper.cs       # User-specific assertions
└── Tests/
    ├── GetAllCartsTests.cs       # GET /carts — all carts
    ├── GetAllProductsTests.cs    # GET /products — all products
    ├── GetAllUsersTests.cs       # GET /users — all users
    ├── GetCartByIdTests.cs       # GET /carts/{id} — cart by ID
    ├── GetProductByIdTests.cs    # GET /products/{id} — product by ID
    ├── GetProductCategoriesTests.cs    # GET /products/categories
    ├── GetProductsByCategoryTests.cs   # GET /products/category/{category}
    ├── GetUserByIdTests.cs       # GET /users/{id} — user by ID
    └── LoginTests.cs             # POST /auth/login — login
```

## TestAdapter/

```
TestAdapter/
├── TestAdapter.csproj
├── AllureBddAttributes.cs        # BDD step attributes for Allure
└── Helpers/
    ├── AllureGlobalSetup.cs      # [SetUpFixture] — captures [Ignore] tests as skipped
    ├── AllureHelper.cs           # Shared utilities (FindProjectRoot, GetResultsDir)
    ├── AllureJsonWriter.cs       # JSON file writer for Allure results
    ├── AllureNUnitAttribute.cs   # Custom Allure adapter for NUnit 4.x (ITestAction)
    ├── AllureSkippedTestWriter.cs # writes skipped test results
    └── AllureTestResultBuilder.cs # Builds Allure JSON dictionaries
```

## Ui/

```
Ui/
├── Ui.csproj
├── .runsettings                  # NUnit run settings
├── AllureGlobalSetup.cs          # [SetUpFixture] — Allure setup
└── Tests/
    └── DummyTests.cs             # placeholder tests
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

## Documentation/

```
documentation/
├── AGENTS.md                     # AI agent instructions
├── FILE_STRUCTURE.md             # this file
├── PLAN.md                       # project plan
├── README.md                     # project readme
├── TestPlan.md                   # test plan
├── TODO.md                       # tasks and priorities
└── token-budget.md               # token budgets per workflow
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
