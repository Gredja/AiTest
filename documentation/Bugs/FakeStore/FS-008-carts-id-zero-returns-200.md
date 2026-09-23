## Bug: GET /carts/0 returns 200 OK instead of 404

### Steps to Reproduce
1. GET /carts/0

### Expected Result
404 Not Found

### Actual Result
200 OK with a cart object

### Severity: 3
### Priority: 3

### Affected Tests
- `GetCartByIdTests.cs` — `[Ignore]` test `GetCartById_ZeroId_ReturnsNotFound`
