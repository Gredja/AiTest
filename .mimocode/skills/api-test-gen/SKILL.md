---
name: api-test-gen
description: Use when the user wants to generate API tests for a FakeStoreAPI endpoint. Trigger on mentions of "api test gen", "generate API tests", "/api-test-gen", or testing a new endpoint.
---

# API Test Generation

Generate a complete set of NUnit API tests for a FakeStoreAPI endpoint.

## Input

User provides an endpoint name (e.g. `products`, `users`, `carts`, `login`).

If no argument provided, ask which endpoint to test.

## Flow

### Step 1: Research the endpoint

1. Fetch the endpoint from FakeStoreAPI: `GET https://fakestoreapi.com/{endpoint}`
2. Analyze the response structure — what fields, what types, what's nullable
3. Check for sub-endpoints: `/{endpoint}/{id}`, nested resources
4. Count total items (needed for `ExpectedCount` constant)

### Step 2: Add constants to Endpoints.cs

Update `Core/Config/Endpoints.cs`:
- `Expected{Endpoint}Count` — total items from API
- `Test{Endpoint}Id` — valid ID for single-item tests (use ID = 1)
- `{Endpoint}` — path string (e.g. `/users`)
- `{Endpoint}ById` — path with segment (e.g. `/users/{id}`)

### Step 3: Generate Models

Create or update models in `Core/Models/`:

**Main model** (`{Endpoint}Model.cs`):
- Suffix: `Model` for response, `Request` for request body
- Always includes `Id` field with `[PositiveId]` attribute
- Reference types (string, object, nested model): `[RequiredField]`, no `?`, no initializer
- Value types (int, decimal, double): `?` only if JSON field can be null/absent
- `[ValueRange(min, max)]` on numeric fields with known bounds (e.g. rating 0-5, price >= 0)
- Namespace: `Core.Models`
- Pure data container — no constructors, no validation logic

**Attribute style** — always on separate line above property:
```csharp
[PositiveId]
public int Id { get; set; }

[RequiredField]
public string Title { get; set; }

[ValueRange(0, 5)]
public double Rate { get; set; }
```

**Nested models** — separate file for each nested object (e.g. `RatingModel`, `UserNameModel`, `AddressModel`).

If models already exist — verify they match current API response.

### Step 4: Generate Assert Helper

Create `Api/Helpers/{Endpoint}AssertHelper.cs` following the pattern of `ProductAssertHelper.cs`:

```csharp
using Core.Helpers;
using Core.Models;
using FluentAssertions;
using RestSharp;

namespace Api.Helpers;

public static class {Endpoint}AssertHelper
{
    public static void ShouldBeOkWithValid{Endpoint}(this RestResponse<{Endpoint}Model> response)
    {
        response.ShouldBeOk();
        response.Data!.ShouldHaveValidFields();
    }

    public static void ShouldAllHaveValidId(this List<{Endpoint}Model> items)
    {
        items.Should().OnlyContain(i => i.Id > 0, "all items must have positive Id");
    }

    // One method per field that needs list-level validation:
    // ShouldAllHaveValid{Name}, ShouldAllHaveValid{Field}, etc.

    public static void ShouldAllHaveValid{Endpoint}s(this List<{Endpoint}Model> items)
    {
        items.ShouldAllHaveValidId();
        // call all individual validators
    }
}
```

### Step 5: Generate Tests — split into TWO files

**File 1: `Api/Tests/GetAll{Endpoint}Tests.cs`**

Tests for the list endpoint. Each test calls the API independently.

```csharp
using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Api.Helpers;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class GetAll{Endpoint}Tests : RequestHelper
{
    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("X.1 Status code is 200")]
    public async Task GetAll{Endpoint}_ReturnsOk()
    {
        var response = await Get<List<{Endpoint}Model>>(Endpoints.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("X.2 Response body is not empty")]
    public async Task GetAll{Endpoint}_ReturnsNonEmptyList() { /* ... */ }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("X.3 Content-Type is application/json")]
    public async Task GetAll{Endpoint}_ContentTypeIsJson() { /* ... */ }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("X.4 Each item has `id` (integer)")]
    public async Task GetAll{Endpoint}_EachItemHasId() { /* ... */ }

    // One test per field: email, username, name, phone, address, etc.

    [Test]
    [Category("Performance")]
    [Category("Slow")]
    [Description("X.N Response time < 5 seconds")]
    public async Task GetAll{Endpoint}_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<{Endpoint}Model>>(Endpoints.{Endpoint}, Method.Get);
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(Endpoints.MaxResponseTimeMs);
    }

    [Test]
    [Category("Validation")]
    [Category("Fast")]
    [Description("X.N+1 Returns expected count")]
    public async Task GetAll{Endpoint}_ReturnsExpectedCount()
    {
        var response = await Get<List<{Endpoint}Model>>(Endpoints.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount(Endpoints.Expected{Endpoint}Count);
    }
}
```

**File 2: `Api/Tests/GetById{Endpoint}Tests.cs`**

Tests for single-item endpoint. Use DRY helper for params:

```csharp
namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class GetById{Endpoint}Tests : RequestHelper
{
    private static List<RequestDictionaryModel> {Endpoint}IdParam(int id) =>
        new() { new() { Type = "UrlSegment", Key = "id", Value = id } };

    [Test]
    [Description("Y.1 Get by valid ID — status code 200")]
    public async Task GetById{Endpoint}_ValidId_ReturnsOk()
    {
        var response = await Get<{Endpoint}Model>(
            Endpoints.{Endpoint}ById,
            Method.Get,
            {Endpoint}IdParam(Endpoints.Test{Endpoint}Id));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    // One test per field validation

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
    [Description("Y.N Get by non-existent ID (maxId + 1) — status code 404")]
    public async Task GetById{Endpoint}_NonExistentId_ReturnsNotFound()
    {
        var allItems = await Get<List<{Endpoint}Model>>(Endpoints.{Endpoint}, Method.Get);
        var maxId = allItems.Data!.Max(i => i.Id);
        var nonExistentId = maxId + 1;

        var response = await Get<{Endpoint}Model>(
            Endpoints.{Endpoint}ById,
            Method.Get,
            {Endpoint}IdParam(nonExistentId));

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for ID=0")]
    [Description("Y.N+1 Get by ID = 0 — status code 404")]
    public async Task GetById{Endpoint}_ZeroId_ReturnsNotFound() { /* ... */ }

    [Test]
    [Ignore("FakeStoreAPI returns 200 OK instead of 404 for negative IDs")]
    [Description("Y.N+2 Get by negative ID (-1) — status code 404")]
    public async Task GetById{Endpoint}_NegativeId_ReturnsNotFound() { /* ... */ }
}
```

### Step 6: Follow All Rules

- **Code:** PascalCase, file-scoped namespaces, async (`ExecuteAsync`), no magic numbers
- **Non-existent IDs:** Dynamic only — GET all → `maxId + 1`. Never static 999 or any hardcoded number
- **Assertions:** FluentAssertions only — `.Should().Be()`, `.NotBeNull()`, `.NotBeNullOrWhiteSpace()`, `.BeGreaterThan()`, `.BeInRange()`, `.OnlyContain()`
- **Attributes on separate lines above properties**, not inline
- **Config:** Use `Endpoints.*` from `Core/Config/Endpoints.cs` — never hardcode URLs
- **Comments:** No comments unless regex or non-obvious WHY
- **Models:** pure data containers, no constructors, no validation logic
- **Negative tests:** `[Ignore]` attribute with explanation for FakeStoreAPI known bugs

### Step 7: Safety Check

Before finishing:
1. Run `dotnet format --verify-no-changes` — fix if needed
2. Run `dotnet test --filter "GetAll{Endpoint}|GetById{Endpoint}"` — verify new tests pass
3. Show test results summary

### Step 8: Report

Show:
- Files created/modified
- Test count (total, positive, negative, edge cases)
- Any issues found during safety check
- Suggested commit message
