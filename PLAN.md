# PLAN

> Trigger: type "План" — agent opens this document.

## Statuses

| Status | Meaning |
|--------|---------|
| `[ ]` | Planned (not started) |
| `[~]` | In progress |
| `[x]` | Done |
| `[-]` | Cancelled |

---

## Roadmap

| # | Task | Status |
|---|------|--------|
| 1 | Project structure (File structure, .csproj) | `[~]` |
| 2 | Test plan (GET /products + GET /products/{id}, 22 tests) | `[~]` |
| 3 | Test generation prompt | `[~]` |
| 4 | Generate and review tests (22 tests, 19 pass + 3 Ignore) | `[x]` |
| 5 | Metrics (pass rate + coverage infrastructure) | `[~]` |
| 6 | Git setup & branching (master→main, features/*) | `[x]` |
| 7 | Cover another service with tests (service TBD) | `[ ]` |

---

## Notes

- **Task #7** — architecture plan for adding a new service: `MEMORY.md` → "Architecture decisions" → "New service in single solution"
