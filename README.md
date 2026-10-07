# Gredja

Test application for learning AI-Native SDLC. Project for API test automation.

## Technologies

- .NET 10.0
- NUnit 4.6.1
- RestSharp 114.0.0
- FluentAssertions 8.11.0
- Allure (via custom AllureAdapter)
- Coverlet (code coverage)

## APIs Under Test

### JSONPlaceholder — https://jsonplaceholder.typicode.com
- Posts — GET, POST, PUT, PATCH, DELETE
- Comments, Albums, Photos, Todos, Users — GET (all, by parent/ID)

### GitHub REST API v3 — https://api.github.com
- Repos, Issues, PRs, Branches, Users, Rate Limit — GET
- Issues, Comments, PRs, Branches — POST, PATCH, DELETE (single-endpoint in `Api/`, scenario chains in `E2E/`)

## Test Structure

- `Api/` — read-only API tests + single-endpoint write tests (NUnit + RestSharp)
- `E2E/` — scenario chain tests: multi-step write flows only (NUnit + RestSharp)
- `Core/Models/` — response/request models
- `Core/Config/` — endpoint constants
- `Core/Helpers/` — Http/, Assertions/, Params/, Data/, GitHub/ (request infra, asserts, params, data generators)
- `Core/Logging/` — action logging + raw test results; `Core/Reporting/` — TestRunReport generation
- `AllureAdapter/` — custom Allure adapter

## Running Tests

```bash
dotnet test Api/Api.csproj
dotnet test E2E/E2E.csproj
dotnet test --filter Category=HealthCheck
./Scripts/test-coverage.ps1  # run tests + coverage report
```

## Documentation

- `documentation/GeneralPlan.md` — permanent user backlog (statuses TODO / IN PROGRESS / DONE, never deleted)
- `documentation/JsonPlaceholderTestPlan.md` — JsonPlaceholder API test plan (coverage + negative floor status)
- `documentation/JsonPlaceholderObservableBehaviour.md` — observable behaviour for JsonPlaceholder API
- `.mimocode/plans/github-full-coverage.md` — GitHub API test plan (Phase 1 + Phase 2)
- `documentation/GitHubTestingStructure.md` — scope, risks, entry/exit criteria
- `documentation/GitHubObservableBehaviour.md` — observable behaviour for GitHub API
- `documentation/ObservableBehaviourTemplate.md` — template for new services
- `Rules/` — project rules (code, assertions, test practices, etc.)

## Coverage

```bash
./Scripts/test-coverage.ps1  # code coverage + file coverage
```

## Deadline

2026-12-31
