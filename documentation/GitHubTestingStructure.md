# GitHub Testing Structure

**Project:** Gredja (.NET 10.0)
**Date:** 2026-02-21

---

## 1. In Scope

| # | Area | What we test | Status |
|---|------|-------------|--------|
| 1 | **Repositories & Users** | GET /repos, GET /user/repos, GET /users/{username}, GET /users/{username}/repos — status, fields, pagination | Done |
| 2 | **Issues & Comments** | GET /repos/{owner}/{repo}/issues, /issues/{number}, /issues/{number}/comments — status, fields, pagination, state filter | Done |
| 3 | **Pull Requests** | GET /repos/{owner}/{repo}/pulls — status, fields, pagination, state filter | Done |
| 4 | **Branches** | GET /repos/{owner}/{repo}/branches — status, fields, pagination | Done |
| 5 | **Rate Limit & Misc** | GET /rate_limit — status, remaining, reset, sections | Done |

**Total:** ~55 read-only API tests across 10 endpoint groups.

## 2. Out of Scope

| Item | Rationale |
|------|-----------|
| **Write operations** (POST/PUT/DELETE on issues, PRs, comments) in Phase 1 | Write tests belong to Phase 2 (E2E project) with sandbox repo isolation, cleanup, and different risk profile. Mixing write ops into read-only suite would break test independence and require auth token management. |
| **GraphQL API** | The test suite targets REST API v3 only. GraphQL has a different endpoint, schema, and rate limit — out of scope for this phase. |
| **Webhook delivery** | Requires event simulation infrastructure not available in current test environment. |

## 3. Top 3 Risks

| # | Risk | Impact | Mitigation |
|---|------|--------|------------|
| 1 | **GitHub API rate limit exhaustion** — unauthenticated: 60 req/hour, authenticated: 5000 req/hour. CI runs with multiple test suites can burn through limits. | Tests fail with 403; false negatives mask real defects. | All GitHub tests use token auth (5000 req/hour). `GetRateLimitTests` monitors remaining budget. Performance category tests guard against redundant calls. |
| 2 | **External API instability** — GitHub downtime, degraded performance, or response schema changes break tests that aren't our fault. | False failures in CI; wasted investigation time. | Response time threshold (5s) is generous. Tests validate essential fields only (not exhaustive schema). Investigate flaky failures before marking as bugs. |
| 3 | **Test data drift** — hardcoded issue numbers (#5), usernames (`Gredja`), and repo names (`AiTest`) become stale if repo is renamed, issues deleted, or account changed. | Hard failures; test maintenance burden. | Constants are centralized in test classes and `GitHubEndpoints`. If data drifts, fix constants — not test logic. Consider dynamic lookup for critical IDs in future. |

## 4. Entry Criteria

- [ ] GitHub personal access token present in `.env` (`GITHUB_TOKEN`)
- [ ] Token has `public_repo` scope (minimum for read operations)
- [ ] `dotnet restore` and `dotnet build` succeed without errors
- [ ] Target repo `Gredja/AiTest` is accessible (not deleted, not renamed)
- [ ] Test issue #5 exists in the target repo

## 5. Exit Criteria

- [ ] All `HealthCheck` tests pass — every endpoint returns 200
- [ ] All `Regression` tests pass — field validation and data integrity
- [ ] All `Smoke` tests pass — Content-Type, pagination, filters
- [ ] All `Negative` tests pass — 404 for non-existent resources
- [ ] All `Performance` tests pass — response time < 5s for every endpoint
- [ ] Zero `Assert` or `NullReference` exceptions in test output
- [ ] Allure report generated with no broken/unknown statuses
- [ ] `dotnet format --verify-no-changes` passes
