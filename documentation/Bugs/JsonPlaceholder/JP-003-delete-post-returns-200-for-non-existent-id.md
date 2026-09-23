## Bug: DELETE /posts/{id} returns 200 for non-existent ID

### Steps to Reproduce
1. DELETE /posts/999999 — resource does not exist
2. Observe response status

### Expected Result
404 Not Found — resource does not exist

### Actual Result
200 OK with empty body

### Severity: 4
### Priority: 4

### Affected Tests
- `DeletePostTests.cs` — `[Ignore]` test `DeletePost_NonExistentId_ReturnsError`
