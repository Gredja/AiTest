# GitHub Testing Structure

**Project:** Gredja (.NET 10.0)
**Date:** 2026-09-30

---

## 1. In Scope

| # | Area | What we test | Status |
|---|------|-------------|--------|
| 1 | **Repositories & Users** | GET /repos, GET /user/repos, GET /users/{username}, GET /users/{username}/repos — status, fields, pagination | Done |
| 2 | **Issues & Comments** | GET /repos/{owner}/{repo}/issues, /issues/{number}, /issues/{number}/comments — status, fields, pagination, state filter | Done |
| 3 | **Pull Requests** | GET /repos/{owner}/{repo}/pulls — status, fields, pagination, state filter | Done |
| 4 | **Branches** | GET /repos/{owner}/{repo}/branches — status, fields, pagination | Done |
| 5 | **Rate Limit & Misc** | GET /rate_limit — status, remaining, reset, sections | Done |
| 6 | **Pull Request details** | GET /pulls/{pull_number}/files, /pulls/{pull_number}/commits — status, contract, fields, negatives | Done (D1–D2) |
| 7 | **Branch detail** | GET /branches/{branch} — status, contract, fields, negatives | Done (D3) |
| 8 | **Repo metadata** | GET /contributors, /languages, /topics, /tags — status, contract, fields, negatives | Done (D4–D7) |
| 9 | **Commits & Releases** | GET /commits, /releases — status, contract, fields, negatives | Done (D8–D9) |
| 10 | **PR detail, Users list & Comment by id** | GET /pulls/{pull_number} (OB §9c), GET /users (OB §3a), GET /issues/comments/{comment_id} (OB §8a) | Done (Phase 1 close-out) |

**Total:** ~210 read-only tests across 20 endpoint groups (**Phase 1 complete**: 23 GET-роута покрыты + GET /repos = no such route, N/A) + 68 single-write tests (Phase 2: §17–20 issues/comments, §21–23 PR create/patch/merge, §24 DELETE-not-supported, §25–26 git refs); scratch-PR pipeline = blob→tree→commit→branch→PR (infra OB §27); next: E2E scenarios 2.4–2.6 — see [.mimocode/plans/github-full-coverage.md](../.mimocode/plans/github-full-coverage.md).

## 2. Out of Scope

| Item | Rationale |
|------|-----------|
| **Multi-step write chains in the read-only suite** | Scenario chains (create → verify → mutate → cleanup) live in the separate `E2E/` project: sandbox repo isolation, mandatory cleanup, different risk profile. Single-endpoint write tests (POST + its negatives) stay in `Api/{Service}/Tests` — same place JsonPlaceholder write tests live. |
| **GraphQL API** | The test suite targets REST API v3 only. GraphQL has a different endpoint, schema, and rate limit — out of scope for this phase. |
| **Webhook delivery** | Requires event simulation infrastructure not available in current test environment. |

## 3. Top 3 Risks

**1. GitHub API rate-limit exhaustion** (shared token, 5000 req/h; unauthenticated only 60).
Parallel CI runs burn the quota mid-suite and every subsequent test fails with 403.
The failure looks like broken auth rather than an empty tank, so engineers chase a phantom defect instead of waiting for reset.
The suite's signal collapses — real regressions get dismissed as "probably rate limit".

*Mitigation:* all GitHub tests use token auth (5000 req/hour); `GetRateLimitTests` monitors remaining budget; Performance category guards against redundant calls; monitor `X-RateLimit-Remaining` in CI.

**2. External API instability** — GitHub downtime, latency, or response schema changes.
Tests fail for reasons unrelated to our code: CI shows red with no local change to investigate.
Releases get delayed chasing upstream noise — or worse, real failures get ignored as "just GitHub being flaky".

*Mitigation:* generous 5s response-time threshold; validate essential fields only (not exhaustive schema); investigate flaky failures before marking them as defects.

**3. Test-data drift** — hardcoded dependencies: issue #5, PR #5 (design D1–D2), branch `main`, user `Gredja`, repo `AiTest`.
Rename, close, or delete any of them and dozens of tests fail at once with no code change behind them.
The team loses trust in the suite and stops running it — the moment coverage starts rotting.

*Mitigation:* constants centralized in test classes and `GitHubEndpoints`; design D1–D2 uses dynamic PR lookup (`max + NonExistentIdOffset` via `[OneTimeSetUp]` — offset guards against concurrent E2E writes in the same run); if data drifts, fix constants — not test logic.

## 4. Entry Criteria

- [ ] GitHub personal access token present in `.env` (`GITHUB_TOKEN`)
- [ ] Token has `public_repo` scope (minimum for read operations)
- [ ] `dotnet restore` and `dotnet build` succeed without errors
- [ ] Target repo `Gredja/AiTest` is accessible (not deleted, not renamed)
- [ ] Test issue #5 exists in the target repo
- [ ] Test PR #5 exists (needed for design D1–D2: PR files/commits)
- [ ] Default branch `main` exists (needed for design D3)
- [ ] Target repo has at least one branch with a slash in its name (slash-edge case 11.6 resolves it via dynamic lookup from `GET /branches` — keep a permanent test branch, e.g. `test/branch-with-slash`, alive for the suite's lifetime)
- [ ] `api.github.com` reachable from the test runner (network/proxy allows it)

## 5. Exit Criteria

- [ ] All `HealthCheck` tests pass — every endpoint returns 200
- [ ] All `Regression` tests pass — field validation and data integrity
- [ ] All `Smoke` tests pass — Content-Type, pagination, filters
- [ ] All `Negative` tests pass — 404 for non-existent resources
- [ ] All `Performance` tests pass — response time < 5s for every list endpoint
- [ ] Every endpoint has ≥ 5 negative tests (seed methodology floor)
- [ ] Zero `Assert` or `NullReference` exceptions in test output
- [ ] Allure report generated with no broken/unknown statuses
- [ ] `dotnet format --verify-no-changes` passes
- [ ] Documents in sync: Test Plan statuses ↔ Observable Behaviour sections ↔ tests
