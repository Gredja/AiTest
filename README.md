# Gredja

Test application for learning AI-Native SDLC. API, E2E and UI test automation project.

## Technologies

- .NET 10.0
- NUnit 4.6.1
- RestSharp 114.0.0
- FluentAssertions 8.11.0
- Microsoft.Playwright 1.63.0 (UI tests, Chromium)
- Serilog 4.4.0 (action logging → `%TEMP%\GredjaTestRun`)
- Allure (via custom AllureAdapter)
- Coverlet (code coverage)

## APIs Under Test

### JSONPlaceholder — https://jsonplaceholder.typicode.com
- Posts — GET, POST, PUT, PATCH, DELETE
- Comments, Albums, Photos, Todos, Users — GET (all, by parent/ID)

### GitHub REST API v3 — https://api.github.com
- Repositories, Issues, PRs, Branches, Users, Commits, Releases, Contributors, Languages, Topics, Tags, Rate Limit — GET (full Phase 1 coverage)
- Issues, Comments, PRs, Branches, Git refs — POST, PATCH, DELETE (single-endpoint in `Api/`, scenario chains in `E2E/`)

## Test Structure

- `Api/` — read-only API tests + single-endpoint write tests (NUnit + RestSharp)
- `E2E/` — scenario chain tests: multi-step write flows only (NUnit + RestSharp)
- `Ui/` — UI tests (Playwright .NET + Chromium): page-objects, storageState auth, write via UI → cleanup via API
- `Core/Models/` — response/request models
- `Core/Config/` — endpoint constants
- `Core/Helpers/` — Http/, Assertions/, Params/, Data/, GitHub/ (request infra, asserts, params, data generators)
- `Core/Logging/` — action logging + raw test results; `Core/Reporting/` — TestRunReport generation
- `AllureAdapter/` — custom Allure adapter

## Running Tests

```bash
dotnet test Api/Api.csproj
dotnet test E2E/E2E.csproj
dotnet test --filter Category=GitHubUi   # UI suite (Playwright)
dotnet test --filter LoginSignInTests    # refresh UI storageState after it expires
dotnet test --filter Category=HealthCheck
./Scripts/install-playwright.ps1  # one-time: install Chromium for UI tests
./Scripts/test-coverage.ps1  # run tests + coverage report
```

## Documentation

- `documentation/GeneralPlan.md` — permanent user backlog (statuses TODO / IN PROGRESS / DONE, never deleted)
- `documentation/JsonPlaceholderTestPlan.md` — JsonPlaceholder API test plan (coverage + negative floor status)
- `documentation/JsonPlaceholderObservableBehaviour.md` — observable behaviour for JsonPlaceholder API
- `documentation/GitHubTestingStructure.md` — scope, risks, entry/exit criteria
- `documentation/GitHubObservableBehaviour.md` — observable behaviour for GitHub API
- `documentation/GitHubUiObservableBehaviour.md` — observable behaviour for GitHub UI (verified locators, gotchas)
- `documentation/FILE_STRUCTURE.md` — project layout
- `documentation/ObservableBehaviourTemplate.md` — template for new services
- `Rules/` — project rules (code, assertions, test practices, UI testing, etc.)

## Coverage

```bash
./Scripts/test-coverage.ps1  # code coverage + file coverage
```

## Deadline

2026-12-31
