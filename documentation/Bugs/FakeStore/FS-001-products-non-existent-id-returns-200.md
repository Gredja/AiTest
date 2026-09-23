## Bug: GET /products/{id} returns 200 OK for non-existent product ID

### Steps to Reproduce
1. GET /products — extract maxId from response
2. GET /products/{maxId + 1}

### Expected Result
404 Not Found

### Actual Result
200 OK with a product object (null fields or default values)

### Severity: 3
### Priority: 3

### Affected Tests
- `GetProductByIdTests.cs` — `[Ignore]` test `GetProductById_NonExistentId_ReturnsNotFound`
