## Bug: GET /products/0 returns 200 OK instead of 404

### Steps to Reproduce
1. GET /products/0

### Expected Result
404 Not Found

### Actual Result
200 OK with a product object

### Severity: 3
### Priority: 3

### Affected Tests
- `GetProductByIdTests.cs` — `[Ignore]` test `GetProductById_ZeroId_ReturnsNotFound`
