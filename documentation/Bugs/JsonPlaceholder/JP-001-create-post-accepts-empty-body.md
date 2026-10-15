## Bug: POST /posts accepts empty request body and returns 201

### Steps to Reproduce
1. POST /posts with empty JSON body `{}`
2. Observe response status

### Expected Result
400 Bad Request — missing required fields (title, body, userId)

### Actual Result
201 Created with generated ID and null fields

### Severity: 3
### Priority: 3

### Affected Tests
- `CreatePostTests.cs` — `[Ignore]` test `CreatePost_EmptyBody_ReturnsError`
