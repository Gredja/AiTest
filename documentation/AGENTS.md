# AGENTS.md — Gredja

AQA-проект. API-тесты (NUnit + RestSharp) и UI-тесты (Playwright).
FakeStoreAPI, 20 товаров (IDs 1-20).

## Project Structure

- `Api/Tests/` — NUnit API-тесты
- `Ui/` — Playwright UI-тесты
- `Core/Models/` — модели ответов/запросов
- `Core/Config/Endpoints.cs` — URL и пути эндпоинтов
- `Prompts/` — шаблоны промптов
- `Rules/` — полные правила (здесь — краткая сводка)
- `documentation/` — документация проекта (Katas, TestPlan, FILE_STRUCTURE.md)
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

### Models
- Response: suffix `Model` (включает `Id`). Request: suffix `Request` (без `Id`)
- Reference types (string, object, List): без `?`, без initializer
- Value types (int, decimal, DateTime): `?` только если JSON-поле может быть null/absent
- Namespace: `Core.Models`. Чистые контейнеры данных — без конструкторов, валидации, логики

### Assertions
- FluentAssertions (не NUnit Assert)
- Ключевые: `.Should().Be()`, `.NotBeNull()`, `.NotBeNullOrWhiteSpace()`, `.BeGreaterThan()`, `.BeInRange()`, `.OnlyContain()`

### Comments
- По умолчанию: без комментариев. Код говорит сам за себя.
- Исключения: regex-объяснения, TODO (только в dev, удалить до merge), non-obvious WHY

### Config
- Base URL и эндпоинты в `Core/Config/Endpoints.cs`. Никогда не хардкодить в тестах.

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

## Detailed Rules (Rules/)

- `Rules/models.md` — model building rules (Model/Request, properties, naming)
- `Rules/comments.md` — when comments are needed in code
- `Rules/assertions.md` — FluentAssertions, key patterns
- `Rules/code.md` — general code writing rules (naming, types, file structure)
- `Rules/git.md` — remote, commits, secrets
- `Rules/workflow.md` — plan → approval → changes → report
- `Rules/config.md` — endpoints, configuration

## Knowledge Graph

```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
graphify query "show all models"
graphify path "ProductModel" "RatingModel"
graphify explain "UserModel"
```
