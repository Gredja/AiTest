# GitHub Test Plan

**Project:** Gredja (.NET 10.0)
**API Under Test:** GitHub REST API v3 (https://api.github.com)
**Sandbox:** Gredja/AiTest
**Observable Behaviour:** [GitHubObservableBehaviour.md](GitHubObservableBehaviour.md)
**Scope, Top-3 risks, Entry/Exit criteria:** [GitHubTestingStructure.md](GitHubTestingStructure.md)

**Status legend:** `Done` — tests implemented and passing · `TODO` — designed (see Phase 1 Test Design), not yet implemented

---

## Phase 1: Full API Testing (read-only)

### 1.1 Repositories
| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.1.1 | GET /repos/{owner}/{repo} | Status, contract, fields, Content-Type, response time, 404 for non-existent | Done |
| 1.1.2 | GET /repositories | Status, contract, pagination | Done |
| 1.1.3 | GET /user/repos | Auth, contract, pagination | Done |

### 1.2 Issues
| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.2.1 | GET /repos/{owner}/{repo}/issues | Status, contract, fields, pagination, state filter | Done |
| 1.2.2 | GET /repos/{owner}/{repo}/issues/{issue_number} | Status, contract, fields | Done |
| 1.2.3 | GET /repos/{owner}/{repo}/issues/{issue_number}/comments | Status, contract, fields | Done |

### 1.3 Pull Requests
| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.3.1 | GET /repos/{owner}/{repo}/pulls | Status, contract, fields, pagination, state filter | Done |
| 1.3.2 | GET /repos/{owner}/{repo}/pulls/{pull_number}/files | Status, contract, fields, negatives (design: D1) | TODO |
| 1.3.3 | GET /repos/{owner}/{repo}/pulls/{pull_number}/commits | Status, contract, fields, negatives (design: D2) | TODO |

### 1.4 Branches
| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.4.1 | GET /repos/{owner}/{repo}/branches | Status, contract, fields | Done |
| 1.4.2 | GET /repos/{owner}/{repo}/branches/{branch} | Status, contract, fields, negatives (design: D3) | TODO |

### 1.5 Users
| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.5.1 | GET /users/{username} | Status, contract, fields | Done |
| 1.5.2 | GET /users/{username}/repos | Status, contract, pagination | Done |

### 1.6 Misc
| # | Endpoint | Tests | Status |
|---|----------|-------|--------|
| 1.6.1 | GET /rate_limit | Status, contract, remaining > 0, reset is future, sections exist | Done |
| 1.6.2 | GET /repos/{owner}/{repo}/contributors | Status, contract, fields, negatives (design: D4) | TODO |
| 1.6.3 | GET /repos/{owner}/{repo}/languages | Status, contract, negatives (design: D5) | TODO |
| 1.6.4 | GET /repos/{owner}/{repo}/topics | Status, contract, negatives (design: D6) | TODO |
| 1.6.5 | GET /repos/{owner}/{repo}/tags | Status, contract, negatives (design: D7) | TODO |

### Test Categories
- `HealthCheck` — endpoint returns 200
- `ContractCheck` — response matches expected model (JSON round-trip + attribute validation)
- `Regression` — valid fields, data integrity
- `Smoke` — Content-Type, pagination, filters
- `Negative` — 404 for non-existent resources, 401 for invalid token
- `Performance` — response time < 5s (list endpoints only)

---

## Phase 1 — TODO Endpoints: Test Design

Seed methodology per `Rules/test-practices.md`: 5 seeds → expand → enforce 5+ negatives per endpoint.
Priority: `1` = must have, `2` = nice to have.

> **Source of truth:** these tables are a test inventory (case → category → priority) derived from `GitHubObservableBehaviour.md` sections 9a, 9b, 11–15. If API behavior changes, update OB first, then adjust the tables here.

### Verified Sandbox Data (source: live GitHub API)

| Resource | State |
|---|---|
| PRs | #1–#5, all closed; PR #5 has files and commits → existing PR = dynamic max, non-existent = max + 1 |
| Branches | `main` (default), `features/GitHub-API-Integration` |
| Languages | `C#` (63056), `PowerShell` (4305) |
| Topics | `names: []` (empty) |
| Tags | `[]` (empty) |
| Contributors | `Gredja` (contributions = 18) |

Verified negative behavior: PR `0` / `-1` / `99999` → 404 · non-existent branch (incl. with spaces) → 404 · non-existent owner → 404 · invalid token → 401 · `per_page=abc` → 200 (silently ignored).

### D1. GET /repos/{owner}/{repo}/pulls/{pull_number}/files

| # | Case | Category | Priority |
|---|------|----------|----------|
| 1 | Existing PR (dynamic max) → 200 | HealthCheck | 1 |
| 2 | Response matches `PullRequestFileModelResponse` contract | ContractCheck | 1 |
| 3 | Fields: `sha` 40 hex chars, `filename` non-empty, `status` valid | Regression | 1 |
| 4 | Content-Type is application/json | Smoke | 2 |
| 5 | Non-empty list (PR has changed files) | Smoke | 1 |
| 6 | `filename` values unique within PR | Regression | 2 |
| 7 | `additions` / `deletions` / `changes` ≥ 0 | Regression | 2 |
| 8 | Response time < 5s | Performance | 2 |
| 9 | Non-existent PR (max + 1, dynamic) → 404 | Negative | 1 |
| 10 | PR number 0 → 404 | Negative | 1 |
| 11 | Negative PR number (-1) → 404 | Negative | 1 |
| 12 | Non-existent repo → 404 | Negative | 1 |
| 13 | Non-existent owner → 404 | Negative | 2 |
| 14 | Invalid token → 401 | Negative | 2 |

### D2. GET /repos/{owner}/{repo}/pulls/{pull_number}/commits

| # | Case | Category | Priority |
|---|------|----------|----------|
| 1 | Existing PR (dynamic max) → 200 | HealthCheck | 1 |
| 2 | Response matches `PullRequestCommitModelResponse` contract | ContractCheck | 1 |
| 3 | Fields: `sha` 40 hex chars, unique per list | Regression | 1 |
| 4 | `commit.message` non-empty | Regression | 1 |
| 5 | Content-Type is application/json | Smoke | 2 |
| 6 | Non-empty list (PR has commits) | Smoke | 1 |
| 7 | `html_url` starts with https://github.com/ | Regression | 2 |
| 8 | Response time < 5s | Performance | 2 |
| 9 | Non-existent PR (max + 1, dynamic) → 404 | Negative | 1 |
| 10 | PR number 0 → 404 | Negative | 1 |
| 11 | Negative PR number (-1) → 404 | Negative | 1 |
| 12 | Non-existent repo → 404 | Negative | 1 |
| 13 | Non-existent owner → 404 | Negative | 2 |
| 14 | Invalid token → 401 | Negative | 2 |

### D3. GET /repos/{owner}/{repo}/branches/{branch}

| # | Case | Category | Priority |
|---|------|----------|----------|
| 1 | Existing branch (`main`) → 200 | HealthCheck | 1 |
| 2 | Response matches `BranchModelResponse` contract | ContractCheck | 1 |
| 3 | `name` equals requested branch | Smoke | 1 |
| 4 | `commit.sha` is 40 hex chars | Regression | 1 |
| 5 | Content-Type is application/json | Smoke | 2 |
| 6 | Edge: branch name with slash (`features/GitHub-API-Integration`) → 200, `name` matches URL segment | Smoke | 2 |
| 7 | Non-existent branch → 404 | Negative | 1 |
| 8 | Branch name with spaces (URL-encoded) → 404 | Negative | 2 |
| 9 | Non-existent repo → 404 | Negative | 1 |
| 10 | Non-existent owner → 404 | Negative | 2 |
| 11 | Invalid token → 401 | Negative | 2 |

### D4. GET /repos/{owner}/{repo}/contributors

| # | Case | Category | Priority |
|---|------|----------|----------|
| 1 | → 200 | HealthCheck | 1 |
| 2 | Response matches `ContributorModelResponse` contract | ContractCheck | 1 |
| 3 | Fields: `login` non-empty, `id` > 0, `contributions` > 0 | Regression | 1 |
| 4 | Content-Type is application/json | Smoke | 2 |
| 5 | Non-empty list (repo has contributor) | Smoke | 1 |
| 6 | Items sorted by `contributions` descending | Regression | 2 |
| 7 | `login` values unique | Regression | 2 |
| 8 | Response time < 5s | Performance | 2 |
| 9 | Edge: `per_page=1` → at most 1 item | Smoke | 2 |
| 10 | Non-existent repo → 404 | Negative | 1 |
| 11 | Non-existent owner → 404 | Negative | 1 |
| 12 | Invalid token → 401 | Negative | 2 |
| 13 | Owner with special chars → 404 | Negative | 2 |
| 14 | Repo with special chars → 404 | Negative | 2 |

### D5. GET /repos/{owner}/{repo}/languages

| # | Case | Category | Priority |
|---|------|----------|----------|
| 1 | → 200 | HealthCheck | 1 |
| 2 | Response parses into `Dictionary<string, long>` (keys = languages, values = bytes) | ContractCheck | 1 |
| 3 | Content-Type is application/json | Smoke | 2 |
| 4 | Non-empty: repo has ≥ 1 language | Smoke | 1 |
| 5 | All values (bytes) > 0 | Regression | 1 |
| 6 | All keys (language names) non-empty | Regression | 2 |
| 7 | Edge: `per_page=abc` → 200 (silently ignored, documented behavior) | Smoke | 2 |
| 8 | Non-existent repo → 404 | Negative | 1 |
| 9 | Non-existent owner → 404 | Negative | 1 |
| 10 | Invalid token → 401 | Negative | 2 |
| 11 | Owner with special chars → 404 | Negative | 2 |
| 12 | Repo with special chars → 404 | Negative | 2 |

### D6. GET /repos/{owner}/{repo}/topics

| # | Case | Category | Priority |
|---|------|----------|----------|
| 1 | → 200 with `Accept: application/vnd.github.mercy-preview+json` | HealthCheck | 1 |
| 2 | Response matches `TopicsModelResponse` contract (`names` array) | ContractCheck | 1 |
| 3 | All names match `^[a-z0-9-]+$` (lowercase, hyphens) | Regression | 1 |
| 4 | `names` values unique | Regression | 2 |
| 5 | Content-Type is application/json | Smoke | 2 |
| 6 | Edge: repo without topics → 200, `names` = `[]` (documented) | Smoke | 2 |
| 7 | Non-existent repo → 404 | Negative | 1 |
| 8 | Non-existent owner → 404 | Negative | 1 |
| 9 | Invalid token → 401 | Negative | 2 |
| 10 | Owner with special chars → 404 | Negative | 2 |
| 11 | Repo with special chars → 404 | Negative | 2 |

### D7. GET /repos/{owner}/{repo}/tags

| # | Case | Category | Priority |
|---|------|----------|----------|
| 1 | → 200 | HealthCheck | 1 |
| 2 | Response matches `TagModelResponse` contract (items without top-level `id`) | ContractCheck | 1 |
| 3 | Every item: `name` non-empty, `commit.sha` 40 hex chars | Regression | 1 |
| 4 | Content-Type is application/json | Smoke | 2 |
| 5 | `name` values unique | Regression | 2 |
| 6 | Response time < 5s | Performance | 2 |
| 7 | Edge: repo without tags → 200, `[]` (documented) | Smoke | 2 |
| 8 | Non-existent repo → 404 | Negative | 1 |
| 9 | Non-existent owner → 404 | Negative | 1 |
| 10 | Invalid token → 401 | Negative | 2 |
| 11 | Owner with special chars → 404 | Negative | 2 |
| 12 | Repo with special chars → 404 | Negative | 2 |

**Design totals:** 88 tests — 51 positive/edge, 37 negative (every endpoint meets the 5+ negative floor).

---

## Phase 2: E2E Testing (write + chains)

> E2E tests are in a separate project `E2E/` and use the sandbox repo `Gredja/AiTest`.

| # | Scenario | Steps | Status |
|---|----------|-------|--------|
| 2.1 | **Issue Lifecycle** | Create Issue -> Add Comment -> Verify comment linked -> Close Issue -> Verify state | TODO |
| 2.2 | **Pull Request Flow** | Create Branch -> Create PR -> Verify PR linked to Issue -> Merge PR -> Verify issue closed | TODO |
| 2.3 | **Comment Chain** | Create Issue -> Add multiple comments -> Verify comment count -> Verify ordering | TODO |
| 2.4 | **Repository Health** | Verify default branch, topics, visibility, branch protection rules | TODO |
| 2.5 | **Collaborators & Permissions** | Verify collaborator access levels, team permissions | TODO |
| 2.6 | **Rate Limit Drain** | Consume rate limit calls -> Verify remaining decrements -> Verify reset time | TODO |

---

## Implementation Scope (Phase 1 TODO)

**New files:**
- Models: `Core/Models/GitHub/PullRequestFileModelResponse.cs`, `PullRequestCommitModelResponse.cs`, `TopicsModelResponse.cs`
- Tests: `Api/GitHub/Tests/PullRequests/GetPullRequestFilesTests.cs`, `GetPullRequestCommitsTests.cs`, `Api/GitHub/Tests/Branches/GetBranchByNameTests.cs`, `Api/GitHub/Tests/Contributors/GetContributorsTests.cs`, `Api/GitHub/Tests/Languages/GetLanguagesTests.cs`, `Api/GitHub/Tests/Topics/GetTopicsTests.cs`, `Api/GitHub/Tests/Tags/GetTagsTests.cs`

**Modified files:**
- `Core/Config/GitHubEndpoints.cs` — add `NonExistentBranchName`, `DefaultBranch`, `MercyPreviewAccept` (no magic strings in tests)
- `documentation/GitHubObservableBehaviour.md` — ✅ done: added sections 9a (`GET /pulls/{n}/files`) and 9b (`GET /pulls/{n}/commits`), extended Data Dependencies with PR #5 / branch `main`
- `documentation/GitHubTestPlan.md` — flip 7 statuses TODO → Done after tests pass

**Safety check:** `dotnet format --verify-no-changes` + `dotnet test` — both must pass.

---

## Key Decisions

- **Sandbox repo:** `Gredja/AiTest` for all write operations
- **E2E as separate project:** isolation from read-only tests, different risk profile
- **Cleanup:** E2E tests clean up after themselves (delete created resources)
- **Rate limiting:** E2E tests respect GitHub API limits (5000 req/hour with token)
- **Dynamic PR numbers:** `[OneTimeSetUp]` fetches `GET /pulls?state=all` → existing = max, non-existent = max + 1 (no hardcoded 99999)
- **Models without `Id`:** GitHub PR files/commits/tags have no top-level `id` → new models do not inherit `IdModel<T>` (`[PositiveId]` would fail contract checks)
- **Empty collections:** topics/tags are empty in sandbox → assert 200 + `OnlyContain(...)` (vacuously true on empty, validates fields once data appears); documented behavior, not a skip
- **`per_page=abc` → 200:** GitHub silently ignores invalid query params — asserted as documented edge behavior
- **Invalid-token negatives (401):** `RequestHelper` auto-adds the valid `Authorization` header; the test-level invalid header is added after — verify actual status on first run; if headers collide and status ≠ 401, replace that negative with another failure mode and document it here
