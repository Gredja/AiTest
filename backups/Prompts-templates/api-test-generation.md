# Prompt Template: API Endpoint Test Generation

Use this prompt to generate a complete test class for any FakeStoreAPI endpoint. Fill in the placeholders and send to the AI.

---

## Prompt

```
Generate a test class for the endpoint: {METHOD} {ENDPOINT_PATH}

## Endpoint info

- Method: {METHOD}
- Path: {ENDPOINT_PATH}
- Path constant in Endpoints file: {ENDPOINTS_CONSTANT}
- Description: {DESCRIPTION}
- Request model (if POST/PUT): {REQUEST_MODEL}
- Response model: {RESPONSE_MODEL}
- Response is a list: {IS_LIST} (true/false)
- Known valid IDs: {VALID_IDS}
- Non-existent ID behavior: {NONEXISTENT_BEHAVIOR} (e.g. "returns 404" or "returns 200 OK — mark with [Ignore]")

## Rules (ALL must be followed)

### File structure
- One test class per endpoint, file: `Api/Tests/{CLASS_NAME}.cs`
- File-scoped namespace: `namespace Api.Tests;`
- Usings at top: NUnit.Framework, RestSharp, Core.Models, Core.Config, Core.Helpers, System.Threading.Tasks, FluentAssertions, plus any needed (System.Net, System.Diagnostics, Api.Helpers)

### Class structure
- `[TestFixture]` + `[AllureNUnit]` + `[Category("{SERVICE}")]` class named `{CLASS_NAME}`
- `{SERVICE}` = `FakeStore`, `JsonPlaceholder`, or `Ui`
- Private field: `private RestClient _client = null!;`
- `[SetUp]`: `_client = new RestClient(Endpoints.BaseUrl);`
- `[TearDown]`: `_client?.Dispose();`

### Async rules (MANDATORY)
- Every test method MUST be `public async Task MethodName()` — never `void`
- All API requests MUST use async methods: `await _client.ExecuteAsync<T>(request)`
- NEVER use synchronous methods: `Execute`, `Execute<T>`, `Get<T>` — they block the thread
- Every `await` must be on an async method — no `.Result` or `.GetAwaiter().GetResult()`
- Setup/TearDown stay synchronous (RestSharp client creation/disposal is sync)

```csharp
// CORRECT — async
public async Task GetAllProducts_ReturnsOk()
{
    var request = new RestRequest(Endpoints.Products, Method.Get);
    var response = await _client.ExecuteAsync<List<ProductModel>>(request);
    response.ShouldHaveStatusCode(HttpStatusCode.OK);
}

// WRONG — sync, will be rejected
public void GetAllProducts_ReturnsOk()
{
    var request = new RestRequest(Endpoints.Products, Method.Get);
    var response = _client.Execute<List<ProductModel>>(request);  // blocks!
    response.ShouldHaveStatusCode(HttpStatusCode.OK);
}
```

### Test method rules
- Every test: `[Test]` + `[Category("...")]` + `[Description("X.Y description")]`
- Naming: `{ClassName}_{Scenario}_{ExpectedBehavior}` (PascalCase)
- Test numbering: continuous per class, starting from `{START_NUMBER}.1`
- Keep methods under 30 lines
- One assertion concept per test (can have multiple `.Should()` on same object)

### Test order in class (MANDATORY)

Tests within a class MUST follow this exact order — positive first, negative last:

1. **P0 — Happy Path** (status code, body not empty, type check)
2. **P1 — Validation** (field checks, id match, schema)
3. **P2 — Edge Cases** (non-existent ID, ID=0, negative ID)
4. **P3 — Performance** (response time, only for GET list endpoints)

This means: all tests that verify the endpoint works correctly come first, all tests that verify error handling come last. Never interleave positive and negative tests.

```
GetProductByIdTests:
  2.1  ReturnsOk           ← P0 happy path
  2.2  ReturnsNonEmptyBody  ← P0 happy path
  2.3  HasAllExpectedFields ← P1 validation
  2.4  IdMatchesRequested   ← P1 validation
  2.5  HasTitle             ← P1 validation
  2.6  HasPrice             ← P1 validation
  2.7  HasCategory          ← P1 validation
  2.8  HasRating            ← P1 validation
  2.9  NonExistentId  → [Ignore] ← P2 edge case
  2.10 ZeroId         → [Ignore] ← P2 edge case
  2.11 NegativeId     → [Ignore] ← P2 edge case
```

### Assertions
- Use FluentAssertions: `.Should().Be()`, `.Should().NotBeNull()`, etc.
- Use custom helpers when available:
  - `response.ShouldHaveStatusCode(HttpStatusCode.OK)` — from AssertHelper
  - `response.ShouldBeOk()` — status 200 + data not null
  - `response.Data!.ShouldHaveValidFields()` — validates via model attributes
  - `list.ShouldAllHaveValidProducts()` — validates all ProductModel items (only for product lists)
- For nested objects: `response.Data!.Field.Should().NotBeNull(); response.Data.Field.Prop.Should()...`

### Test categories (cover ALL applicable)

Every test MUST have `[Category]` attributes. See `Rules/categories.md` for full rules:
- Service category on class (`FakeStore` / `JsonPlaceholder` / `Ui`)
- Check-type category on method (`HealthCheck` / `Smoke` / `Regression` / `Negative` / `Performance`)
- Category-to-test mapping table

### Non-existent ID test pattern (for GET /{id} endpoints)

```csharp
[Test]
[Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
[Description("{N}.1 Get by non-existent ID (maxId + 1) — status code 404")]
public async Task {ClassName}_NonExistentId_ReturnsNotFound()
{
    var getAllRequest = new RestRequest(Endpoints.{ALL_ITEMS_CONSTANT}, Method.Get);
    var allItems = await _client.ExecuteAsync<List<{Model}>>(getAllRequest);
    var maxId = allItems.Data!.Max(p => p.Id);
    var nonExistentId = maxId + 1;

    var request = new RestRequest(Endpoints.{BY_ID_CONSTANT}, Method.Get);
    request.AddUrlSegment("id", nonExistentId);

    var response = await _client.ExecuteAsync<{Model}>(request);

    response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
}
```

### POST/PUT test pattern (when applicable)

```csharp
[Test]
[Description("{N}.1 Create {entity} — status code 200")]
public async Task {ClassName}_ValidData_ReturnsOk()
{
    var body = new {RequestModel}
    {
        // fill with valid test data
    };

    var request = new RestRequest(Endpoints.{ENDPOINT}, Method.Post);
    request.AddJsonBody(body);

    var response = await _client.ExecuteAsync<{ResponseModel}>(request);

    response.ShouldHaveStatusCode(HttpStatusCode.OK);
}
```

### DELETE test pattern (when applicable)

```csharp
[Test]
[Description("{N}.1 Delete {entity} by ID — status code 200")]
public async Task {ClassName}_ValidId_ReturnsOk()
{
    var request = new RestRequest(Endpoints.{BY_ID_CONSTANT}, Method.Delete);
    request.AddUrlSegment("id", {validId});

    var response = await _client.ExecuteAsync<{ResponseModel}>(request);

    response.ShouldHaveStatusCode(HttpStatusCode.OK);
}
```

## Output

Generate the complete test file content only. No explanations, no markdown wrapping — just the C# code ready to save.
```

---

## How to use

1. Pick an endpoint from `TestPlan.md` or the corresponding Endpoints file
2. Fill in the placeholders in the prompt above
3. Send the filled prompt to the AI
4. Review the generated test class against project rules
5. Save to `Api/Tests/{ClassName}.cs`
6. Run `dotnet build` and `dotnet test` to verify
7. Update `FILE_STRUCTURE.md` with the new file

## Placeholder reference

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{METHOD}` | HTTP method | GET, POST, PUT, DELETE |
| `{ENDPOINT_PATH}` | URL path with segments | /products/{id} |
| `{ENDPOINTS_CONSTANT}` | Constant name in Endpoints file | ProductsById |
| `{DESCRIPTION}` | Human-readable endpoint description | Get product by ID |
| `{REQUEST_MODEL}` | Model class for request body | ProductRequest |
| `{RESPONSE_MODEL}` | Model class for response body | ProductModel |
| `{IS_LIST}` | Does endpoint return a list? | true / false |
| `{VALID_IDS}` | Known valid IDs for testing | 1-20 |
| `{NONEXISTENT_BEHAVIOR}` | What API returns for missing ID | "returns 404" / "returns 200 OK — mark [Ignore]" |
| `{CLASS_NAME}` | Test class name | GetProductByIdTests |
| `{START_NUMBER}` | First test number prefix | 2 (if products list is 1) |
| `{EXPECTED_STATUS}` | Expected HTTP status code | 200, 201, 404 |
| `{SERVICE}` | Service category for [Category] | FakeStore, JsonPlaceholder, Ui |
| `{ALL_ITEMS_CONSTANT}` | Constant for list endpoint | Products |
| `{BY_ID_CONSTANT}` | Constant for by-ID endpoint | ProductsById |
| `{N}` | Test number prefix in descriptions | 2 |

## Existing endpoint constants

```
Products    = "/products"
ProductsById = "/products/{id}"
Carts       = "/carts"
CartsById   = "/carts/{id}"
Users       = "/users"
UsersById   = "/users/{id}"
Login       = "/auth/login"
```

## Example: filled prompt for GET /carts/{id}

```
Generate a test class for the endpoint: GET /carts/{id}

## Endpoint info

- Method: GET
- Path: /carts/{id}
- Path constant in Endpoints file: CartsById
- Description: Get cart by ID
- Request model (if POST/PUT): n/a
- Response model: CartModel
- Response is a list: false
- Known valid IDs: 1-7
- Non-existent ID behavior: returns 200 OK — mark with [Ignore]

## Rules (ALL must be followed)
... (paste full rules section from above) ...
```
