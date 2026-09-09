# Model Selection Note

**Date:** 2026-06-25
**Author:** Алексей — AQA Engineer
**Project:** Gredja
**Task:** Generate NUnit API tests for FakeStoreAPI endpoint via /api-test-gen skill
**Committed location:** `Gredja/documentation/Katas/model-selection-note.md`

---

## Evaluation Criteria

| # | Criterion | Why it matters for this task |
|---|-----------|------------------------------|
| 1 | Template compliance | Consistency |
| 2 | Completeness | Tests should cover a large number of scenarios |
| 3 | Code-style | Established in the team |
| 4 | Simplicity | Test code should speak for itself, comments are not needed |

---

## Prompt Used

You are generating API tests for FakeStoreAPI /users endpoint.

Generate a COMPLETE set of NUnit API tests following these rules:

## Task
1. Fetch https://fakestoreapi.com/users to analyze response structure
2. Create/update constants in Core/Config/FakeStoreEndpoints.cs (ExpectedUsersCount, TestUsersId, Users, UsersById)
3. Create/update models in Core/Models/FakeStore/ (UsersModel.cs + nested models like UserNameModel, AddressModel, etc.)
4. Generate tests in Api/FakeStore/Tests/UsersTests.cs

## Rules to follow
- Namespace: Api.FakeStore.Tests
- Base class: RequestHelper
- Pattern: ShouldHaveValidFields() via attributes, NOT per-field helpers
- PascalCase classes/methods/properties/constants
- File-scoped namespaces, one class per file
- All API requests async (ExecuteAsync)
- FluentAssertions only (not NUnit Assert)
- Attributes on separate lines above properties, not inline
- Reference types (string, object): no ?, no initializer, [RequiredField]
- Value types: ? only if JSON field can be null/absent
- [PositiveId] on Id, [ValueRange(min,max)] on numeric fields with known bounds
- Pure data containers — no constructors, no validation logic
- Non-existent IDs: dynamic only — GET all → maxId + 1. Never static 999
- Comments: NO comments unless regex or non-obvious WHY
- Config: use FakeStoreEndpoints.* — never hardcode URLs
- Negative tests: [Ignore] with explanation for API known bugs

## Tests to generate
GET /users (all), GET /users/{id} (single), GET /users/{invalid} (non-existent)

## Output
Write the generated files. Show files created/modified and test count summary.

---

## Output Comparison

### Model A: MiMo V2.5 Pro (subagent)
> ```csharp
> [Test]
> [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
> [Description("5.1 GET /users/{id} — non-existent ID (maxId + 1) returns 404")]
> public async Task GetUserById_NonExistentId_ReturnsNotFound()
> {
>     var allUsers = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);
>     var maxId = allUsers.Data!.Max(u => u.Id);
>     var nonExistentId = maxId + 1;
>
>     var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(nonExistentId));
>     response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
> }
> ```
> File: `UsersTests.cs` — duplicates existing `GetAllUsersTests.cs` + `GetUserByIdTests.cs`. Redundant import `Core.Models`. Redundant intermediate variable `nonExistentId`. Double checks `ShouldHaveStatusCode` + `NotBeEmpty` in `ReturnsNonEmptyList`.

### Model B: MiMo V2.5 Pro (main)
> ```csharp
> [Test]
> [Ignore("FakeStoreAPI returns 200 OK instead of 404 for non-existent IDs")]
> [Description("3.1 GET /users/{id} — non-existent ID (maxId + 1) returns 404")]
> public async Task GetUserById_NonExistentId_ReturnsNotFound()
> {
>     var allUsers = await Get<List<UserModel>>(FakeStoreEndpoints.Users, Method.Get);
>     var maxId = allUsers.Data!.Max(u => u.Id);
>
>     var response = await Get<UserModel>(FakeStoreEndpoints.UsersById, Method.Get, UserIdParam(maxId + 1));
>     response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
> }
> ```
> File: `UsersTests_Main.cs` — also duplicates existing files. No redundant variable. No double checks. Cleaner code-style. Redundant import `Core.Models`.

---

## Scorecard

| Criterion | Model A score (1–3) | Model A evidence | Model B score (1–3) | Model B evidence |
|-----------|---------------------|------------------|---------------------|------------------|
| Template compliance | 2 | Created duplicate file instead of working with existing GetAllUsersTests.cs + GetUserByIdTests.cs | 2 | Created duplicate file instead of working with existing GetAllUsersTests.cs + GetUserByIdTests.cs |
| Completeness | 3 | 12 tests, all cases covered (GET all, GET by id, invalid id) | 3 | 12 tests, all cases covered (GET all, GET by id, invalid id) |
| Code-style | 2 | Redundant import Core.Models, double ShouldHaveStatusCode in ReturnsNonEmptyList, intermediate variable nonExistentId | 3 | Redundant import Core.Models, but no double checks, no redundant variables |
| Simplicity | 2 | Redundant ShouldHaveStatusCode before NotBeEmpty, redundant variable nonExistentId | 3 | Minimal code, no redundant checks or variables |
| **Total** | **9** | | **11** | |

---

## Decision

**Selected model:** MiMo V2.5 Pro (main)

**Rationale:** Main won — cleaner code-style (3 vs 2), no redundant checks or variables. But both models failed on template compliance — created duplicate files instead of working with existing ones. This is the most critical error: didn't read the project code before generation.

---

## Active Constraint

**What could change this decision within 30 days:**
External models (Claude, DeepSeek) are currently unavailable due to API errors — if access appears and cost decreases, a re-comparison with consideration of reading existing code may change the result.

---

## Revision history

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-06-25 | Initial commit |
