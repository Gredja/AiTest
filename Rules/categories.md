# Categories

Every test MUST have `[Category]` attributes — one for the service (on class) and one for the check type (on method).

## Service category (on class)

Add exactly one service category to the `[TestFixture]` class:

| Service | Category |
|---------|----------|
| FakeStore API | `[Category("FakeStore")]` |
| JSONPlaceholder API | `[Category("JsonPlaceholder")]` |
| UI tests | `[Category("Ui")]` |

```csharp
[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class GetAllProductsTests : RequestHelper
```

## Check-type category (on method)

Each test method gets ONE primary category based on what it verifies:

| Category | Applies to | Description |
|----------|-----------|-------------|
| `HealthCheck` | Status code checks only | Endpoint is alive, returns expected HTTP status (200, 201, etc.) |
| `Smoke` | Basic functionality | Response not empty, correct count, content-type is JSON, correct data values |
| `Regression` | Schema / field validation | Each field present and valid, `id` matches requested, `ShouldHaveValidFields()` |
| `Negative` | Error handling | Non-existent ID, ID=0, negative ID, wrong data — all edge cases that should fail |
| `Performance` | Response time | `ResponseTimeIsAcceptable` tests (GET list endpoints only) |

```csharp
[Test]
[Category("HealthCheck")]
[Description("1.1 Status code is 200")]
public async Task GetAllProducts_ReturnsOk()

[Test]
[Category("Regression")]
[Description("1.4 Each item has valid required fields")]
public async Task GetAllProducts_EachItemHasValidFields()
```

## Category-to-test mapping

| Test name pattern | Category |
|-------------------|----------|
| `*_ReturnsOk`, `*_ReturnsCreated`, `*_ReturnsDeleted` | `HealthCheck` |
| `*_ReturnsNonEmptyList`, `*_ReturnsExpectedCount`, `*_ContentTypeIsJson` | `Smoke` |
| `*_ReturnsNonEmptyBody`, `*_ReturnsNonNull` | `Smoke` |
| `*_ReturnsCorrectData`, `*_ReturnsCorrectId`, `*_HasGeneratedId` | `Smoke` |
| `*_AllBelongToSameUser`, `*_ReturnsUpdatedTitle`, `*_ReturnsEmptyObject` | `Smoke` |
| `*_EachItemHasValidFields`, `*_HasAllExpectedFields`, `*_HasValidFields` | `Regression` |
| `*_IdMatchesRequested`, `*_ReturnsCorrectId` | `Regression` |
| `*_NonExistentId_*`, `*_ZeroId_*`, `*_NegativeId_*` | `Negative` |
| `*_NonExistentUser_*` | `Negative` |
| `*_ResponseTimeIsAcceptable` | `Performance` |

## Rules

- NEVER put check-type categories on the class — they go on methods
- NEVER put the service category on methods — it goes on the class
- A method can have only ONE check-type category (the primary thing it verifies)
- `[Ignore]` tests also get their category (Negative) — ignored tests are still categorized
- When filtering: `dotnet test --filter Category=FakeStore` runs all FakeStore tests; `dotnet test --filter Category=Negative` runs all negative tests across all services
