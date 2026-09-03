# AGENTS.md — Gredja

Инструкции для AI-агентов, работающих с проектом Gredja.

## Проект

Gredja — .NET 10.0 решение для автоматизации тестирования. Три проекта:
- **Core** — общая логика (модели, хелперы, конфигурация)
- **Api** — API-тесты против FakeStoreAPI (NUnit + RestSharp + FluentAssertions)
- **Ui** — UI-тесты через Playwright (NUnit + Microsoft.Playwright)

## Правила

Правила проекта хранятся в `Rules/` — каждое правило в отдельном файле markdown.
При работе с сущностью обращаться к соответствующему файлу правил.

- `Rules/models.md` — правила построения моделей (Model/Request, свойства, именование)
- `Rules/comments.md` — когда нужны комментарии в коде
- `Rules/assertions.md` — FluentAssertions, ключевые паттерны
- `Rules/git.md` — remote, коммиты, секреты
- `Rules/workflow.md` — план → апрув → изменения → отчёт
- `Rules/config.md` — endpoints, конфигурация

## Структура файлов

```
Gredja/
├── Gredja.slnx
├── AGENTS.md
├── Rules/
├── Prompts/
├── Core/
│   ├── Config/
│   │   └── Endpoints.cs
│   ├── Helpers/
│   └── Models/
├── Api/
│   └── Tests/
└── Ui/
```

## Knowledge Graph

Граф знаний проекта: `graphify-out/graph.json`
Визуализация: `graphify-out/model-dependencies.html`

Обновление графа:
```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
```

## Навигация по памяти

Для доступа к сохранённым данным используй:

- `memory({ operation: "search", query: "<ключевое слово>" })` — поиск по памяти проекта
- `Read(file_path="<путь>")` — чтение конкретного файла
- `task({ operation: "list" })` — список активных задач
- `actor({ operation: "status", actor_id: "<id>" })` — статус агента

## Структура памяти

```
~/.local/share/mimocode/memory/
├── projects/global/MEMORY.md    — правила, конвенции, архитектурные решения
├── sessions/<sid>/checkpoint.md — текущее состояние сессии
└── sessions/<sid>/notes.md      — заметки и наблюдения
```

## Команды Graphify

```bash
graphify query "show all models"           # запрос к графу
graphify path "ProductModel" "RatingModel" # путь между нодами
graphify explain "UserModel"               # объяснить ноду
```
