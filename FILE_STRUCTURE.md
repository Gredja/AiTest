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
├── .env                          # secrets (not tracked)
├── .gitignore
└── .graphifyignore
```

## Core/

```
Core/
├── Core.csproj
├── Config/
│   └── Endpoints.cs              # BaseUrl + all endpoint constants
├── Helpers/
│   └── RequestHelper.cs
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
└── Tests/
    └── ProductApiTests.cs
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
