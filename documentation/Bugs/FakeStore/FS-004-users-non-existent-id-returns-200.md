## Bug: GET /users/{id} returns 200 OK for non-existent user ID

### Steps to Reproduce
1. GET /users — extract maxId from response
2. GET /users/{maxId + 1}

### Expected Result
404 Not Found

### Actual Result
200 OK with a user object (null fields or default values)

### Severity: 3
### Priority: 3

### Affected Tests
- `GetUserByIdTests.cs` — `[Ignore]` test `GetUserById_NonExistentId_ReturnsNotFound`
