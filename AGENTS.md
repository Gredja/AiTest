# AGENTS.md — Gredja

AQA-проект. API-тесты (NUnit + RestSharp): JSONPlaceholder, GitHub API.
E2E тесты (NUnit + RestSharp): GitHub сценарные цепочки (multi-step write flows).

## Project Structure

- `Api/` — NUnit API-тесты (JsonPlaceholder, GitHub), включая одиночные write-тесты
- `Ui/` — NUnit UI-тесты (Playwright .NET + Chromium): сайт-скоуп — папка `Ui/github/` (будущие сайты — соседние папки, напр. `Ui/moodledemo/`); pages/forms — `Ui/github/Pages/` (общий хром — `Ui/github/Pages/Common/`, семейственные базовые классы — `RepositoryPageBase`), инфраструктура — `Ui/github/Tests/Support/`, тесты — `Ui/github/Tests/`; авторизация через storageState (`.auth/github-ui-state.json`): state обновляет автотест `LoginSignInTests` (логин по `GITHUB_UI_EMAIL`/`GITHUB_UI_PASSWORD`), остальные тесты state только читают; write через UI → cleanup через API; локаторы ТОЛЬКО на Page/Form (внизу файла), хелперы получают `ILocator` параметрами; правила — `Rules/ui-testing.md`
- `E2E/` — NUnit E2E тесты: ТОЛЬКО сценарные цепочки (create → verify → mutate → cleanup)
- `Core/Models/` — модели ответов/запросов
- `Core/Helpers/` — общие хелперы по подпапкам: `Http/` (request-инфраструктура: `RequestHelper`, `JsonPlaceholder/GitHubRequestHelper`), `Data/` (`DataGenerator` — вариативные write-данные), `Assertions/` (`AssertHelper`), `Params/` (`ParamHelper`); нужны E2E → `GitHub/` (`GitHubTestBase`, `GitHubParamHelper`); только для API-тестов → `Api/<Service>/Helpers/`
- `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs` — URL и пути эндпоинтов
- `Core/Logging/` — логирование действий (Serilog → `%TEMP%\GredjaTestRun\actions-*.log`) и сырые результаты тестов (`test-results-*.log`). `TestResults/` содержит **только отчёты**, сырые артефакты прогона — во временной папке
- `Core/Reporting/` — `TestRunReportGenerator` → `TestResults/TestRunReport-*.md` (вызывается из NUnit-teardown `TestReportSetup`), age-based очистка артефактов старше 7 дней
- `Rules/` — полные правила (здесь — краткая сводка)
- `documentation/` — документация проекта; `documentation/Katas/` — обучение, не трогать
- `documentation/*ObservableBehaviour.md` — наблюдаемое поведение API (актуальные состояния, типы полей, негативные кейсы). **Читать при генерации и review тестов.**
- `education/TESTING_PLAYBOOK.md` — переносимая project-agnostic методология тестирования (EN, скармливается другим агентам/проектам); обновлять при каждом коммите (скилл `/commit`, Step 6)
- `.mimocode/skills/` — скиллы для AI-агентов
- `.github/workflows/` — CI: `pr-checks.yml` (required PR-gate: format всегда, HealthCheck для same-repo веток) и `nightly-tests.yml` (ночной informational-прогон); секреты — только в настройках репо, live-сьют никогда не required-check (`Rules/git.md` → CI)

## Rules

### Workflow
- **Session start:** right after the greeting, before any other work — run the `/nightly-check` skill (one-line verdict on the `Gredja/AiTest` nightly run). Never skip it, even for a "quick question" — verdict first, then the user's work (duplicate of project MEMORY → Session start; this is the canonical text present in every session context).
- **Documentation language (hard rule, 2026-10-19):** repository-facing documents — **English only** (`Rules/*`, `education/TESTING_PLAYBOOK.md`, `documentation/*` including OB and templates, `.mimocode/skills/*/SKILL.md`, `README.md`, `AGENTS.md` itself). User-facing chat replies — **Russian**. Russian text inside English files is allowed only as quoted user trigger phrases or rule citations (e.g. skill description triggers), never as the main body. Mixed language without a quoted-source reason = a review violation.
Все изменения: план → одобрение → отчёт. После коммита — review `Rules/`. После структурных изменений — обновить документацию И tracked `backups/` (включая удаления gitignored-файлов — в том же ходу, даже без коммита). Выполненные планы (`.mimocode/plans/`) — удалять сразу после реализации. Правило, выраженное как IDE0xxx → `.editorconfig` в том же изменении; новая формулировка правила сверяется с существующими `Rules/*.md` на противоречие.

### Code
- Naming: PascalCase (classes, methods, properties, constants), camelCase (locals, params), `_camelCase` (private fields)
- Перед созданием любого файла — решать его место по здравому смыслу и планам на будущее (`Rules/code.md` → File Layout)
- Lambda parameters: readable singular noun (`product => product.Id`), не однобуквенные (`p =>`)
- Без аббревиатур (`response`, не `resp`). Boolean: `Is`, `Has`, `Can`, `Should`
- File-scoped namespaces, one class per file, use `var` wherever possible (explicit type only when `var` is not applicable, e.g. `null`, tuples)
- Без мёртвых `using` — удалять в том же ходу, что и код (ловится `dotnet format` через `.editorconfig` IDE0005)
- Methods: short, one responsibility, max ~30 lines (для тестовых методов допустимо длиннее, если разбиение скрывает сценарий — `Rules/code.md`), max 5 params
- Все API-запросы async (`ExecuteAsync`, не `Execute`)
- Нет модификатора = private. Нет magic numbers. Нет вложенных ternary. `nameof()` для exceptions
- Error handling: конкретные исключения, без `null!`, без exceptions для flow control
- LINQ: `Any()` вместо `Count() > 0`, без лишних `.ToList()`, `FirstOrDefault()` вместо `Where().FirstOrDefault()`; синхронные side-effect циклы на `List<T>` → `.ForEach(item => ...)` (async/ленивые источники → `foreach`, `Rules/code-style.md` → Loops)
- Strings: интерполяция `$""`, `StringBuilder` в циклах, `IsNullOrEmpty()` вместо `.Length == 0`
- Null safety: `?.` для safe navigation, `??` для fallback, `is not null` вместо `!= null`
- SOLID: один класс — одна задача, зависимости через интерфейсы, расширяемость через наследование
- Тестовые классы (`*Tests.cs`): только `[Test]` + lifecycle + тестовые данные; общие билдеры параметров — в base/helpers, не в тестовом классе; cleanup — только в `[OneTimeTearDown]` (**последним членом класса**) через fixture-реестр (register-then-assert), не в `finally`; **создал → удалил, тестовые записи не храним** (нет delete API → ближайшая уборка: close issue/PR) (`Rules/test-practices.md`)

### Models
- Response: suffix `ModelResponse` (включает `Id`). Request: suffix `ModelRequest` (без `Id`). Вложенные/вспомогательные — без суффикса
- Reference types (string, object, List): без `?`, без initializer
- Value types (int, decimal, DateTime): `?` только если JSON-поле может быть null/absent
- Namespace: `Core.Models.{Service}` (например `Core.Models.GitHub`). Чистые контейнеры данных — без конструкторов, валидации, логики
- JSON-имена: только константы `JsonFields` своего домена (`Core/Models/{Service}/JsonFields.cs`) — не инлайн-литералы, не копии в тестовых хелперах

### Assertions
- FluentAssertions (не NUnit Assert)
- Ключевые: `.Should().Be()`, `.NotBeNull()`, `.NotBeNullOrWhiteSpace()`, `.BeGreaterThan()`, `.BeInRange()`, `.OnlyContain()`
- Helper-методы: `ShouldHaveStatusCode()`, `ShouldHaveError()`, `ShouldHaveValidContract()`, `ShouldHaveValidFields()`, `ShouldMatchRequest()` — в `AssertHelper.cs`

### Comments
- По умолчанию: без комментариев. Код говорит сам за себя.
- Комментарий — крайняя мера: сначала рефакторинг ради ясности; комментарий только если код уже нельзя отрефакторить и он всё ещё непонятен (`Rules/comments.md`).
- Исключения: regex-объяснения, TODO (только в dev, удалить до merge), non-obvious WHY

### Config
- Base URL и эндпоинты в `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs`. Никогда не хардкодить в тестах.
- `testsettings.example.json` синхронен с `testsettings.json`; токен: JSON `Token` → env `GITHUB_PAT` → `.env` (`Rules/config.md`); два GitHub-аккаунта не смешивать: API/E2E = `Gredja`, UI = `gredjapl` (`Rules/config.md` → GitHub identities)

### Git
- Repo: github.com/Gredja/AiTest.git, branch: `main`
- Изменения в `features/<topic>` ветках
- Коммиты: ТОЛЬКО через скилл `/commit` (никогда вручную); только по запросу, английский, формат: action + object
- Никогда не коммитить `.env` или токены
- **Перед коммитом:** `dotnet format --verify-no-changes` + `dotnet test --filter Category=HealthCheck` — оба должны пройти (полный прогон — `/test`)
- CI: PR-gate (`format` + `HealthCheck`) и ночной informational-прогон; live-сьют не ставить в required checks (`Rules/git.md` → CI)

## Key Decisions
- 0 и -1 — безопасные static IDs (всегда невалидные)
- Dynamic non-existent ID = maxId + 1 (не статический 999); при concurrent-записи в том же ране (Api + E2E solution-ран) — maxId + Offset (`GitHubEndpoints.NonExistentIdOffset = 100`)
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
- `Rules/test-practices.md` — test isolation, API patterns, non-existent IDs, E2E cleanup, artifact cleanup, seed methodology, document sync (incl. single-source guidance), read-after-write visibility
- `Rules/comments.md` — when comments are needed in code
- `Rules/config.md` — endpoints, configuration
- `Rules/git.md` — remote, commits, secrets, repo hygiene, CI
- `Rules/workflow.md` — plan → approval → changes → report
- `Rules/categories.md` — test categories
- `Rules/ui-testing.md` — UI-тесты (Playwright): структура page-objects, локаторы, auth/storageState, waits (ожидание исхода действия до assert), cleanup, категории

## AI Onboarding (для новичка)

Чтобы начать работать с AI в этом проекте без объяснений:

1. Прочитай `AGENTS.md` целиком и `Rules/test-practices.md` — там базовые правила и seed-методология
2. `/api-test-gen <service> <endpoint>` — сгенерировать тесты для нового эндпоинта (скилл сам прочитает Observable Behaviour и правила)
3. `node Scripts/locator-dump.mjs <url> [--waitfor "текст"]` — одноразовый дамп role + accessible name со страницы для UI-локаторов (CDP, без зависимостей; печатьки из `.auth/github-ui-state.json` по умолчанию, `--anonymous` — без входа)
4. `/test` — запустить весь тест-сьют (пишет `TestResults/TestRunReport-*.md`), `/test-report` — запуск + Allure-отчёт
5. `/review-commit` — проверить незакоммиченные изменения по правилам проекта
6. `/commit` — форматирование + HealthCheck + коммит + пуш

Полный список скиллов: `.mimocode/skills/`. Все изменения следуют циклу: план → одобрение → изменения → отчёт (`Rules/workflow.md`).

## Knowledge Graph

```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
graphify query "show all models"
graphify path "PostModelResponse" "User"
graphify explain "UserModelResponse"
```
