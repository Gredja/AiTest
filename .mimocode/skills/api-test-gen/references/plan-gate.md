# Step 4 — Plan the Test List (mandatory gate)

Before writing ANY code — build the plan and STOP for user approval. This step is mandatory for every endpoint; skipping it invalidates the run.

**Part 1 — Seed table.** Plan test cases using Seed → Expand → Review:
1. Write 5 seeds per endpoint (2 happy, 2 failure, 1 edge) — see `Rules/test-practices.md` → "Seed methodology"
2. Expand to table: `# | Case | Category | Priority | Source seed` — with concrete example for the endpoint
3. Enforce minimum 5 active negatives per endpoint (`[Ignore]`d don't count; mock-API exception + ceiling rule — see `Rules/test-practices.md` Step 4)
4. **One negative test per required request field** — `*_Missing{Field}_*` for EVERY required body field (mandatory for all services; empty-body test doesn't substitute)
5. Target ~15-20 unique test cases per endpoint

**Part 2 — Full list of future test methods.** Translate the seed table into the exact code-level plan — every future test method as it will appear in the file:
- method name (`{Endpoint}_{Case}_{Expected}`, PascalCase)
- `[Category(...)]` (HealthCheck / ContractCheck / Smoke / Regression / Performance / …)
- `[Description(...)]`
- target file (`Api/{Service}/Tests/{Category}/{Endpoint}Tests.cs`)

Also list files to be **created** and files to be **modified**.

**Gate:** output Part 1 + Part 2 in chat and STOP. Ask the user to approve or request modifications. On modifications → update the plan, re-present, wait again. Write code ONLY after explicit approval. Never jump straight from seeds to code.
