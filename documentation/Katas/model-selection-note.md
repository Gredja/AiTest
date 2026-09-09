# Model Selection Note

**Date:** 2026-06-25
**Author:** Алексей — AQA Engineer
**Project:** Gredja
**Task:** Генерация NUnit API-тестов для эндпоинта FakeStoreAPI через скилл /api-test-gen
**Committed location:** `Gredja/documentation/Katas/model-selection-note.md`

---

## Evaluation Criteria

| # | Criterion | Why it matters for this task |
|---|-----------|------------------------------|
| 1 | Соответствие шаблону | Единообразие |
| 2 | Полнота | Тесты должны покрывать большое количество вариантов |
| 3 | Code-style | Закреплено в команде |
| 4 | Простота | Код теста должен говорить сам за себя, комментарии не нужны |

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
> Файл: `UsersTests.cs` — дублирует существующие `GetAllUsersTests.cs` + `GetUserByIdTests.cs`. Лишний import `Core.Models`. Лишняя промежуточная переменная `nonExistentId`. Двойные проверки `ShouldHaveStatusCode` + `NotBeEmpty` в `ReturnsNonEmptyList`.

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
> Файл: `UsersTests_Main.cs` — тоже дублирует существующие файлы. Нет лишней переменной. Нет двойных проверок. Чище по code-style. Лишний import `Core.Models`.

---

## Scorecard

| Criterion | Model A score (1–3) | Model A evidence | Model B score (1–3) | Model B evidence |
|-----------|---------------------|------------------|---------------------|------------------|
| Соответствие шаблону | 2 | Создал дублирующий файл вместо работы с существующими GetAllUsersTests.cs + GetUserByIdTests.cs | 2 | Создал дублирующий файл вместо работы с существующими GetAllUsersTests.cs + GetUserByIdTests.cs |
| Полнота | 3 | 12 тестов, все кейсы покрыты (GET all, GET by id, invalid id) | 3 | 12 тестов, все кейсы покрыты (GET all, GET by id, invalid id) |
| Code-style | 2 | Лишний import Core.Models, двойные ShouldHaveStatusCode в ReturnsNonEmptyList, промежуточная переменная nonExistentId | 3 | Лишний import Core.Models, но нет двойных проверок, нет лишних переменных |
| Простота | 2 | Избыточные ShouldHaveStatusCode перед NotBeEmpty, лишняя переменная nonExistentId | 3 | Минимальный код, нет лишних проверок и переменных |
| **Total** | **9** | | **11** | |

---

## Decision

**Selected model:** MiMo V2.5 Pro (main)

**Rationale:** Main выиграл — чище по code-style (3 vs 2), нет лишних проверок и переменных. Но обе модели проиграли на соответствии шаблону — создали дублирующие файлы вместо работы с существующими. Это самая грубая ошибка: не прочитали код проекта перед генерацией.

---

## Active Constraint

**What could change this decision within 30 days:**
Внешние модели (Claude, DeepSeek) сейчас недоступны из-за API-ошибок — если доступ появится и стоимость снизится, повторное сравнение с учётом чтения существующего кода может изменить результат.

---

## Revision history

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-06-25 | Initial commit |
