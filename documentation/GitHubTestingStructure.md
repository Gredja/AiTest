# GitHub Testing Structure

**Project:** Gredja (.NET 10.0)
**Date:** 2026-10-16

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
| 11 | **GitHub UI (Playwright)** | Web UI: login/logout/auth state, repo landing, code view, 404s, issues & PRs flows, branches, labels, comments actions, anonymous gates — page-objects, storageState auth, write via UI → cleanup via API | Done (44 tests, `Ui/`) |
| 12 | **Collaborators & invitations** | GET/PUT /collaborators, /collaborators/{username}/permission, GET/DELETE /invitations (OB §28–28d) | Done (14 tests) |
| 13 | **Authenticated user** | GET /user (OB §5a) — status, contract, login match, 401s | Done (5 tests) |
| 14 | **Release detail** | GET /releases/latest, /releases/tags/{tag} (OB §15c–15d) + fixture infra §15e | Done (4 tests) |
| 15 | **Search API** | GET /search/repositories, /search/issues, /search/users (OB §29–29b) | Done (5 tests) |
| 16 | **Deepening & gotchas** | GET /git/ref/{ref} (§26a), POST /git/trees (§26b), git/commits (§27), issues-include-PRs, comments order, rate-limit headers, merged_at null | Done (18 tests) |

**Total:** 376 GitHub API tests live (`dotnet test --filter "Category=GitHub"` — read-only + single-write + negatives + gotchas across ~26 endpoint groups), 9 E2E-цепочек и 44 UI-теста. CI: required PR-gate (`format` + `HealthCheck`) + nightly informational two-pass (`Rules/git.md` → CI). Расширение UI до 44 закрыто 2026-10-19 (`.mimocode/plans/github-test-expansion.md` выполнен и удалён).

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

- [ ] GitHub personal access token present in `.env` (`GITHUB_PAT`, classic with `repo` scope — JSON `Token` → env → `.env`)
- [ ] `dotnet restore` and `dotnet build` succeed without errors
- [ ] Target repo `Gredja/AiTest` is accessible (not deleted, not renamed)
- [ ] UI credentials present in `.env` (`GITHUB_UI_EMAIL`, `GITHUB_UI_PASSWORD`, `GITHUB_UI_PAT`) and storageState `.auth/github-ui-state.json` is fresh (re-run `LoginSignInTests` when it expires)
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

## 6. E2E scenario numbering

The reserved `E2E-N` prefixes in `[Description]` (`Rules/test-practices.md` → Document sync) map 1:1 to the scenario classes in `E2E/GitHub/Tests/`:

| Prefix | Test class | Scenario |
|---|---|---|
| E2E-1 | `IssueLifecycleTests` | create → comment → verify comment linked → close → verify state |
| E2E-2 | `PullRequestFlowTests` | create branch → create PR → verify → merge → verify merged |
| E2E-3 | `CommentChainTests` | create issue → add comments → verify count and ordering |
| E2E-4 | `RepositoryHealthTests` | default branch, topics, visibility |
| E2E-5 | `CollaboratorPermissionTests` | owner access → invite → verify pending → revoke |
| E2E-6 | `RateLimitDrainTests` | consume calls → verify remaining decrements → reset is future |
| E2E-7 | `GitRefLifecycleTests` | create branch → visible → duplicate 422 → delete → gone |
| E2E-8 | `IssueStateMachineTests` | open → closed → reopen with list filters |
| E2E-9 | `FailedWriteTests` | PR without commits → 422 → no side effects |
