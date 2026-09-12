# GitHub Test Plan

**Project:** Gredja (.NET 10.0)
**API Under Test:** GitHub REST API v3 (https://api.github.com)
**Sandbox:** Gredja/GitHubApiTests

---

## Phase 1: Full API Testing (read-only)

### 1.1 Repositories
| # | Endpoint | Tests |
|---|----------|-------|
| 1.1.1 | GET /repos/{owner}/{repo} | Status, fields, Content-Type, response time, 404 for non-existent |
| 1.1.2 | GET /repositories | Status, pagination |
| 1.1.3 | GET /user/repos | Auth required, pagination |

### 1.2 Issues
| # | Endpoint | Tests |
|---|----------|-------|
| 1.2.1 | GET /repos/{owner}/{repo}/issues | Status, fields, pagination, state filter |
| 1.2.2 | GET /repos/{owner}/{repo}/issues/{issue_number} | Status, fields |
| 1.2.3 | GET /repos/{owner}/{repo}/issues/{issue_number}/comments | Status, fields |

### 1.3 Pull Requests
| # | Endpoint | Tests |
|---|----------|-------|
| 1.3.1 | GET /repos/{owner}/{repo}/pulls | Status, fields, pagination, state filter |
| 1.3.2 | GET /repos/{owner}/{repo}/pulls/{pull_number}/files | Status |
| 1.3.3 | GET /repos/{owner}/{repo}/pulls/{pull_number}/commits | Status |

### 1.4 Branches
| # | Endpoint | Tests |
|---|----------|-------|
| 1.4.1 | GET /repos/{owner}/{repo}/branches | Status, fields |
| 1.4.2 | GET /repos/{owner}/{repo}/branches/{branch} | Status, fields |

### 1.5 Users
| # | Endpoint | Tests |
|---|----------|-------|
| 1.5.1 | GET /users/{username} | Status, fields |
| 1.5.2 | GET /users/{username}/repos | Status, pagination |

### 1.6 Misc
| # | Endpoint | Tests |
|---|----------|-------|
| 1.6.1 | GET /rate_limit | Status, remaining > 0, reset is future, sections exist |
| 1.6.2 | GET /repos/{owner}/{repo}/contributors | Status, fields |
| 1.6.3 | GET /repos/{owner}/{repo}/languages | Status |
| 1.6.4 | GET /repos/{owner}/{repo}/topics | Status |
| 1.6.5 | GET /repos/{owner}/{repo}/tags | Status |

### Test Categories
- `HealthCheck` — endpoint returns 200
- `Regression` — valid fields, data integrity
- `Smoke` — Content-Type, pagination, filters
- `Negative` — 404 for non-existent resources
- `Performance` — response time < 5s

---

## Phase 2: E2E Testing (write + chains)

> E2E tests are in a separate project `Api.E2E/` and use the sandbox repo `Gredja/GitHubApiTests`.

### 2.1 Issue Lifecycle
Create Issue -> Add Comment -> Verify comment linked -> Close Issue -> Verify state

### 2.2 Pull Request Flow
Create Branch -> Create PR -> Verify PR linked to Issue -> Merge PR -> Verify issue closed

### 2.3 Comment Chain
Create Issue -> Add multiple comments -> Verify comment count -> Verify ordering

### 2.4 Repository Health (config as infrastructure)
Verify default branch, topics, visibility, branch protection rules

### 2.5 Collaborators & Permissions
Verify collaborator access levels, team permissions

### 2.6 Rate Limit Drain
Consume rate limit calls -> Verify remaining decrements -> Verify reset time

---

## Key Decisions

- **Sandbox repo:** `Gredja/GitHubApiTests` for all write operations
- **E2E as separate project:** isolation from read-only tests, different risk profile
- **Cleanup:** E2E tests clean up after themselves (delete created resources)
- **Rate limiting:** E2E tests respect GitHub API limits (5000 req/hour with token)
