## Bug: DELETE /posts/{id} returns 200 but does not actually delete the resource

### Steps to Reproduce
1. GET /posts/1 — verify resource exists
2. DELETE /posts/1 — observe 200 response
3. GET /posts/1 — resource still exists

### Expected Result
200 OK, then GET returns 404 Not Found

### Actual Result
200 OK, then GET still returns the resource with all data intact

### Severity: 3
### Priority: 3

### Affected Tests
- `DeletePostTests.cs` — `[Ignore]` test `DeletePost_DeletedPostReturnsNotFound`
