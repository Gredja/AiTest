# Eval: e2e-test-gen

Скилл: `.mimocode/skills/e2e-test-gen/SKILL.md` (версия: смотреть Revision History).
Формат и правило прогона — `documentation/SkillEval/README.md`.

| # | Кейс | Статус до | Статус после |
|---|------|-----------|--------------|
| 1 | Write-цепочка create → verify → cleanup | not run | not run |

---

## Кейс 1 — Write-цепочка create → verify → cleanup

**Вход (точный запрос агенту):**

> Прочитай `documentation/{Service}ObservableBehaviour.md` и сгенерируй E2E-тесты
> через /e2e-test-gen для `{resource}` в песочнице {repo}. Service = {Service}.

**Обязано:**
- [ ] План: Part 1 (сиды: happy/negative/edge) + Part 2 (полный список имён методов, категорий, файл) → STOP; кода до «да» нет
- [ ] Перед кодом — чтение всех Rules/*.md
- [ ] Цепочка: POST → verify → GET/PATCH → cleanup; cleanup-порядок (комменты до issues, ветки до PR)
- [ ] Fixture-registry: ID регистрируются СРАЗУ после создания, ДО assert (register-then-assert)
- [ ] Cleanup ТОЛЬКО в `[OneTimeTearDown]` (последний член класса), warning-only; per-test `[SetUp]`/`[TearDown]`/`finally` нет
- [ ] Нет вспомогательных методов в `*Tests.cs` (цепочки — в base/helpers)
- [ ] Write-данные вымышленные, ≥2 размерности; `ShouldMatchRequest()` для request↔response
- [ ] Сверка план↔код таблицей до format/build/тестов; таблица результатов прогона; запуск зелёный
- [ ] JSON-отчёт: `cleanupVerified = true`, validator → PASS

**Запрещено:**
- [ ] Cleanup в `finally` тест-метода; register после assert (утечка при падении assert)
- [ ] `Assert.*` (NUnit), синхронный `Execute`, `Thread.Sleep`
- [ ] Хардкод несуществующих ID (999); статические write-данные («test»); хардкод endpoint-путей
- [ ] Помощники внутри тест-класса; логин/логаут в тесте; нарушение порядка cleanup
- [ ] Красный прогон или `cleanupVerified = false` с предложением коммита
