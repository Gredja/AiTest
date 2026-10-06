# AGENTS.md — Gredja

AQA-проект. API-тесты (NUnit + RestSharp): FakeStoreAPI, JSONPlaceholder, GitHub API.
E2E тесты (NUnit + RestSharp): GitHub write operations.

## Project Structure

- `Api/` — NUnit API-тесты (FakeStore, JsonPlaceholder, GitHub)
- `E2E/` — NUnit E2E тесты (GitHub write operations)
- `Core/Models/` — модели ответов/запросов
- `Core/Helpers/` — общие хелперы по подпапкам: `Http/` (request-инфраструктура: `RequestHelper`, `FakeStore/JsonPlaceholder/GitHubRequestHelper`), `Data/` (`DataGenerator` — вариативные write-данные), `Assertions/` (`AssertHelper`), `Params/` (`ParamHelper`); нужны E2E → `GitHub/` (`GitHubTestBase`, `GitHubParamHelper`); только для API-тестов → `Api/<Service>/Helpers/`
- `Core/Config/FakeStoreEndpoints.cs`, `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs` — URL и пути эндпоинтов
- `Core/Logging/` — логирование действий (Serilog → `%TEMP%\GredjaTestRun\actions-*.log`) и сырые результаты тестов (`test-results-*.log`). `TestResults/` содержит **только отчёты**, сырые артефакты прогона — во временной папке
- `Core/Reporting/` — `TestRunReportGenerator` → `TestResults/TestRunReport-*.md` (вызывается из NUnit-teardown `TestReportSetup`), age-based очистка артефактов старше 7 дней
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
- Lambda parameters: readable singular noun (`product => product.Id`), не однобуквенные (`p =>`)
- Без аббревиатур (`response`, не `resp`). Boolean: `Is`, `Has`, `Can`, `Should`
- File-scoped namespaces, one class per file, use `var` wherever possible (explicit type only when `var` is not applicable, e.g. `null`, tuples)
- Methods: short, one responsibility, max ~30 lines, max 5 params
- Все API-запросы async (`ExecuteAsync`, не `Execute`)
- Нет модификатора = private. Нет magic numbers. Нет вложенных ternary. `nameof()` для exceptions
- Error handling: конкретные исключения, без `null!`, без exceptions для flow control
- LINQ: `Any()` вместо `Count() > 0`, без лишних `.ToList()`, `FirstOrDefault()` вместо `Where().FirstOrDefault()`
- Strings: интерполяция `$""`, `StringBuilder` в циклах, `IsNullOrEmpty()` вместо `.Length == 0`
- Null safety: `?.` для safe navigation, `??` для fallback, `is not null` вместо `!= null`
- SOLID: один класс — одна задача, зависимости через интерфейсы, расширяемость через наследование

### Models
- Response: suffix `ModelResponse` (включает `Id`). Request: suffix `ModelRequest` (без `Id`). Вложенные/вспомогательные — без суффикса
- Reference types (string, object, List): без `?`, без initializer
- Value types (int, decimal, DateTime): `?` только если JSON-поле может быть null/absent
- Namespace: `Core.Models.{Service}` (например `Core.Models.FakeStore`). Чистые контейнеры данных — без конструкторов, валидации, логики

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
- Seed methodology: 5 seeds → expand → enforce 5+ active negatives (mock-API exception + ceiling rule) → see `Rules/test-practices.md`
- Document sync: Observable Behaviour ↔ Test Plan ↔ Rules — always in sync
- Guarantee Data: GET пуст → POST в OneTimeSetUp → GET снова → Assertion → DELETE в OneTimeTearDown
- Test data для write: только вымышленные значения (E2E пишет в публичный репо), vary ≥2 размерности, обфускация заменой — see `Rules/test-practices.md` → "Test data for write operations"

## Detailed Rules (Rules/)

- `Rules/code.md` — naming, types, file structure, methods, async, access modifiers, cleanup
- `Rules/code-style.md` — error handling, LINQ, strings, null safety
- `Rules/code-principles.md` — SOLID, general principles
- `Rules/models.md` — model building rules (ModelResponse/ModelRequest, properties, naming)
- `Rules/assertions.md` — FluentAssertions, key patterns, HTTP response assertions, request/response comparison
- `Rules/test-practices.md` — test isolation, API patterns, non-existent IDs, E2E cleanup, artifact cleanup, seed methodology, document sync, read-after-write visibility
- `Rules/comments.md` — when comments are needed in code
- `Rules/config.md` — endpoints, configuration
- `Rules/git.md` — remote, commits, secrets
- `Rules/workflow.md` — plan → approval → changes → report
- `Rules/categories.md` — test categories

## AI Onboarding (для новичка)

Чтобы начать работать с AI в этом проекте без объяснений:

1. Прочитай `AGENTS.md` целиком и `Rules/test-practices.md` — там базовые правила и seed-методология
2. `/api-test-gen <service> <endpoint>` — сгенерировать тесты для нового эндпоинта (скилл сам прочитает Observable Behaviour и правила)
3. `/test` — запустить весь тест-сьют (пишет `TestResults/TestRunReport-*.md`), `/test-report` — запуск + Allure-отчёт
4. `/review-commit` — проверить незакоммиченные изменения по правилам проекта
5. `/commit` — форматирование + HealthCheck + коммит + пуш

Полный список скиллов: `.mimocode/skills/`. Все изменения следуют циклу: план → одобрение → изменения → отчёт (`Rules/workflow.md`).

## Knowledge Graph

```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
graphify query "show all models"
graphify path "ProductModelResponse" "Rating"
graphify explain "UserModelResponse"
```
