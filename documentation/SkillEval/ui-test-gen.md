# Eval: ui-test-gen

Скилл: `.mimocode/skills/ui-test-gen/SKILL.md` (версия: смотреть Revision History).
Формат и правило прогона — `documentation/SkillEval/README.md`.

| # | Кейс | Статус до | Статус после |
|---|------|-----------|--------------|
| 1 | Страница с локаторами: навигация + проверка | not run | not run |

---

## Кейс 1 — Страница с локаторами: навигация + проверка

**Вход (точный запрос агенту):**

> Прочитай `documentation/{service}UiObservableBehaviour.md` и сгенерируй UI-тесты
> через /ui-test-gen для страницы `{page}`: `{flow}`. Service = {service}.

**Обязано:**
- [ ] Сначала live-снапшот: `node Scripts/locator-dump.mjs <url>` — каждый локатор из снапшота, с датой в UI-OB
- [ ] План: Part 1 (кейс, категория, ресурсы→клинер, источник локатора) + Part 2 (имена `{{area}}_{{Flow}}_{{Outcome}}`, категории, `[Description]` с UI-OB §, файлы incl. page-object) → STOP; файлов до «да» нет
- [ ] Перед кодом — чтение всех Rules (включая `ui-testing.md`)
- [ ] Локаторы только role/text/label (`GetByRole`/`GetByText`/`GetByLabel`); CSS/XPath на тестируемой странице запрещены
- [ ] Page-objects: бизнес-действия (`CreateIssue`, не сырые клики); тесты — оркестрация + ассерты
- [ ] Аутентификация через `storageState`; логин только в `LoginSignInTests`; гибрид «действие в UI → проверка через API»
- [ ] Write через UI → cleanup через API в `[OneTimeTearDown]` (последний член), register-then-assert
- [ ] Данные через `DataGenerator.RandomString` + `TestDataConstants`; без `WaitForTimeout` без WHY
- [ ] Сверка план↔код (тесты и page-object) до format/тестов; таблица результатов; запуск зелёный
- [ ] JSON-отчёт: `locators.draft = 0`, validator → PASS

**Запрещено:**
- [ ] Локаторы из памяти модели; CSS-классы/XPath на странице под тестом; draft-локаторы в коде
- [ ] Логаут в feature-тесте; логин в каждом тесте; `WaitForTimeoutAsync(3000)` как замена ожидания
- [ ] `Assert.*` (NUnit), helper-методы в `*Tests.cs`, скриншот-код в тестах
- [ ] Статические «test»-данные; cleanup без register-before-assert
- [ ] Файлы созданы до одобрения плана; красный прогон с предложением коммита
