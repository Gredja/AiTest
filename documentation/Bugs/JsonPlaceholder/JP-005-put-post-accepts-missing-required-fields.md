## Bug: PUT /posts/{id} accepts body missing required fields

### Steps to Reproduce
1. PUT /posts/1 with body `{"title": "t", "userId": 1}` (no `body` field)
2. Repeat omitting `title`, then omitting `userId`
3. Observe response for each variant

### Expected Result
400 Bad Request — PUT replaces the whole resource, all required fields must be present

### Actual Result
200 OK for all three variants — mock performs no body validation

### Severity: (pending — human judgement, per BugReportTemplate)
### Priority: (pending — human judgement, per BugReportTemplate)

### Affected Tests
- `UpdatePostTests.cs` — `[Ignore]` tests `UpdatePost_Put_MissingTitle_ReturnsBadRequest`, `UpdatePost_Put_MissingBody_ReturnsBadRequest`, `UpdatePost_Put_MissingUserId_ReturnsBadRequest`
