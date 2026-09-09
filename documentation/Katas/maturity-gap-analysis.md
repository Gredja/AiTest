# Maturity Gap Analysis

**Date:** 2026-06-25
**Author:** Алексей — AQA Engineer
**Project:** Gredja
**Committed location:** `Gredja/documentation/Katas/maturity-gap-analysis.md`

---

## Scorecard

| Dimension | Level (L1 / L2 / L3) | Score (1.0 / 2.0 / 3.0) | Evidence (2–3 sentences) |
|---|---|---|---|
| AI Capabilities | L1 | 1.0 | AI helps in writing automated tests — generates tests, analyzes errors. But >50% deliverables are not through AI: manual environment setup, manual review, manual rule writing. Results vary depending on the model. |
| Reusability | L2 | 2.0 | 7 skills and 4 commands in `.mimocode/` — templates are reused via `/skill-name`. 7 rule files in `Rules/` are picked up by AGENTS.md. Any AI agent automatically applies rules when working with the project. |
| AI Champions | L1 | 1.0 | Alexey is the only person in the project. No team, no mandate, no designated Champion. |
| Performance Tracking | L1 | 1.0 | No productivity metrics. No tracking of AI time or cost. The only data point — model-selection-note (Kata 1). |
| DAU | L1 | 1.0 | 1 person, doesn't work every day. |
| **Average** | | **1.2** | |
| **Overall Level** | **L1** | | L1 = 1.0–1.9 |

---

## Gap Analysis

### Gap 1

**Dimension:** Performance Tracking
**Current level:** L1
**Why this gap is most damaging:** Without metrics, it's impossible to prove AI value and justify investments. Model selection is based on gut-feel, not data.
**Root cause:** No defined productivity metrics or tracking tools — AI is used but its impact is not measured.

---

### Gap 2

**Dimension:** AI Champions
**Current level:** L1
**Why this gap is most damaging:** No designated Champion — no mandate for scaling AI to the team. One enthusiast cannot change the process.
**Root cause:** Project is individual, no team — no organizational structure for appointing a Champion and transferring experience.

---

## 30-Day Improvement Plan

### Step 1 — addresses Gap 1

| Field | Value |
|---|---|
| **Action** | Create a `metrics.md` file in the project root. Track: number of tests generated via AI, time on generation vs manual writing, API call cost per model. Update after each use of /api-test-gen. |
| **Owner** | Alexey |
| **Timeline** | 2026-07-10 |
| **Success metric** | ≥5 entries in metrics.md with specific numbers (tests, time, cost) |

---

### Step 2 — addresses Gap 2

| Field | Value |
|---|---|
| **Action** | Document all AI processes in AGENTS.md and Rules/ so that a new person can start working with AI without explanations. Add an "AI Onboarding" section to AGENTS.md with step-by-step instructions. |
| **Owner** | Alexey |
| **Timeline** | 2026-07-15 |
| **Success metric** | "AI Onboarding" section in AGENTS.md contains ≥3 steps with specific commands (/api-test-gen, /review, /commit) |

---

## Peer Review

**Reviewer:** MiMo (AI — playing teammate)
**Date reviewed:** 2026-06-25

| Review question | Reviewer answer |
|---|---|
| Is the evidence for each dimension specific and observable — not aspirational? | Yes — all evidence describes what is done today, not plans. |
| Which score do you challenge, and why? | DAU — L1 is correct, but interestingly 1 out of 1 = 100% DAU. However, the matrix defines DAU as a "team" metric, so L1 is correct. |
| Is each root cause a structural/behavioural cause — not a symptom? | Yes — "no metrics" and "no team" are causes, not symptoms. |
| Are the success metrics measurable without asking the author? | Yes — ≥5 entries in metrics.md, ≥3 steps in AGENTS.md — verifiable without questions. |
| Would you sign off on this plan as a teammate? | Yes — the plan is specific, with dates and metrics. |

---

## Revision History

| Version | Date | Change | Author |
|---|---|---|---|
| 1.0 | 2026-06-25 | Initial commit | Алексей |
| 1.1 | 2026-06-25 | Peer review: all scores confirmed, metrics validated | MiMo (AI) |
