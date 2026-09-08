---
name: api-test-gen
description: Use when the user wants to generate API tests for a FakeStoreAPI or JSONPlaceholder endpoint. Trigger on mentions of "api test gen", "generate API tests", "/api-test-gen", or testing a new endpoint.
---

# API Test Generation

Generate a complete set of NUnit API tests for an API endpoint.

## Input

User provides:
1. **Service name** — `FakeStore` or `JsonPlaceholder`
2. **Endpoint name** (e.g. `products`, `users`, `posts`, `todos`)

If not provided, ask which service and endpoint to test.

## Flow

### Step 1: Research the endpoint

1. Fetch the endpoint from the API
2. Analyze the response structure — what fields, what types, what's nullable
3. Check for sub-endpoints: `/{endpoint}/{id}`, nested resources
4. Count total items (needed for `ExpectedCount`)

### Step 2: Add constants to Endpoints file

Update the appropriate endpoints file:
- **FakeStore:** `Core/Config/FakeStoreEndpoints.cs`
- **JsonPlaceholder:** `Core/Config/JsonPlaceholderEndpoints.cs`

Add:
- `Expected{Endpoint}Count` — total items from API
- `Test{Endpoint}Id` — valid ID for single-item tests (use ID = 1)
- `{Endpoint}` — path string (e.g. `/users`)
- `{Endpoint}ById` — path with segment (e.g. `/users/{id}`)

### Step 3: Generate Models

Create or update models in `Core/Models/{Service}/`:

**Main model** (`{Endpoint}Model.cs`):
- Suffix: `Model` for response, `Request` for request body
- Always includes `Id` field with `[PositiveId]` attribute
- Reference types (string, object, nested model): `[RequiredField]`, no `?`, no initializer
- Value types (int, decimal, double): `?` only if JSON field can be null/absent
- `[ValueRange(min, max)]` on numeric fields with known bounds (e.g. rating 0-5, price >= 0)
- Namespace: `Core.Models.{Service}`
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

**Check for shared base classes** — if 2+ models share identical fields with same types, create a base class in `Core/Models/Generic/` and inherit (e.g. `UserOwnedModel` for entities with `Id` + `UserId` + `Title`).

If models already exist — verify they match current API response.

### Step 4: Generate Tests

**File location:**
- **FakeStore:** `Api/FakeStore/Tests/{Endpoint}Tests.cs`
- **JsonPlaceholder:** `Api/JsonPlaceholder/Tests/{Endpoint}Tests.cs`

**Namespace:**
- **FakeStore:** `Api.FakeStore.Tests`
- **JsonPlaceholder:** `Api.JsonPlaceholder.Tests`

**Base class:**
- **FakeStore:** `RequestHelper`
- **JsonPlaceholder:** `JsonPlaceholderRequestHelper`

**Pattern:** Use `ShouldHaveValidFields()` via attributes — NOT per-field helpers.

```csharp
using NUnit.Framework;
using RestSharp;
using Core.Models.{Service};
using Core.Config;
using Core.Helpers;
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.{Service}.Tests;

[TestFixture]
[AllureNUnit]
public class GetAll{Endpoint}Tests : {BaseClass}
{
    [Test]
    [Description("1.1 Status code is 200")]
    public async Task GetAll{Endpoint}_ReturnsOk()
    {
        var response = await Get<List<{Endpoint}Model>>({Endpoints}.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Description("1.2 Response body is not empty")]
    public async Task GetAll{Endpoint}_ReturnsNonEmptyList()
    {
        var response = await Get<List<{Endpoint}Model>>({Endpoints}.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Description("1.3 Content-Type is application/json")]
    public async Task GetAll{Endpoint}_ContentTypeIsJson()
    {
        var response = await Get<List<{Endpoint}Model>>({Endpoints}.{Endpoint}, Method.Get);
        response.ContentType.Should().Contain("application/json");
    }

    [Test]
    [Description("1.4 Each item has valid required fields (via attributes)")]
    public async Task GetAll{Endpoint}_EachItemHasValidFields()
    {
        var response = await Get<List<{Endpoint}Model>>({Endpoints}.{Endpoint}, Method.Get);
        foreach (var item in response.Data!)
        {
            item.ShouldHaveValidFields();
        }
    }

    [Test]
    [Description("1.5 Response time < 5 seconds")]
    public async Task GetAll{Endpoint}_ResponseTimeIsAcceptable()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await Get<List<{Endpoint}Model>>({Endpoints}.{Endpoint}, Method.Get);
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan({Endpoints}.MaxResponseTimeMs);
    }

    [Test]
    [Description("1.6 Returns expected count")]
    public async Task GetAll{Endpoint}_ReturnsExpectedCount()
    {
        var response = await Get<List<{Endpoint}Model>>({Endpoints}.{Endpoint}, Method.Get);
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().HaveCount({Endpoints}.Expected{Endpoint}Count);
    }
}
```

**For negative tests (non-existent ID):**
- Use dynamic approach: `GET all → maxId + 1`
- Never hardcode 999 or any static number
- If API returns 200 instead of 404 — use `[Ignore]` with explanation

### Step 5: Follow All Rules

- **Code:** PascalCase, file-scoped namespaces, async (`ExecuteAsync`), no magic numbers
- **Non-existent IDs:** Dynamic only — GET all → `maxId + 1`. Never static 999 or any hardcoded number
- **Assertions:** FluentAssertions only. Use `ShouldHaveValidFields()` via attributes — not per-field helpers
- **Attributes on separate lines above properties**, not inline
- **Config:** Use `{Endpoints}.*` from appropriate Endpoints file — never hardcode URLs
- **Comments:** No comments unless regex or non-obvious WHY
- **Models:** pure data containers, no constructors, no validation logic. Check for shared base classes
- **Negative tests:** `[Ignore]` attribute with explanation for API known bugs
- **Access modifiers:** narrowest possible — private > protected > public

### Step 6: Safety Check

Before finishing:
1. Run `dotnet format --verify-no-changes` — fix if needed
2. Run `dotnet test --filter "GetAll{Endpoint}"` — verify new tests pass
3. Show test results summary

### Step 7: Report

Show:
- Files created/modified
- Test count (total, positive, negative, edge cases)
- Any issues found during safety check
- Suggested commit message
