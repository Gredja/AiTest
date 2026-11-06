# GitHub Testing Structure

**Project:** Gredja (.NET 10.0)
**Date:** 2026-11-04

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
| 12 | **Collaborators & invitations** | GET/PUT /collaborators, /collaborators/{username}/permission, GET/DELETE /invitations (OB §28–28d) | Done (21 tests) |
| 13 | **Authenticated user** | GET /user (OB §5a) — status, contract, login match, 401s | Done (5 tests) |
| 14 | **Release detail & by-id** | GET /releases/latest, /releases/tags/{tag} (OB §15c–15d), GET/DELETE /releases/{release_id} (OB §15e–15f) + fixture infra §15e | Done (22 tests) |
| 15 | **Search API** | GET /search/repositories, /search/issues, /search/users (OB §29–29b) | Done (30 tests) |
| 16 | **Deepening & gotchas** | GET /git/ref/{ref} (§26a), POST /git/trees (§26b), POST /git/blobs (§26c, dedicated tests since 2026-11-04), git/commits (§27), issues-include-PRs, comments order, rate-limit headers, merged_at null | Done (27 tests) |

**Total:** 481 GitHub API tests live (`dotnet test --filter "Category=GitHub"` — read-only + single-write + negatives + gotchas across ~29 endpoint groups), 9 E2E scenario chains and 44 UI tests. CI: required PR-gate (`format` + `HealthCheck`) + nightly informational two-pass (`Rules/git.md` → CI). Thin-fixture deepening (search/invitations/repos-no-route/git-commit: contract + count-consistency + content-type + auth negatives) closed 2026-10-26; negative-floor program (priority batch + follow-up batch: +45 tests, all 48 fixtures at floor or with documented ceiling) closed 2026-11-02 (§7); release-by-id + DELETE release + git-blob dedicated tests (OB §15e–15f, §26c — the two endpoints the coverage report flagged as untested) added 2026-11-04 (+25 tests, live-probed first). The UI expansion to 44 tests was closed on 2026-10-19 (`.mimocode/plans/github-test-expansion.md` completed and deleted).

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
- [ ] Every endpoint has ≥ 5 active negative tests **or a documented ceiling** (`Rules/test-practices.md` Step 4 — see §7 Negative Floor Status)
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
| E2E-10 | `IssuesTests` | full-field create → read-back; combined content PATCH; close-one-of-two count; DELETE absent → 404 → fallback close; rapid create-close-create; special-char read-back |

## 7. Negative Floor Status

Rule: `Rules/test-practices.md` Step 4 — ≥5 active negatives per endpoint; mock-API exception + ceiling rule apply when distinct useful failure modes are exhausted (live probes 2026-11-02).

**Coverage (2026-11-06 — `IssuesTests` E2E batch; counters measured on the working tree): 52 / 52 fixtures accounted for — 35 at floor + 17 with a documented ceiling; 235 active negatives of 507 GitHub tests.**

### Fixtures at floor (35)

| Fixture | Neg | Fixture | Neg | Fixture | Neg |
|---|---:|---|---:|---|---:|
| AuthNegative | 5 | CreateComment | 5 | CreateGitBlob | 6 |
| CreateGitCommit | 5 | CreateGitTree | 5 | CreateInvitation | 5 |
| CreateIssueNegative | 6 | CreatePullRequest | 7 | CreateRef | 6 |
| DeleteComment | 5 | DeleteInvitation | 5 | DeleteRef | 5 |
| DeleteRelease | 5 | GetBranchByName | 5 | GetCollaboratorPermission | 5 |
| GetCommits | 5 | GetContributors | 5 | GetIssueById | 5 |
| GetIssueComments | 8 | GetIssues | 5 | GetLanguages | 5 |
| GetPublicRepos | 5 | GetPullRequestById | 6 | GetPullRequestCommits | 6 |
| GetPullRequestFiles | 6 | GetReleases | 5 | GetReleaseById | 5 |
| GetRepository | 5 | GetTags | 5 | GetTopics | 5 |
| GetUserRepos | 5 | SearchIssues | 5 | SearchRepositories | 5 |
| SearchUsers | 5 | UpdateIssue | 5 | | |

### Fixtures with a documented ceiling (16)

| Fixture | Neg | Ceiling | Where documented |
|---|---:|---:|---|
| GetAuthenticatedUser | 2 | 2 | OB §5a — both auth modes covered; no path/query surface |
| GetAuthenticatedUserRepos | 2 | 2 | OB §5 — 403 wrong-scope needs a scopeless token the suite does not hold |
| GetBranches | 4 | 4 | OB §10 — missing auth → 200, query normalization → 200 |
| GetCollaborators | 4 | 4 | OB §28 — auth-required route fully covered, no query modes |
| GetGitCommit | 4 | 4 | OB §27 — missing auth is not a 401 on this public read |
| GetGitRefByName | 4 | 4 | OB §26a — missing auth → 200, no query surface |
| GetInvitations | 4 | 4 | OB §28c — auth-required route fully covered |
| GetPullRequests | 4 | 4 | OB §9 — missing auth → 200 (public), `per_page`/`page` normalized |
| GetRateLimit | 1 | 1 | OB §16 — endpoint always 200 when credentials are absent or valid |
| GetReleases* → GetReleaseDetail | 4 | 4 | OB §15c/15d — no-releases mode untestable while the fixture creates a release |
| GetRepos | 2 | 2 | OB §1a — a non-existent route can only 404 (route check precedes auth) |
| GetUser | 4 | 4 | OB §3 — no query params; missing auth → 200 |
| GetUsers | 1 | 1 | OB §3a — no path segments; invalid queries silently ignored |
| MergePullRequest | 4 | 4 | OB §23 — all other modes collapse to 200 |
| UpdatePullRequest | 4 | 4 | OB §22 — null `title` / unknown `state` accepted (200) |
| CreateIssueVisibility | 0 | — | OB §17 — per-ENDPOINT floor met by `CreateIssueNegativeTests` (6); this fixture is Performance-only by design |
| IssuesTests (E2E) | 1 | 1 | OB §24 — DELETE /issues/{n} absent (404) is its single failure mode; per-endpoint floor met by `CreateIssueNegativeTests` (6) + `UpdateIssue` (5), no padding per ceiling rule |

### Auth strategy (shared ceiling)

Auth failures are one uniform middleware: `AuthNegativeTests` clusters invalid / missing / malformed `Authorization` on key routes, and read fixtures keep their own `*_InvalidToken_*` where the floor needed it. Expanding 401s to every route adds no new failure class — ceiling rule: *a uniform GitHub auth middleware doesn't need a 401 test on every route*.

### Method (reproducible for any future batch)

1. Count `Category("Negative")` per fixture file; list those below 5.
2. For each, read the OB section's Negative list + Validation Rules (path segments, headers, query params).
3. **Live-probe every uncertain mode before writing a test** (read-only GET/PUT-to-missing-resource; batch-1 and batch-2 probes both corrected wrong OB claims — invalid pulls `state`, `since` beyond 1e9).
4. Add tests until 5 or until modes are exhausted; write the ceiling line into the OB section for the remainder.
5. Refresh this table and the counters above.

### JsonPlaceholder

Ceilings live in `JsonPlaceholderTestPlan.md` → Negative Floor Status (closed 2026-10-02: mock-API exception for GetAll ×6, filter rationale ×7).
