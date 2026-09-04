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
│   └── AssertHelper.cs           # Generic assertions (ShouldBeOk, ShouldHaveValidFields<T>)
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
└── prompts.md

graphify-out/
├── graph.json
├── model-dependencies.html
└── manifest.json
```
