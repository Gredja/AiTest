## Bug: GET /users/0 returns 200 OK instead of 404

### Steps to Reproduce
1. GET /users/0

### Expected Result
404 Not Found

### Actual Result
200 OK with a user object

### Severity: 3
### Priority: 3

### Affected Tests
- `GetUserByIdTests.cs` — `[Ignore]` test `GetUserById_ZeroId_ReturnsNotFound`
