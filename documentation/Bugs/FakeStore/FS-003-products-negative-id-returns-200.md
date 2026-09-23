## Bug: GET /products/{id} returns 200 OK for negative product ID

### Steps to Reproduce
1. GET /products/-1

### Expected Result
404 Not Found

### Actual Result
200 OK with a product object

### Severity: 3
### Priority: 3

### Affected Tests
- `GetProductByIdTests.cs` — `[Ignore]` test `GetProductById_NegativeId_ReturnsNotFound`
