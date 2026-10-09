# Categories

Every test MUST have `[Category]` attributes — one for the service (on class) and one for the check type (on method).

## Service category (on class)

Add exactly one service category to the `[TestFixture]` class:

| Service | Category |
|---------|----------|
| JSONPlaceholder API | `[Category("JsonPlaceholder")]` |
| GitHub API | `[Category("GitHub")]` |
| GitHub E2E | `[Category("GitHubE2E")]` |

```csharp
[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetAllPostsTests : RequestHelper
```

## Check-type category (on method)

Each test method gets ONE primary category based on what it verifies:

| Category | Applies to | Description |
|----------|-----------|-------------|
| `HealthCheck` | Status code checks only | Endpoint is alive, returns expected HTTP status (200, 201, etc.) |
| `ContractCheck` | Response/request schema matches expected models | Use `ShouldHaveValidContract()` (JSON round-trip + attribute validation) |
| `Smoke` | Basic functionality — checks of a single response | Response not empty, correct count, content-type is JSON, echo/returned data values |
| `Regression` | Validation against a rule or invariant | Field patterns/formats, `id` matches requested, `ShouldHaveValidFields()`, state transitions and cross-step data integrity (E2E chains, quota decrements) |
| `Negative` | Error handling | Non-existent ID, ID=0, negative ID, wrong data — all edge cases that should fail |
| `Performance` | Response time | `ResponseTimeIsAcceptable` tests (GET endpoints) |

**Boundary:** Smoke answers "is the response sane?" (one response, one look); Regression answers "does the data obey a rule?" (pattern, format, state machine, invariant across steps). When a test mixes both, the rule/invariant wins.

```csharp
[Test]
[Category("HealthCheck")]
[Description("1.1 Status code is 200")]
public async Task GetAllPosts_ReturnsOk()

[Test]
[Category("ContractCheck")]
[Description("1.2 Response matches expected model schema")]
public async Task GetAllPosts_ReturnsExpectedFields()

[Test]
[Category("Regression")]
[Description("1.4 Each item has valid required fields")]
public async Task GetAllPosts_EachItemHasValidFields()
```

## Category-to-test mapping

| Test name pattern | Category |
|-------------------|----------|
| `*_ReturnsOk`, `*_ReturnsCreated`, `*_ReturnsDeleted` | `HealthCheck` |
| `*_ReturnsExpectedFields`, `*_ResponseMatchesContract`, `*_ModelMatchesResponse`, `*_HasExpectedSchema` | `ContractCheck` |
| `*_ReturnsNonEmptyList`, `*_ReturnsExpectedCount`, `*_ContentTypeIsJson` | `Smoke` |
| `*_ReturnsNonEmptyBody`, `*_ReturnsNonNull` | `Smoke` |
| `*_ReturnsCorrectData`, `*_HasGeneratedId` | `Smoke` |
| `*_AllBelongToSameUser`, `*_ReturnsUpdatedTitle`, `*_ReturnsEmptyObject` | `Smoke` |
| `*_EachItemHasValidFields`, `*_HasAllExpectedFields`, `*_HasValidFields` | `Regression` |
| `*_IdMatchesRequested`, `*_ReturnsCorrectId` | `Regression` |
| E2E scenario chains (create → verify → mutate → cleanup) | `Regression` |
| `*_NonExistentId_*`, `*_ZeroId_*`, `*_NegativeId_*` | `Negative` |
| `*_NonExistentUser_*` | `Negative` |
| `*_ResponseTimeIsAcceptable` | `Performance` |

## Rules

- NEVER put check-type categories on the class — they go on methods
- NEVER put the service category on methods — it goes on the class
- A method can have only ONE check-type category (the primary thing it verifies)
- `[Ignore]` tests also get their category (Negative) — ignored tests are still categorized
- When filtering: `dotnet test --filter Category=JsonPlaceholder` runs all JsonPlaceholder tests; `dotnet test --filter Category=Negative` runs all negative tests across all services
