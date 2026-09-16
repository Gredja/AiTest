---
name: e2e-test-gen
description: Use when the user wants to generate E2E tests for write operations (POST/PATCH/DELETE) with setup and teardown. Trigger on mentions of "e2e test gen", "generate e2e tests", "/e2e-test-gen", or testing create/update/delete scenarios.
---

# E2E Test Generation

## Purpose

Generates NUnit E2E test classes for write operations (POST, PATCH, DELETE) with mandatory setup/teardown for resource lifecycle management. Each test creates resources, tests behavior, and cleans up after itself.

---

## Variable Placeholders

| Placeholder | Description | Example value |
|---|---|---|
| `{{service_name}}` | API service (must match existing directory in `E2E/`) | GitHub |
| `{{endpoint_name}}` | Resource to test (e.g. issues, comments, pull requests) | issues |

---

## Output Format Instruction

Model must return: test files (.cs) in C# format with namespace, class, setup, teardown, test methods. Format — code without comments. After generation — report: list of files, test count (total / positive / negative / ignored).

---

## Input

User provides:
1. **Service name** — `{{service_name}}` (must match existing directory in `E2E/`)
2. **Endpoint name** — `{{endpoint_name}}` (e.g. `issues`, `comments`, `pulls`)
3. **Sandbox repo** — target repo for write operations (e.g. `Gredja/AiTest`)

If not provided, ask which service and endpoint to test.

---

## File Convention

| What | Path pattern |
|---|---|
| Observable Behaviour | `documentation/{Service}ObservableBehaviour.md` |
| E2E Tests | `E2E/{Service}/Tests/{Endpoint}Tests.cs` |
| E2E Test Base | `E2E/{Service}/{Service}E2ETestBase.cs` |
| Endpoints config | `Core/Config/{Service}Endpoints.cs` |
| Request models | `Core/Models/{Service}/` |
| Namespace (tests) | `E2E.{Service}.Tests` |

---

## Flow

### Step 1: Read Observable Behaviour

Read `documentation/{Service}ObservableBehaviour.md`. Focus on:
- POST/PATCH/DELETE endpoint sections (Write Operations)
- Request Body table — required vs optional fields
- Response status codes (201 Created, 200 OK, 204 No Content)
- Negative cases (401, 403, 404, 422)
- Risk Framing → Write Operation Risks

### Step 2: Design resource lifecycle

For each E2E scenario, map the lifecycle:

```
Setup:    POST → create resource → store ID
Test:     GET/PATCH/DELETE → verify behavior
Teardown: DELETE → clean up resource → verify cleanup
```

**Cleanup order matters:**
1. Delete comments before issues
2. Delete branches before PRs
3. Delete PRs before repos (if applicable)

### Step 3: Plan test cases (seed methodology)

Apply seed methodology from `Rules/test-practices.md`:

**Happy path seeds:**
- Create → verify response → verify via GET → delete → verify deleted
- Create with all fields → verify all fields present
- Update → verify changed fields → verify unchanged fields preserved
- Create multiple → verify count → delete one → verify count decremented

**Negative seeds:**
- Create without auth → 401
- Create with missing required field → 422
- Update non-existent resource → 404
- Delete non-existent resource → 404
- Create in non-existent repo → 404

**Edge seeds:**
- Create with special chars in fields
- Rapid create-delete-create cycle
- Concurrent creates

Output as table: `# | Case | Category | Priority | Source seed`

### Step 4: Generate test class

**Required pattern:**

```csharp
using NUnit.Framework;
using RestSharp;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;
using static Core.Helpers.GitHub.GitHubParamHelper;

namespace E2E.{Service}.Tests;

[TestFixture]
[AllureNUnit]
[Category("{Service}E2E")]
public class {Endpoint}Tests : {Service}E2ETestBase
{
    private int _createdId;

    [SetUp]
    public async Task Setup()
    {
        // Create resource for test
        var response = await Post<CreateRequest, ResponseModel>(
            {Service}Endpoints.{Endpoint}, _testBody, ...);
        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        _createdId = response.Data!.Id;
    }

    [TearDown]
    public async Task Teardown()
    {
        // Always clean up — even if test failed
        try
        {
            await Delete<object>({Service}Endpoints.{Endpoint}ById, IdParam(_createdId));
        }
        catch
        {
            // Log but don't fail teardown
        }
    }

    [Test]
    [Category("Smoke")]
    [Description("1.1 Create returns 201")]
    public async Task Create_ReturnsCreated()
    {
        // Resource already created in Setup
        // Verify it exists
        var response = await Get<ResponseModel>(
            {Service}Endpoints.{Endpoint}ById, Method.Get, IdParam(_createdId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data!.Id.Should().Be(_createdId);
    }
}
```

**Key rules:**
- `[SetUp]` creates the resource, stores ID in `_createdId`
- `[TearDown]` always deletes — wrapped in try/catch to not mask test failures
- Setup failure = test skipped (SetUpException), not failed
- Each test is independent — Setup creates fresh resource every time
- Use `ShouldMatchRequest()` for request/response comparison

### Step 5: Generate cleanup helpers (if needed)

For complex cleanup chains, add helper methods:

```csharp
private async Task CleanupComments(int issueNumber)
{
    var comments = await Get<List<CommentModel>>(..., IssueNumberParam(issueNumber));
    foreach (var comment in comments.Data ?? new())
    {
        await Delete<object>(..., CommentIdParam(comment.Id));
    }
}
```

### Step 6: Follow All Rules

- **Code:** PascalCase, file-scoped namespaces, async, no magic numbers — see `Rules/code.md`
- **Cleanup:** mandatory, try/catch in TearDown — see `Rules/test-practices.md`
- **Assertions:** ShouldHaveStatusCode, ShouldHaveValidFields, ShouldMatchRequest (reduces code vs manual field comparison) — see `Rules/assertions.md`
- **Non-existent IDs:** dynamic (GET all → maxId + 1) — see `Rules/test-practices.md`
- **Mock API behavior:** `[Ignore]` with explanation for known mock limitations — see `Rules/test-practices.md`
- **Seed methodology:** 5 seeds → expand to table → enforce 5+ negatives — see `Rules/test-practices.md`

### Step 7: Safety Check

1. `dotnet format --verify-no-changes` — fix if needed
2. `dotnet build` — verify compilation
3. `dotnet test --filter "Category={Service}E2E"` — verify tests pass
4. Show test results summary

### Step 8: Report

- Files created/modified
- Test count (total, active, ignored)
- Cleanup chain verified
- Suggested commit message

---

## Key Differences from api-test-gen

| Aspect | api-test-gen (read-only) | e2e-test-gen (write) |
|---|---|---|
| HTTP methods | GET only | POST, PATCH, DELETE |
| Setup/Teardown | Not needed | Mandatory |
| Resource cleanup | No | Yes — try/catch in TearDown |
| Test independence | Stateless | Each test creates own resources |
| Sandbox repo | Reads from any repo | Writes only to sandbox |
| Risk level | Low (no side effects) | High (creates/deletes resources) |
| Mock behavior | Rarely ignored | Often ignored (mock APIs don't persist) |

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-02-21 | Initial commit | Алексей |
