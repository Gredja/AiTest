## Bug: GET /users/{id} returns 200 OK for negative user ID

### Steps to Reproduce
1. GET /users/-1

### Expected Result
404 Not Found

### Actual Result
200 OK with a user object

### Severity: 3
### Priority: 3

### Affected Tests
- `GetUserByIdTests.cs` — `[Ignore]` test `GetUserById_NegativeId_ReturnsNotFound`
