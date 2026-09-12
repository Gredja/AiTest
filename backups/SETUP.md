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
GITHUB_TOKEN=твой_личный_токен_здесь
```

**Как получить токен:**
1. Зайди на GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Нажми "Generate new token"
3. Дай имя (например `gredja-dev`)
4. Выбери scopes: `repo` (полный доступ к репозиториям)
5. Скопируй токен и вставь в `.env`

Токен нужен для тестов GitHub API.

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
dotnet test --verbosity minimal    # должны пройти тесты
```

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
├── AGENTS.md                     # Описание проекта для ИИ-агента
├── Core/
│   ├── Config/                   # URL и пути к API endpoint'ам
│   │   ├── FakeStoreEndpoints.cs
│   │   ├── JsonPlaceholderEndpoints.cs
│   │   └── GitHubEndpoints.cs
│   └── Models/                   # Модели данных (Response/Request)
│       ├── FakeStore/            # Модели FakeStoreAPI
│       ├── JsonPlaceholder/      # Модели JSONPlaceholder
│       ├── GitHub/               # Модели GitHub API
│       └── Generic/              # Общие модели
├── Api/
│   ├── FakeStore/Tests/          # Тесты FakeStoreAPI
│   ├── JsonPlaceholder/Tests/    # Тесты JSONPlaceholder
│   └── GitHub/Tests/             # Тесты GitHub API
├── E2E/                          # E2E тесты (цепочки связей)
├── TestAdapter/                  # Allure-адаптер
├── Scripts/                      # Скрипты (allure-report.ps1)
├── Rules/                        # Правила кодирования
├── documentation/                # Документация
├── Prompts/                      # Шаблоны промптов
└── .mimocode/skills/             # Скиллы для MiMoCode
```

**Проект тестирует три REST API:**
- **FakeStoreAPI** — интернет-магазин (20 товаров, IDs 1-20)
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
| `/test` | Запускает все тесты |
| `/test-report` | Запускает тесты + генерирует Allure-отчёт |
| `/commit` | Форматирование + ревью + тесты + синк бэкапов + коммит + пуш |
| `/review-commit` | Ревью не закоммиченных изменений |
| `/review-pr` | Ревью pull request |
| `/api-test-gen` | Генерирует шаблон теста для нового API endpoint |
| `/gredja-rules` | Показывает правила проекта |

Просто напиши `/test` в чате с MiMoCode — и он запустит тесты.

### 7.3. Важные правила поведения

1. **MiMoCode НЕ коммитит сам** — только по твоей просьбе. Напиши "закоммить" или `/commit`.
2. **MiMoCode показывает план перед работой** — для каждой задачи он предложит план, подождёт одобрения, потом выполнит.
3. **Работа в ветках** — все изменения делаются в ветках `features/<topic>`, не в `main`.
4. **Файлы с `.env` секретами** — никогда не коммить файл `.env`.

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
- Все API-запросы async (`ExecuteAsync`)
- Маленькие методы, одно действие, максимум ~30 строк
- Конкретные исключения вместо `Exception`, без `null!`
- LINQ: `Any()` вместо `Count() > 0`, без лишних `.ToList()`
- Строки: интерполяция `$""`, `StringBuilder` в циклах
- SOLID: один класс — одна задача, зависимости через интерфейсы

**Модели:**
- Response модели: суффикс `Model` (включает `Id`)
- Request модели: суффикс `Request` (без `Id`)
- Чистые контейнеры данных — без логики

**Тесты:**
- 1 endpoint = 1 тестовый класс
- Сначала позитивные тесты, потом негативные
- FluentAssertions (не NUnit Assert)
- Категории: тип сервиса (FakeStore/JsonPlaceholder/GitHub/Ui) + тип проверки (HealthCheck/Smoke/Regression/Negative/Performance)
- Тесты независимы друг от друга, Given/When/Then структура
- Проверяй HTTP status и body отдельно

**Workflow:**
- План → одобение → изменения → отчёт
- Перед коммитом: `dotnet format` + `dotnet test` должны пройти
- Коммиты только по запросу

---

## Часть 10. Частые задачи

### "Добавить тест для нового endpoint"

1. Напиши в MiMoCode: "Создай тест для GET /comments"
2. Он прочитает шаблон в `Prompts/templates/api-test-generation.md`
3. Напишет тестовый класс по правилам проекта
4. Ты проверяешь и говоришь "закоммить" или вносишь правки

### "Запустить тесты и посмотреть отчёт"

Напиши `/test-report` — MiMoCode запустит тесты и откроет Allure-отчёт в браузере.

### "Проверить код перед коммитом"

Напиши `/review-commit` — MiMoCode проверит все не закоммиченные изменения по правилам проекта.

### "Закоммить изменения"

Напиши `/commit` — MiMoCode проверит форматирование, сделает ревью, запустит HealthCheck-тесты, синхронизирует бэкапы, закоммитит и запушит.

### "Что-то сломалось"

Напиши что сломалось и покажи ошибку. MiMoCode найдёт проблему и предложит исправление.

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

- FakeStoreAPI и JSONPlaceholder — фейковые API, проблемы с интернетом
- GitHub API — нужен `.env` с `GITHUB_TOKEN` (см. Часть 4)

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
- **FakeStoreAPI:** https://fakestoreapi.com/docs
- **JSONPlaceholder:** https://jsonplaceholder.typicode.com
- **GitHub API:** https://docs.github.com/en/rest

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
- [ ] Создан `.env` с GITHUB_TOKEN (см. Часть 4)
- [ ] `dotnet build` проходит без ошибок
- [ ] `dotnet test` показывает пройденные тесты
- [ ] `mimo` запускается и отвечает на сообщения
- [ ] Написал `/test` — тесты запустились

**Рекомендовано:**
- [ ] Установлена Visual Studio Community (или VS Code) для удобной работы с кодом
- [ ] Подана заявка на MiMo Desktop (https://mimocode.mi.com)
