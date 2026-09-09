# Maturity Gap Analysis

**Date:** 2026-06-25
**Author:** Алексей — AQA Engineer
**Project:** Gredja
**Committed location:** `Gredja/documentation/Katas/maturity-gap-analysis.md`

---

## Scorecard

| Dimension | Level (L1 / L2 / L3) | Score (1.0 / 2.0 / 3.0) | Evidence (2–3 sentences) |
|---|---|---|---|
| AI Capabilities | L1 | 1.0 | AI помогает в написании автотестов — генерирует тесты, анализирует ошибки. Но >50% deliverables не через AI: ручная настройка среды, ручной ревью, ручное написание правил. Результаты варьируются в зависимости от модели. |
| Reusability | L2 | 2.0 | 7 скиллов и 4 команды в `.mimocode/` — шаблоны переиспользуются через `/skill-name`. 7 файлов правил в `Rules/` подхватываются AGENTS.md. Любой AI-агент автоматически применяет правила при работе с проектом. |
| AI Champions | L1 | 1.0 | Алексей — единственный человек в проекте. Нет команды, нет mandate, нет designated Champion. |
| Performance Tracking | L1 | 1.0 | Нет метрик продуктивности. Нет трекинга времени или стоимости AI. Единственный data point — model-selection-note (Kata 1). |
| DAU | L1 | 1.0 | 1 человек, работает не каждый день. |
| **Average** | | **1.2** | |
| **Overall Level** | **L1** | | L1 = 1.0–1.9 |

---

## Gap Analysis

### Gap 1

**Dimension:** Performance Tracking
**Current level:** L1
**Why this gap is most damaging:** Без метрик невозможно доказать ценность AI и обосновать инвестиции. Модель选择 основана на gut-feel, а не на данных.
**Root cause:** Нет определённых метрик продуктивности и инструментов для их трекинга — AI используется, но его impact не измеряется.

---

### Gap 2

**Dimension:** AI Champions
**Current level:** L1
**Why this gap is most damaging:** Нет designated Champion — нет mandate для масштабирования AI на команду. Один энтузиаст не может изменить процесс.
**Root cause:** Проект индивидуальный, нет команды — нет organizational structure для назначения Champion и передачи опыта.

---

## 30-Day Improvement Plan

### Step 1 — addresses Gap 1

| Field | Value |
|---|---|
| **Action** | Создать файл `metrics.md` в корне проекта. Трекать: количество сгенерированных тестов через AI, время на генерацию vs ручное написание, стоимость API-вызовов по модели. Обновлять после каждого использования /api-test-gen. |
| **Owner** | Алексей |
| **Timeline** | 2026-07-10 |
| **Success metric** | ≥5 записей в metrics.md с конкретными цифрами (тесты, время, стоимость) |

---

### Step 2 — addresses Gap 2

| Field | Value |
|---|---|
| **Action** | Документировать все AI-процессы в AGENTS.md и Rules/ так, чтобы новый человек мог начать работу с AI без объяснений. Добавить секцию "AI Onboarding" в AGENTS.md с пошаговой инструкцией. |
| **Owner** | Алексей |
| **Timeline** | 2026-07-15 |
| **Success metric** | Секция "AI Onboarding" в AGENTS.md содержит ≥3 шага с конкретными командами (/api-test-gen, /review, /commit) |

---

## Peer Review

**Reviewer:** MiMo (AI —扮演 тиммейт)
**Date reviewed:** 2026-06-25

| Review question | Reviewer answer |
|---|---|
| Is the evidence for each dimension specific and observable — not aspirational? | Да — все evidence описывают что делается сегодня, не планы. |
| Which score do you challenge, and why? | DAU — L1 корректен, но有趣的是 что 1 из 1 = 100% DAU. Однако матрица определяет DAU как "team" metric, поэтому L1 правильный. |
| Is each root cause a structural/behavioural cause — not a symptom? | Да — "нет метрик" и "нет команды" causes, не symptoms. |
| Are the success metrics measurable without asking the author? | Да — ≥5 записей в metrics.md, ≥3 шага в AGENTS.md — проверяемые без вопросов. |
| Would you sign off on this plan as a teammate? | Да — план конкретный, с датами и метриками. |

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-06-25 | Initial commit | Алексей |
| 1.1 | 2026-06-25 | Peer review: all scores confirmed, metrics validated | MiMo (AI) |
