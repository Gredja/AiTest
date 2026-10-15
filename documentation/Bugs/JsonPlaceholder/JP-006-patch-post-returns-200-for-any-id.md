## Bug: PATCH /posts/{id} returns 200 for any ID — 0, -1, non-existent, non-numeric

### Steps to Reproduce
1. PATCH /posts/0 with body `{"title": "t"}`
2. Repeat for /posts/-1, /posts/101 (non-existent), /posts/abc (non-numeric)

### Expected Result
400 Bad Request for invalid/non-numeric IDs; 404 Not Found for non-existent IDs

### Actual Result
200 OK for every ID variant — mock routes any `/posts/{id}` PATCH to the same handler

### Severity: (pending — human judgement, per BugReportTemplate)
### Priority: (pending — human judgement, per BugReportTemplate)

### Affected Tests
- `UpdatePostTests.cs` — `[Ignore]` tests `UpdatePost_Patch_ZeroId_ReturnsNotFound`, `UpdatePost_Patch_NonExistentId_ReturnsNotFound`, `UpdatePost_Patch_InvalidSegment_ReturnsNotFound`
