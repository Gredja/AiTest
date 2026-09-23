## Bug: GET /carts/{id} returns 200 OK for non-existent cart ID

### Steps to Reproduce
1. GET /carts — extract maxId from response
2. GET /carts/{maxId + 1}

### Expected Result
404 Not Found

### Actual Result
200 OK with a cart object (null fields or default values)

### Severity: 3
### Priority: 3

### Affected Tests
- `GetCartByIdTests.cs` — `[Ignore]` test `GetCartById_NonExistentId_ReturnsNotFound`
