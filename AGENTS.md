# AGENTS.md — Gredja

AQA-проект. API-тесты (NUnit + RestSharp) и UI-тесты (Playwright). FakeStoreAPI, 20 товаров (IDs 1-20).

## Project Structure

- `Api/Tests/` — NUnit API-тесты
- `Ui/` — Playwright UI-тесты
- `Core/Models/` — модели ответов/запросов
- `Core/Config/Endpoints.cs` — URL и пути эндпоинтов
- `Prompts/` — шаблоны промптов
- `Rules/` — полные правила (здесь — краткая сводка)

File structure: see `FILE_STRUCTURE.md`.

## Skills

- `/test` — запуск тестов + Allure отчёт
- `/commit` — коммит с safety checks + push
- `/pr` — создание PR с safety checks
- `/api-test-gen` — генерация API тестов для FakeStoreAPI
- `/gredja-rules` — все правила проекта

## Rules (quick reference)

Detailed rule files in `Rules/` — read the relevant file before working on the corresponding entity:

- `Rules/models.md` — model building rules (Model/Request, properties, naming)
- `Rules/comments.md` — when comments are needed in code
- `Rules/assertions.md` — FluentAssertions, key patterns
- `Rules/code.md` — general code writing rules (naming, types, file structure)
- `Rules/git.md` — remote, commits, secrets
- `Rules/workflow.md` — plan → approval → changes → report
- `Rules/config.md` — endpoints, configuration

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
- Исключения: regex-объяснения, non-obvious WHY

### Config
- Base URL и эндпоинты в `Core/Config/Endpoints.cs`. Никогда не хардкодить в тестах.

### Git
- Repo: github.com/Gredja/AiTest.git, branch: `main`
- Изменения в `features/<topic>` ветках
- Коммиты: только по запросу, английский, формат: action + object
- Никогда не коммитить `.env` или токены
- **Перед коммитом:** `dotnet format --verify-no-changes` + `dotnet test` — оба должны пройти

### Workflow
Все изменения: план → одобрение → отчёт. После коммита — review `Rules/`. После структурных изменений — обновить документацию.
- После завершения plan mode — удалять `.mimocode/plans/*.md` (не коммитить планы)

## Key Decisions
- 0 и -1 — безопасные static IDs (всегда невалидные)
- Dynamic non-existent ID = maxId + 1 (не статический 999)
- FakeStoreAPI: ровно 20 товаров (IDs 1-20)

## Knowledge Graph

Project knowledge graph: `graphify-out/graph.json`
Visualization: `graphify-out/model-dependencies.html`

Update the graph:
```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
```

## Session Start Hook (mandatory)

**Before doing ANYTHING else at the start of every session, read BOTH memory files:**
1. `C:\Users\User\.local\share\mimocode\memory\projects\global\MEMORY.md`
2. `C:\Users\User\.local\share\mimocode\memory\projects\gredja\MEMORY.md`

This is not optional. Even if the user's first message seems unrelated — read memory first, then respond.

## Memory Navigation

To access saved data:

- `memory({ operation: "search", query: "<keyword>" })` — search project memory
- `Read(file_path="<path>")` — read a specific file
- `task({ operation: "list" })` — list active tasks
- `actor({ operation: "status", actor_id: "<id>" })` — agent status

**Read project memory first:**
- `C:\Users\User\.local\share\mimocode\memory\projects\global\MEMORY.md` — global rules, conventions, architecture decisions
- `C:\Users\User\.local\share\mimocode\memory\projects\gredja\MEMORY.md` — Gredja-specific context, rules, roadmap, curriculum progress

## Memory Structure

```
~/.local/share/mimocode/memory/
├── projects/global/MEMORY.md    — rules, conventions, architecture decisions
├── projects/gredja/MEMORY.md    — project-specific context, rules, roadmap, curriculum progress
├── sessions/<sid>/checkpoint.md — current session state
└── sessions/<sid>/notes.md      — notes and observations
```

## Graphify Commands

```bash
graphify query "show all models"           # query the graph
graphify path "ProductModel" "RatingModel" # path between nodes
graphify explain "UserModel"               # explain a node
```
