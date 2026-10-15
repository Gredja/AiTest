# Bug Report Template

Six-field defect entry format (from Module 600 — Quality, Block 6).

---

## Fields

| # | Field | Rule |
|---|-------|------|
| 1 | **Title** | One-line summary naming the **user impact**, not the symptom. *"Customer charged but no order created on payment retry"* beats *"weird behaviour"* |
| 2 | **Steps to reproduce** | Numbered, **minimal**. Trim until removing one more step loses the condition. If > 10 steps — you're reproducing a workflow, not a condition |
| 3 | **Expected result** | What should have happened |
| 4 | **Actual result** | What actually happened |
| 5 | **Severity** | How bad it is when it hits (1 = blocker, 4 = cosmetic) |
| 6 | **Priority** | How soon to fix (1 = now, 4 = whenever) |

## Severity vs Priority

Not the same thing:

| Case | Severity | Priority |
|------|----------|----------|
| Typo on homepage | 3 (minor) | 1 (executives noticed) |
| Rare edge case corrupting an order | 1 (blocker) | 3 (one customer per month) |

When the two get conflated, the wrong things ship and the wrong things get patched at midnight.

## Template

```markdown
## Bug: [Title — user impact, not symptom]

### Steps to Reproduce
1. ...
2. ...
3. ...

### Expected Result
...

### Actual Result
...

### Severity: [1-4]
### Priority: [1-4]
```

## Rules

- **Severity and Priority are human judgement calls** — AI can draft the entry, but never assign sev/pri
- A bug nobody can reproduce is a **story**, not a defect — no engineer can write the failing test from it
- For AI-feature bugs: also log the exact prompt/inputs and model version
