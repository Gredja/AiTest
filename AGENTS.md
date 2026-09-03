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

Краткие правила:
- **Reference-типы** (string, object, List): без `?`, без инициализации
- **Value-типы** (int, decimal, double, DateTime): проверяем JSON — если поле может быть 0/null, добавляем `?`
- Фреймворк: NUnit (не xUnit)
- Проекты без префикса: `Core`, `Api`, `Ui`

## Структура файлов

```
Gredja/
├── Gredja.slnx
├── README.md
├── AGENTS.md
├── .graphifyignore
├── Rules/
│   ├── models.md
│   └── comments.md
├── Prompts/
│   └── prompts.md
├── graphify-out/
│   ├── graph.json
│   ├── model-dependencies.html
│   └── manifest.json
├── Core/
│   ├── Core.csproj
│   ├── Helpers/
│   │   └── RequestHelper.cs
│   └── Models/
│       ├── Generic/
│       │   └── IdNameModel.cs
│       ├── ProductModel.cs
│       ├── ProductRequest.cs
│       ├── RatingModel.cs
│       ├── CartModel.cs
│       ├── CartRequest.cs
│       ├── CartProductModel.cs
│       ├── UserModel.cs
│       ├── UserRequest.cs
│       ├── AuthRequest.cs
│       ├── UserNameModel.cs
│       ├── AddressModel.cs
│       └── GeolocationModel.cs
├── Api/
│   └── Api.csproj
└── Ui/
    └── Ui.csproj
```

## API для тестирования

FakeStoreAPI — https://fakestoreapi.com
- GET /products — список товаров
- GET /products/{id} — товар по ID
- POST /products — создать товар
- PUT /products/{id} — обновить товар
- DELETE /products/{id} — удалить товар
- GET /carts — корзины
- GET /users — пользователи
- POST /auth/login — аутентификация (JWT)

## Правила работы

1. Все изменения через апрув пользователя
2. После изменения структуры проекта — обновить Graphify
3. Промпты сохранять в `Prompts/prompts.md`
4. Проверять сборку после каждого изменения: `dotnet build Gredja.slnx`
5. **Тесты на неуществующий ID:** 0 и -1 — безопасны (всегда невалидны). Для проверки "not found" — динамически: GET все → maxId → maxId + 1. Никогда статический 999 (может существовать если API вырастет).

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
