# AGENTS.md — Gredja

AQA-проект. API-тесты (NUnit + RestSharp): FakeStoreAPI, JSONPlaceholder, GitHub API.
E2E тесты (NUnit + RestSharp): GitHub write operations.

## Project Structure

- `Api/` — NUnit API-тесты (FakeStore, JsonPlaceholder, GitHub)
- `E2E/` — NUnit E2E тесты (GitHub write operations)
- `Core/Models/` — модели ответов/запросов
- `Core/Config/FakeStoreEndpoints.cs`, `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs` — URL и пути эндпоинтов
- `Prompts/` — шаблоны промптов
- `Rules/` — полные правила (здесь — краткая сводка)
- `documentation/` — документация проекта
- `documentation/*ObservableBehaviour.md` — наблюдаемое поведение API (актуальные состояния, типы полей, негативные кейсы). **Читать при генерации и review тестов.**
- `.mimocode/skills/` — скиллы для AI-агентов

## Rules

### Workflow
Все изменения: план → одобрение → отчёт. После коммита — review `Rules/`. После структурных изменений — обновить документацию. Выполненные планы (`.mimocode/plans/`) — удалять сразу после реализации.

### Code
- Naming: PascalCase (classes, methods, properties, constants), camelCase (locals, params), `_camelCase` (private fields)
- Без аббревиатур (`response`, не `resp`). Boolean: `Is`, `Has`, `Can`, `Should`
- File-scoped namespaces, one class per file, explicit types > var (unless obvious)
- Methods: short, one responsibility, max ~30 lines, max 5 params
- Все API-запросы async (`ExecuteAsync`, не `Execute`)
- Нет модификатора = private. Нет magic numbers. Нет вложенных ternary. `nameof()` для exceptions
- Error handling: конкретные исключения, без `null!`, без exceptions для flow control
- LINQ: `Any()` вместо `Count() > 0`, без лишних `.ToList()`, `FirstOrDefault()` вместо `Where().FirstOrDefault()`
- Strings: интерполяция `$""`, `StringBuilder` в циклах, `IsNullOrEmpty()` вместо `.Length == 0`
- Null safety: `?.` для safe navigation, `??` для fallback, `is not null` вместо `!= null`
- SOLID: один класс — одна задача, зависимости через интерфейсы, расширяемость через наследование

### Models
- Response: suffix `Model` (включает `Id`). Request: suffix `Request` (без `Id`)
- Reference types (string, object, List): без `?`, без initializer
- Value types (int, decimal, DateTime): `?` только если JSON-поле может быть null/absent
- Namespace: `Core.Models`. Чистые контейнеры данных — без конструкторов, валидации, логики

### Assertions
- FluentAssertions (не NUnit Assert)
- Ключевые: `.Should().Be()`, `.NotBeNull()`, `.NotBeNullOrWhiteSpace()`, `.BeGreaterThan()`, `.BeInRange()`, `.OnlyContain()`
- Helper-методы: `ShouldHaveStatusCode()`, `ShouldHaveValidContract()`, `ShouldHaveValidFields()`, `ShouldMatchRequest()` — в `AssertHelper.cs`

### Comments
- По умолчанию: без комментариев. Код говорит сам за себя.
- Исключения: regex-объяснения, TODO (только в dev, удалить до merge), non-obvious WHY

### Config
- Base URL и эндпоинты в `Core/Config/FakeStoreEndpoints.cs`, `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs`. Никогда не хардкодить в тестах.

### Git
- Repo: github.com/Gredja/AiTest.git, branch: `main`
- Изменения в `features/<topic>` ветках
- Коммиты: только по запросу, английский, формат: action + object
- Никогда не коммитить `.env` или токены
- **Перед коммитом:** `dotnet format --verify-no-changes` + `dotnet test` — оба должны пройти

## Key Decisions
- 0 и -1 — безопасные static IDs (всегда невалидные)
- Dynamic non-existent ID = maxId + 1 (не статический 999)
- FakeStoreAPI: ровно 20 товаров (IDs 1-20)
- Seed methodology: 5 seeds → expand → enforce 5+ negatives → see `Rules/test-practices.md`
- Document sync: Observable Behaviour ↔ Test Plan ↔ Rules — always in sync
- Guarantee Data: GET пуст → POST в OneTimeSetUp → GET снова → Assertion → DELETE в OneTimeTearDown

## Detailed Rules (Rules/)

- `Rules/code.md` — naming, types, file structure, methods, async, access modifiers, cleanup
- `Rules/code-style.md` — error handling, LINQ, strings, null safety
- `Rules/code-principles.md` — SOLID, general principles
- `Rules/models.md` — model building rules (Model/Request, properties, naming)
- `Rules/assertions.md` — FluentAssertions, key patterns, HTTP response assertions, request/response comparison
- `Rules/test-practices.md` — test isolation, API patterns, non-existent IDs, E2E cleanup, seed methodology, document sync
- `Rules/comments.md` — when comments are needed in code
- `Rules/config.md` — endpoints, configuration
- `Rules/git.md` — remote, commits, secrets
- `Rules/workflow.md` — plan → approval → changes → report
- `Rules/categories.md` — test categories

## Knowledge Graph

```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
graphify query "show all models"
graphify path "ProductModel" "RatingModel"
graphify explain "UserModel"
```
