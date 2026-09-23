## Bug: GET /products and GET /users return 200 OK for invalid query parameters

### Steps to Reproduce
1. GET /products?invalid=true
2. GET /users?invalid=true

### Expected Result
400 Bad Request

### Actual Result
200 OK with the full list of items (invalid params ignored)

### Severity: 4
### Priority: 4

### Affected Tests
- `GetAllProductsTests.cs` — `[Ignore]` test `GetAllProducts_InvalidQueryParam_ReturnsBadRequest`
- `GetAllUsersTests.cs` — `[Ignore]` test `GetAllUsers_InvalidQueryParam_ReturnsBadRequest`
