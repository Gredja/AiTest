# Token Budget — Gredja

Every automated AI workflow must declare a token budget. The budget is part of the spec, not part of the bill. Without a budget, agents can fan out, retry, and expand context — and the cost surfaces only after the fact.

---

## Active Workflows

### 1. AI-Driven Code Review

| Parameter | Value |
|-----------|-------|
| **Budget** | $0.20 per session |
| **Scope** | Single PR review |
| **Trigger** | PR opened or updated |

**When budget is exceeded:**
1. Stop the review immediately
2. Commit partial results (issues found so far) as an artefact
3. Escalate to human review for the remaining scope
4. Log actual spend vs budget in PR comment

---

### 2. Test Generation

| Parameter | Value |
|-----------|-------|
| **Budget** | 50,000 tokens per test class |
| **Scope** | One endpoint or feature |
| **Trigger** | User request or test plan update |

**When budget is exceeded:**
1. Stop generation
2. Commit generated tests with `// TODO: remaining cases — budget exceeded` marker
3. Report to user: generated N of M planned tests, budget spent
4. User decides: continue with fresh budget or finalize current set

---

### 3. Test Run + Auto-Fix

| Parameter | Value |
|-----------|-------|
| **Budget** | $0.10 per test run |
| **Scope** | Single `dotnet test` execution + fix attempts |
| **Trigger** | Test failure detected |

**When budget is exceeded:**
1. Stop auto-fix attempts (max 3 retries regardless of budget)
2. Commit all applied fixes
3. Report: tests fixed / tests still failing / budget spent
4. Remaining failures require human intervention

---

## Warning Thresholds

Every workflow must emit warnings as budget consumption increases:

| Threshold | Action |
|-----------|--------|
| **50%** | Info message: "Half the budget spent. Monitoring." |
| **80%** | Warning: "Approaching budget limit. Prepare to wrap up current step." |
| **90%** | Critical: "Budget nearly exhausted. Finish current operation only. No new tasks." |
| **100%** | Hard stop per workflow-specific escalation rules below |

Warnings are visible to the user in real time. The agent must not start new tool calls after 90% unless explicitly approved.

## Rules

- Every workflow spec in the repository must include a `token_budget` section
- Budget is set at spec time, not at billing time
- Budget must include warning thresholds (50% / 80% / 90%) and hard-stop escalation
- When budget is hit — stop, report, escalate. No silent overruns.
- Track actual spend per session for calibration. If a workflow consistently hits budget early, the spec needs adjustment — not the budget.
