# Gredja

Тестовое приложение для изучения AI-Native SDLC. Проект для автоматизации тестирования API (FakeStoreAPI) и UI (Playwright).

## Структура решения

```
Gredja/
├── Gredja.slnx
├── README.md
├── AGENTS.md
├── .graphifyignore
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

## Технологии

- .NET 10.0
- NUnit 4.6.1
- RestSharp 114.0.0 (API-тесты)
- Microsoft.Playwright 1.62.0 (UI-тесты)
- Graphify 0.9.53 (knowledge graph)

## API под тестирование

FakeStoreAPI — https://fakestoreapi.com

Ресурсы:
- Products — товары (GET, POST, PUT, DELETE)
- Carts — корзины
- Users — пользователи
- Auth — аутентификация (JWT)

## Конвенции

- Классы моделей: суффикс `Model` (response), `Request` (request)
- Один класс = один файл
- Свойства без инициализаций, nullable типы
- Фреймворк тестов: NUnit
- Проекты без префикса: `Core`, `Api`, `Ui`

## Запуск тестов

```bash
dotnet test Api/Api.csproj
dotnet test Ui/Ui.csproj
```

## Knowledge Graph

Graphify построен в режиме `--code-only` (без LLM):

```bash
# Обновить граф после изменений
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force

# Запросы к графу
graphify query "show all models"
graphify path "ProductModel" "RatingModel"
graphify explain "UserModel"
```

Визуализация зависимостей моделей: `graphify-out/model-dependencies.html`

## Сроки

Дедлайн: 31.12.2026
