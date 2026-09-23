## Bug: GET /carts/{id} returns 200 OK for negative cart ID

### Steps to Reproduce
1. GET /carts/-1

### Expected Result
404 Not Found

### Actual Result
200 OK with a cart object

### Severity: 3
### Priority: 3

### Affected Tests
- `GetCartByIdTests.cs` — `[Ignore]` test `GetCartById_NegativeId_ReturnsNotFound`
