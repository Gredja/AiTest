---
description: Use when the user wants to generate API tests for a FakeStoreAPI endpoint
argument-hint: [endpoint name, e.g. "products", "users", "carts"]
---

# Skill: API Test Generation

Generate a complete set of NUnit API tests for the `{endpoint}` endpoint of FakeStoreAPI.

## Input

`$ARGUMENTS` = endpoint name (e.g. `products`, `users`, `carts`, `login`).

If no argument provided, ask which endpoint to test.

## Flow

### Step 1: Research the endpoint

1. Fetch the endpoint from FakeStoreAPI: `GET https://fakestoreapi.com/{endpoint}`
2. Analyze the response structure — what fields, what types, what's nullable
3. Check for sub-endpoints: `/{endpoint}/{id}`, nested resources

### Step 2: Generate Model

Create or update `Core/Models/{Endpoint}Model.cs` following Rules/models.md:

- Suffix: `Model` for response, `Request` for request body
- Reference types: no `?`, no initializer
- Value types: `?` only if JSON field can be null/absent
- Namespace: `Core.Models`
- Pure data container — no constructors, no validation, no logic

If model already exists — verify it matches current API response.

### Step 3: Generate Test Cases

Create `Api/Tests/{Endpoint}ApiTests.cs` with these test categories:

**Positive tests:**
- `GetAll_{Endpoint}_ReturnsOk` — GET all, verify count and structure
- `GetAll_{Endpoint}_ReturnsCorrectContentType` — verify JSON content type
- `GetById_{Endpoint}_ReturnsOk` — GET by valid ID (use real ID from step 1)
- `GetById_{Endpoint}_ReturnsCorrectFields` — verify all fields present

**Negative tests:**
- `GetById_{Endpoint}_NonExistentId_ReturnsNotFound` — ID = maxId + 1 (dynamic!)
- `GetById_{Endpoint}_ZeroId_ReturnsNotFound` — ID = 0
- `GetById_{Endpoint}_NegativeId_ReturnsNotFound` — ID = -1

**Edge cases:**
- `GetAll_{Endpoint}_ReturnsNonEmptyCollection` — verify collection is not empty
- `GetById_{Endpoint}_FirstItem_HasValidId` — first item's ID > 0
- `GetById_{Endpoint}_LastItem_HasValidId` — last item's ID > 0

### Step 4: Follow All Rules

- **Code:** PascalCase, file-scoped namespaces, async (`ExecuteAsync`), no magic numbers
- **Assertions:** FluentAssertions — `.Should().Be()`, `.NotBeNull()`, `.NotBeNullOrWhiteSpace()`
- **Config:** Use `FakeStoreEndpoints.{Endpoint}` or `JsonPlaceholderEndpoints.{Endpoint}` from the corresponding config file — never hardcode URLs
- **Comments:** No comments unless regex or non-obvious WHY

### Step 5: Safety Check

Before finishing:
1. Run `dotnet format --verify-no-changes` — fix if needed
2. Run `dotnet test --verbosity quiet` — verify new tests pass
3. Show test results summary

### Step 6: Report

Show:
- Files created/modified
- Test count (total, positive, negative, edge cases)
- Any issues found during safety check
- Suggested commit message

## Example

User: `/api-test-gen products`

Agent:
1. Fetches `https://fakestoreapi.com/products` → analyzes structure
2. Creates/updates `Core/Models/ProductModel.cs` with fields: Id, Title, Price, Description, Category, Image, Rating (nested)
3. Creates `Api/Tests/ProductsApiTests.cs` with 10+ test methods
4. Runs format + test
5. Reports: "Created ProductModel + ProductsApiTests (10 tests). All passing."
