# Gredja: полный гайд для новичка

Этот документ объясняет с нуля: что такое MiMoCode, зачем он нужен, как поставить и как работать с проектом Gredja.

---

## Часть 1. Что такое MiMoCode

MiMoCode — это ИИ-ассистент, который работает в терминале (командной строке). Он помогает писать код, запускать тесты, делать ревью, коммитить — всё через текстовый диалог.

**Как это выглядит:**
- Ты открываешь терминал, запускаешь `mimo`
- Пишешь ему сообщения на русском или английском
- Он выполняет задачи: читает файлы, пишет код, запускает команды, делает git-операции

**Ключевое:**
- MiMoCode — это НЕ редактор кода. Это агент, который работает с твоим кодом через терминал.
- У него есть доступ к файлам проекта, git, dotnet, и другим инструментам.
- Он может запускать команды, читать/писать файлы, искать по кодовой базе.
- Он работает по модели "ты просишь — он делает". Ты не пишешь код сам, а описываешь что нужно.

---

## Часть 2. Установка

### 2.1. Установи MiMoCode

Открой терминал (CMD, PowerShell или другой) и выполни:

```
npm install -g mimocode
```

Если `npm` не установлен — сначала поставь Node.js: https://nodejs.org/

Проверь что установилось:

```
mimo --version
```

Должна показаться версия (например `0.1.14`).

### 2.2. Установи Required Tools

Для проекта Gredja нужны:

| Инструмент | Зачем | Как установить |
|------------|-------|----------------|
| .NET 10.0 SDK | Компиляция и запуск тестов | https://dotnet.microsoft.com/download |
| Java 17+ | Нужен для Allure-отчётов | `winget install Microsoft.OpenJDK.17` |
| Allure CLI | Генерация отчётов по тестам | `winget install AllureCommandLine` |

Проверь (в CMD или PowerShell — любая оболочка):

```
dotnet --version    # должно показать 10.x
java --version      # должно показать 17+
allure --version    # должно показать 2.x
```

### 2.3. Полезные инструменты (рекомендовано)

Помимо обязательных, есть инструменты, которые сильно упростят работу:

**Visual Studio (рекомендуется для .NET)**

Бесплатная Community-версия: https://visualstudio.microsoft.com/ru/vs/community/

Зачем:
- Автодополнение кода, переход к определениям, рефакторинг
- Отладчик — можно ставить breakpoint'ы и смотреть значения переменных
- Intellisense — подсказки по типам, параметрам, документации
- Встроенный git — ветки, коммиты, diff прямо в IDE
- Обзор решений — видна вся структура проекта

При установке выбери рабочую нагрузку ".NET-разработка".

**Visual Studio Code (альтернатива)**

Легковесный редактор: https://code.visualstudio.com/

Полезные расширения:
- C# Dev Kit — автодополнение и отладка для .NET
- GitLens — расширенная работа с git
- Error Lens — ошибки прямо в строках кода

**MiMo Desktop (ИИ-помощник на рабочем столе)**

MiMo Desktop — это настольное приложение от Xiaomi с ИИ-помощником. Работает как "базовый" для MiMoCode: показывает статус, быстрые команды, уведомления.

Как получить:
1. Зайди на https://mimocode.mi.com
2. Подай заявку на бета-тест
3. После одобрения получи доступ к скачиванию

Не обязателен для работы, но удобен как дополнение к `mimo` в терминале.

**Git**

Если ещё не установлен: https://git-scm.com/download/win

При установке выбери "Git from the command line and also from 3rd-party software" — это добавит `git` в PATH для CMD и PowerShell.

---

## Часть 3. Конфигурация MiMoCode

### 3.1. Глобальный конфиг

MiMoCode хранит настройки в файле `~/.config/mimocode/mimocode.jsonc`.

**Путь на Windows:** `C:\Users\<твоё_имя>\.config\mimocode\mimocode.jsonc`

Создай этот файл со следующим содержимым:

```jsonc
{
  "$schema": "https://mimo.xiaomi.com/mimocode/config.json",
  "model": "mimo/mimo-auto",
  "permission": {
    "bash": {
      "*": "ask",
      "curl.exe *": "allow",
      "explorer.exe *": "allow",
      "Get-ChildItem *": "allow",
      "mkdir *": "allow",
      "copy *": "allow",
      "del *": "allow",
      "ren *": "allow",
      "dir *": "allow",
      "git add *": "allow",
      "git commit *": "allow",
      "git push *": "allow",
      "git status": "allow",
      "git diff *": "allow",
      "git log *": "allow",
      "dotnet test *": "allow",
      "dotnet build *": "allow",
      "dotnet format *": "allow",
      "Remove-Item *": "allow",
      "./Scripts/*": "allow",
      "allure *": "allow",
      "powershell *": "allow"
    }
  }
}
```

**Зачем это нужно?** MiMoCode по умолчанию спрашивает разрешение перед каждой командой. Это безопасно, но неудобно — агент будет спрашивать "можно ли выполнить `dotnet test`?" после каждого шага. Секция `permission` говорит: "эти команды можно выполнять без вопросов". Без этого субагенты (внутренние помощники MiMoCode) не смогут запускать тесты и генерировать отчёты.

### 3.2. Проектный конфиг

В папке проекта уже есть файл `.mimocode/mimocode.jsonc`. Он минимальный — просто указывает где лежат скиллы. Менять его не нужно.

---

## Часть 4. Переменные окружения

В корне проекта создай файл `.env` (НЕ коммитить, NEVER):

```
GITHUB_PAT=твой_личный_токен_здесь
```

**Как получить токен:**
1. Зайди на GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Нажми "Generate new token"
3. Дай имя (например `gredja-dev`)
4. Выбери scopes: `repo` (полный доступ к репозиториям)
5. Скопируй токен и вставь в `.env`

Токен нужен для тестов GitHub API.

**Файл конфигурации тестов:** `testsettings.json` в корне (в `.gitignore`, в клоне его нет).
Без него сборка и тесты работают: `Core.csproj` подставляет трекаемый `testsettings.example.json`.
Если нужны свои значения — скопируй:

```
copy testsettings.example.json testsettings.json
```

**Порядок чтения токена** (`TestConfig.GitHubToken`, проверяй по коду, не по памяти):

1. `GitHub.Token` из `testsettings.json`
2. переменная окружения `GITHUB_PAT` (вариант для CI)
3. строка `GITHUB_PAT=...` в `.env` (поиск вверх по дереву от каталога бинарников)

---

## Часть 5. Запуск

### 5.1. Клонируй проект

```
git clone https://github.com/Gredja/AiTest.git
cd AiTest
```

### 5.2. Проверь что всё работает

```
dotnet build                       # должна компилироваться без ошибок
dotnet test --verbosity minimal    # штатный запуск всех тестов: пишет TestRunReport-*.md в TestResults/
```

Отчёт — таблица по каждому тесту + итог (количество и проценты), создаётся автоматически после любого `dotnet test`. Фильтр: `dotnet test --verbosity minimal --filter "Category=HealthCheck"`.

### 5.3. Запусти MiMoCode

```
mimo
```

Откроется интерфейс MiMoCode. Теперь ты можешь общаться с ИИ-агентом.

---

## Часть 6. Структура проекта Gredja

Прежде чем начинать работать, полезно понять что где лежит.

```
Gredja/
├── README.md                     # Обзор проекта (GitHub рендерит как главную страницу)
├── AGENTS.md                     # Описание проекта для ИИ-агента
├── Core/
│   ├── Config/                   # URL и пути к API endpoint'ам
│   │   ├── JsonPlaceholderEndpoints.cs
│   │   └── GitHubEndpoints.cs
│   ├── Helpers/                  # Тестовая инфраструктура (всегда здесь, даже для E2E)
│   │   ├── Http/                 # RequestHelper (Get/Post/Put/Patch/Delete) + JsonPlaceholder/GitHubRequestHelper
│   │   ├── Params/               # Общие IdParam/UrlSegment/Query
│   │   ├── Assertions/           # ShouldHaveStatusCode и др.
│   │   ├── Data/                 # DataGenerator: RandomString/RandomIntExclusive для write-данных
│   │   ├── Waiting/              # WaitHelper: асинхронное ожидание условия (polling)
│   │   └── GitHub/               # GitHubTestBase + GitHubParamHelper
│   ├── Logging/                   # Лог действий (Serilog → %TEMP%\GredjaTestRun) + test-results-*.log
│   ├── Reporting/                 # Генератор TestRunReport-*.md (teardown)
│   └── Models/                   # Модели данных (Response/Request)
│       ├── JsonPlaceholder/      # Модели JSONPlaceholder
│       ├── GitHub/               # Модели GitHub API
│       └── Generic/              # Общие модели
├── Api/
│   ├── JsonPlaceholder/Tests/    # Тесты JSONPlaceholder
│   │   ├── Albums/               # Тесты альбомов
│   │   ├── Comments/             # Тесты комментариев
│   │   ├── Photos/               # Тесты фотографий
│   │   ├── Posts/                # Тесты постов
│   │   ├── Todos/                # Тесты задач
│   │   └── Users/                # Тесты пользователей
│   └── GitHub/Tests/             # Тесты GitHub API (Auth/, Issues/, Repos/ и др.)
├── E2E/                          # E2E тесты (цепочки связей)
├── Ui/                           # UI-тесты (Playwright + Chromium, storageState)
├── AllureAdapter/                  # Allure-адаптер
├── Scripts/                      # Скрипты (allure-report.ps1, test-coverage.ps1)
├── Rules/                        # Правила кодирования
├── Gredja.slnx                   # Решение — все dotnet-команды идут через него
├── metrics.md                    # Метрики AI-использования (tests/time/cost)
├── documentation/                # Документация
│   ├── FILE_STRUCTURE.md         # Точное дерево всех файлов проекта
│   ├── GeneralPlan.md            # Постоянный план хотелок пользователя (не удаляется)
│   ├── Bugs/                     # Баг-репорты
│   │   └── JsonPlaceholder/      # Баги JsonPlaceholder
│   ├── GitHubTestingStructure.md # Структура тестирования
│   ├── GitHubObservableBehaviour.md # Наблюдаемое поведение API
│   ├── JsonPlaceholderObservableBehaviour.md # Наблюдаемое поведение JP API
│   ├── JsonPlaceholderTestPlan.md # План тестирования JP API
│   ├── ObservableBehaviourTemplate.md # Шаблон для новых сервисов
│   └── README.md                 # Заглушка → ссылка на корневой README.md
└── .mimocode/
    ├── skills/                  # Скиллы для MiMoCode
    └── plans/                   # Активные планы работ (github-full-coverage.md и др.)
```

**Проект тестирует два REST API:**
- **JSONPlaceholder** — фейковый REST API (посты, комментарии, пользователи)
- **GitHub API** — реальный GitHub (репозитории, Issues, PR, Branches)

---

## Часть 7. Как работать с MiMoCode

### 7.1. Основы общения

Просто пиши сообщения на русском или английском. Примеры:

- "Запусти все тесты"
- "Напиши тест для endpoint GET /users/{id}"
- "Сделай ревью моих изменений"
- "Закоммить изменения"
- "Что не так с этим тестом?"

MiMoCode:
- Читает файлы проекта
- Пишет и редактирует код
- Запускает команды (dotnet, git, и т.д.)
- Генерирует отчёты

### 7.2. Слэш-команды (скиллы)

В MiMoCode есть команды, которые начинаются с `/`. Это "скиллы" — готовые инструкции для частых задач:

| Команда | Что делает |
|---------|-----------|
| `/test` | Запускает все тесты + пишет TestRunReport-*.md в `TestResults/` |
| `/test-report` | Запускает тесты + генерирует Allure-отчёт |
| `/commit` | Форматирование + ревью + тесты + синк бэкапов + коммит + пуш |
| `/review-commit` | Ревью не закоммиченных изменений |
| `/review-pr` | Ревью pull request |
| `/api-test-gen` | Генерирует тесты для нового API endpoint (читает Observable Behaviour) |
| `/e2e-test-gen` | Генерирует E2E тесты с Setup/Teardown для write operations |
| `/ui-test-gen` | Генерирует UI-тесты Playwright (page-objects, live-снапшоты локаторов) |
| `/coverage` | Запускает тесты + считает code coverage (coverlet) + file coverage (endpoint→test) |
| `/update-docs` | Обновляет .md файлы проекта после структурных изменений |
| `/gredja-rules` | Показывает правила проекта |

Просто напиши `/test` в чате с MiMoCode — и он запустит тесты.

### 7.3. Важные правила поведения

1. **MiMoCode НЕ коммитит сам** — только по твоей просьбе. Напиши "закоммить" или `/commit`.
2. **MiMoCode показывает план перед работой** — для каждой задачи он предложит план, подождёт одобрения, потом выполнит.
3. **Работа в ветках** — все изменения делаются в ветках `features/<topic>`, не в `main`.
4. **Файлы с `.env` секретами** — никогда не коммить файл `.env`.
5. **Observable Behaviour — source of truth** — при генерации тестов MiMoCode читает `{Service}ObservableBehaviour.md` и генерирует тесты по описанным полям и типам.
6. **Document sync** — при изменении эндпоинтов обновляй Observable Behaviour ↔ Test Plan ↔ Rules. Каждый Top-3 риск плана покрывается ≥1 тест-кейсом (risk-coverage check).

---

## Часть 8. Git workflow

### 8.1. Создание ветки

Все изменения делаются в отдельных ветках. Название — `features/<topic>`:

```
git checkout -b features/add-user-tests
```

### 8.2. Изменения и коммиты

1. Вноси изменения (сам или через MiMoCode)
2. Напиши `/commit` — MiMoCode проверит формат, сделает ревью, запустит тесты, закоммитит и запушит

### 8.3. Pull Request

Когда ветка готова:
1. Зайди на https://github.com/Gredja/AiTest
2. Нажми "Compare & pull request"
3. Опиши что сделал
4. Дождись ревью (или напиши `/review-pr` в MiMoCode)

### 8.4. Важные правила

- Никогда не коммить прямо в `main`
- Перед коммитом всегда проверяй формат и тесты (`/commit` делает это автоматически)
- Коммиты на английском, формат: `action + object` (например "Add product API tests")

---

## Часть 9. Краткий справочник правил

Полные правила в `Rules/`. Вот самое важное:

**Код:**
- PascalCase для классов/методов/свойств, camelCase для локальных переменных
- Без аббревиатур: `response`, не `resp`
- Лямбды: читаемое имя (`product => product.Id`), не однобуквенные (`p =>`)
- Все API-запросы async (`ExecuteAsync`)
- Без мёртвых `using` — удалять сразу вместе с кодом; enforcement: `.editorconfig` → `IDE0005`/`IDE0007` (`var`)/`IDE0161` (file-scoped namespace) = warning, ловится `dotnet format --verify-no-changes`
- Маленькие методы, одно действие, максимум ~30 строк
- Конкретные исключения вместо `Exception`, без `null!`
- LINQ: `Any()` вместо `Count() > 0`, без лишних `.ToList()`; синхронные side-effect циклы на `List<T>` → `.ForEach(item => ...)`, `foreach` — только для async/ленивых источников (Rules/code-style.md → Loops)
- Regex: только когда предикат читается хуже; паттерн — именованный `static readonly` + словесный комментарий над ним; дубли в 2+ файлах → base/хелпер (Rules/code-style.md → Regex)
- UI: role/label-based локаторы только после живого снапшота; write через UI → cleanup через API (Rules/ui-testing.md)
- Строки: интерполяция `$""`, `StringBuilder` в циклах
- SOLID: один класс — одна задача, зависимости через интерфейсы

**Модели:**
- Response модели: суффикс `ModelResponse` (включает `Id`)
- Request модели: суффикс `ModelRequest` (без `Id`)
- Вложенные/вспомогательные модели — без суффикса
- Namespace: `Core.Models.{Service}` (например `Core.Models.GitHub`)
- JSON-имена — только константы `JsonFields` своего домена (`Core/Models/{Service}/JsonFields.cs`): `[JsonPropertyName(JsonFields.X)]`, не инлайн-литералы, не копии в тестовых хелперах
- Чистые контейнеры данных — без логики

**Тесты:**
- 1 endpoint = 1 тестовый класс
- В тестовом классе только тесты: `[Test]`, lifecycle (`OneTimeSetUp`/`TearDown`), тестовые данные (константы/state); общие билдеры параметров — в base/param helpers (дубль в 2+ классах → извлечь в base); cleanup — только в `[OneTimeTearDown]` **последним членом класса** через fixture-реестр (register-then-assert сразу после create, до asserts), не в `finally`; ядро cleanup — общий `RunCleanupAsync` (warning, не failure, NotFound глушится); **создал → удалил, тестовые записи не храним** (нет delete API → ближайшая уборка: close issue/PR; мерж-история неизменяема — мержи идут в одноразовую base-ветку, main не трогается)
- Seed-методология: 5 seeds → expand → enforce 5+ active negatives (mock-API exception + ceiling rule) → ~15-20 тестов на endpoint
- Обязательные поля request body → тест на КАЖДОЕ отсутствующее поле (`*_Missing{Field}_*`); пустой body не заменяет проверку полей
- Негативы считаются по АКТИВНЫМ тестам (`[Ignore]` не в счёт); когда полезные режимы исчерпаны — фиксируй достигнутое, не плоди дубли
- Non-existent ID: динамически `maxId + 1`; если в том же ране есть concurrent-записи (solution-ран Api + E2E) — `maxId + Offset` (`GitHubEndpoints.NonExistentIdOffset = 100`); stateless mock (JP) остаётся на `maxId + 1`; волатильные сущности (имена живых веток) — только dynamic lookup либо запись в Entry Criteria
- Setup/teardown: каждый запрос в `[OneTimeSetUp]` сразу проверяет статус (`ShouldHaveStatusCode`) — иначе падение токена даёт NRE посреди фикстуры; в `[OneTimeTearDown]`/`finally` cleanup статус НЕ проверяется — `try/catch` + warning (cleanup = warning, не failure, и не подмена исходного исключения); перед ЛЮБЫМ разыменованием `.Data` — status-assert (в том числе в телах тестов), `Max`/`First` в setup — только после `NotBeEmpty` с сообщением про Entry Criteria
- Сначала позитивные тесты, потом негативные
- FluentAssertions (не NUnit Assert)
- Helper-методы: `ShouldHaveStatusCode()`, `ShouldHaveError()` (статус + читаемый message + совпадение с документированным в OB), `ShouldHaveValidContract()`, `ShouldHaveValidFields()`, `ShouldMatchRequest()` — эти три собирают все нарушения в один failure (`AssertionScope`); прямые `x.Should()` в тестах — fail-fast
- Категории: тип сервиса (JsonPlaceholder/GitHub/GitHubE2E/GitHubUi) + тип проверки (HealthCheck/ContractCheck/Smoke/Regression/Negative/Performance)
- Тесты независимы друг от друга, Given/When/Then структура
- Проверяй HTTP status и body отдельно
- После POST/PATCH сравнивай request ↔ response через `ShouldMatchRequest()` (внутри хелпера: скаляр/строка → `Be`, коллекции и вложенные объекты → структурное сравнение)
- Guarantee Data: GET пуст → POST в OneTimeSetUp → GET снова → Assertion → DELETE в OneTimeTearDown — cleanup = удаление, ЕСЛИ API позволяет; нет endpoint удаления → soft-close (GitHub issues: PATCH `state=closed`, DELETE → 404)
- Read-after-write visibility: baseline GET → POST в OneTimeSetUp → GET снова → assert «+1» и ShouldMatchRequest всех полей → DELETE в OneTimeTearDown (ошибка cleanup = warning, не failure); КАЖДЫЙ POST-flow включает проверку скорости добавления записи — `*_RecordVisibleWithinTimeLimit` (Performance): `WaitHelper.WaitUntilAsync` с timeout `MaxResponseTimeMs`, ассерт `IsSuccess` + диагностика через `Elapsed`/`Attempts`/`LastValue` (mock-API без persistence — тест с `[Ignore]`)
- Ожидание условий (API с задержкой видимости): `WaitHelper.WaitUntilAsync(action, condition, timeout?, interval?)` → `WaitResult<T>` (`IsSuccess`/`Elapsed`/`Attempts`/`LastValue`) в `Core/Helpers/Waiting/`; дефолты 30s/2s; ассерт на `Elapsed` для time-limit тестов (`*_AppearsWithinTimeLimit`, категория Performance). Мгновенные API и latency одного запроса — не через хелпер (Stopwatch + `MaxResponseTimeMs`)
- Test data для write: только вымышленные значения (публичный sandbox), vary ≥2 размерности (вариативные поля — через `Core/Helpers/Data/DataGenerator`), обфускация заменой, метод-нота для сложных наборов
- Чистка артефактов обязательна: temp-логи (`%TEMP%\GredjaTestRun`) чистит `TestRunWorkspace.PrepareRun()`; файлы старше 7 дней (`ArtifactRetentionDays`) в `allure-results/` и `TestResults/` — `TestRunReportGenerator.PrepareRun()` → `CleanupAccumulatedArtifacts()`; новые каталоги артефактов регистрировать там же в том же коммите; cleanup не фейлит прогон (warning вместо failure)

**Workflow:**
- План → одобение → изменения → отчёт
- Все `dotnet build/test/format` — через `Gredja.slnx` (в корне проекта; `.sln` нет)
- Параллелизм тестов: механизм включён (`[assembly: Parallelizable(Fixtures)]` в `Api/AssemblyInfo.cs` и `E2E/AssemblyInfo.cs`), число потоков = **1 по умолчанию**; менять через `.runsettings` (`<NUnit><NumberOfTestWorkers>`) или CLI: `dotnet test -- NUnit.NumberOfTestWorkers=8` (на 8 потоках solution-ран ~10s вместо ~57s)
- Перед коммитом: `dotnet format` + `dotnet test` должны пройти
- SDK пин: `global.json` → `10.0.401`, `rollForward: latestFeature` — сборка на другой машине берёт ближайший совместимый SDK, а не случайный
- Коммиты только по запросу
- Гигиена репо: корень — только файлы проекта; вывод инструментов → gitignored/%TEMP%; после массовых правок `git add -A` без EOL-warning (`.gitattributes`: .cs/.csproj → CRLF, .md → LF; LF-файлы от инструментов нормализует `Scripts/normalize-eol.ps1` — перед add запускает `/commit`); структурные изменения включают удаления gitignored-файлов → `FILE_STRUCTURE.md`/`AGENTS.md` обновлять в том же ходу, даже без коммита
- Генерация тестов — только через скиллы `.mimocode/skills/*`; параллельные библиотеки промптов запрещены (`Prompts/` удалён из-за дрейфа содержания)

---

## Часть 10. Частые задачи

### "Добавить тест для нового endpoint"

1. Напиши в MiMoCode: "Создай тест для GET /comments"
2. Он прочитает `documentation/{Service}ObservableBehaviour.md` (или создаст из шаблона)
3. Напишет тестовый класс по правилам проекта с seed-методологией (5 seeds → ~15-20 тестов)
4. Ты проверяешь и говоришь "закоммить" или вносишь правки

### "Добавить E2E тест для write operations"

1. Напиши в MiMoCode: "Создай E2E тесты для POST /repos/{owner}/{repo}/issues"
2. Он прочитает Observable Behaviour (POST/PATCH/DELETE секции)
3. Сгенерирует тесты с Setup (создание ресурса) и Teardown (удаление)
4. Каждый тест независим — создаёт свой ресурс и удаляет после себя

### "Запустить тесты и посмотреть отчёт"

Напиши `/test-report` — MiMoCode запустит тесты и откроет Allure-отчёт в браузере. Или запусти сам `dotnet test --verbosity minimal` — после прогона в `TestResults/` появится файл-таблица `TestRunReport-*.md` с результатами каждого теста и итогом в процентах.

### "Проверить код перед коммитом"

Напиши `/review-commit` — MiMoCode проверит все не закоммиченные изменения по правилам проекта.

### "Закоммить изменения"

Напиши `/commit` — MiMoCode проверит форматирование, сделает ревью, запустит HealthCheck-тесты, синхронизирует бэкапы, закоммитит и запушит.

### "Что-то сломалось"

Напиши что сломалось и покажи ошибку. MiMoCode найдёт проблему и предложит исправление.

### "Посмотреть покрытие API"

Напиши "проанализируй покрытие" — MiMoCode проверит какие эндпоинты имеют тесты, сколько тестов на каждый, и покажет нехватку по seed-методологии.

---

## Часть 11. Troubleshooting

### `dotnet build` падает с ошибкой

**"SDK not found" или "No installed .NET SDK"**

Установи .NET 10.0 SDK: https://dotnet.microsoft.com/download

Проверь: `dotnet --version` — должно показать 10.x

**"Could not resolve project"**

Проверь что ты в корне проекта (где лежит `Gredja.slnx`). Попробуй:

```
dotnet restore
dotnet build
```

### `dotnet test` падает

**"No test found"**

Проверь что проект скомпилировался: `dotnet build`

**Тесты падают с ошибкой подключения**

- JSONPlaceholder — фейковый API, проблемы с интернетом
- GitHub API — нужен `.env` с `GITHUB_PAT` (см. Часть 4)

**Тесты падают с 401/403**

- Проверь что `.env` содержит валидный `GITHUB_PAT`
- Токен должен иметь scope `public_repo` (read) или `repo` (read + write)
- Проверь что токен не истёк: GitHub → Settings → Developer settings → Personal access tokens

### Allure не генерируется

**"allure is not recognized"**

Установи Allure CLI: `winget install AllureCommandLine`

**"Java not found"**

Установи Java 17+: `winget install Microsoft.OpenJDK.17`

### MiMoCode не запускается

**"mimo is not recognized"**

Установи: `npm install -g mimocode`

Проверь что `npm` установлен: `npm --version`

**MiMoCode не понимает команды**

Убедись что конфиг создан (см. Часть 3). Без него субагенты не могут запускать тесты.

---

## Часть 12. Контакты и полезные ссылки

- **Репозиторий:** https://github.com/Gredja/AiTest
- **MiMoCode:** https://github.com/XiaomiMiMo/MiMo-Code
- **JSONPlaceholder:** https://jsonplaceholder.typicode.com
- **GitHub API:** https://docs.github.com/en/rest
- **Observable Behaviour (source of truth для тестов):** `documentation/JsonPlaceholderObservableBehaviour.md`, `documentation/GitHubObservableBehaviour.md` — читай OB своего сервиса
- **Test Plan:** `documentation/JsonPlaceholderTestPlan.md`, `.mimocode/plans/github-full-coverage.md` — покрытие и статусы
- **Правила:** `Rules/*.md` — код, ассерты, тест-практики

---

## Чеклист первого дня

**Обязательно:**
- [ ] Установлен Node.js + npm
- [ ] Установлен MiMoCode (`npm install -g mimocode`)
- [ ] Установлен .NET 10.0 SDK
- [ ] Установлен Java 17+ и Allure CLI
- [ ] Установлен Git
- [ ] Создан `~/.config/mimocode/mimocode.jsonc` (см. Часть 3)
- [ ] Клонирован репозиторий
- [ ] Создан `.env` с GITHUB_PAT (см. Часть 4)
- [ ] `dotnet build` проходит без ошибок
- [ ] `dotnet test` показывает пройденные тесты
- [ ] `mimo` запускается и отвечает на сообщения
- [ ] Написал `/test` — тесты запустились

**Рекомендовано:**
- [ ] Установлена Visual Studio Community (или VS Code) для удобной работы с кодом
- [ ] Подана заявка на MiMo Desktop (https://mimocode.mi.com)
- [ ] Прочитал OB своего сервиса (`documentation/{Service}ObservableBehaviour.md`) — понимаешь структуру документа
- [ ] Прочитал `Rules/test-practices.md` — знаешь seed-методологию и read-after-write visibility
