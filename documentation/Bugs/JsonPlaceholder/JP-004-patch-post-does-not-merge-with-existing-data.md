## Bug: PATCH /posts/{id} does not merge with existing data

### Steps to Reproduce
1. GET /posts/1 — verify original data (title, body, userId)
2. PATCH /posts/1 with `{"title": "updated"}`
3. GET /posts/1 — check response

### Expected Result
200 OK with merged data: updated title + original body + original userId

### Actual Result
200 OK with only `{"id": 1, "title": "updated"}` — body and userId are null/missing

### Severity: 3
### Priority: 3

### Affected Tests
- `UpdatePostTests.cs` — `[Ignore]` test `UpdatePost_Patch_OnlyChangesSpecifiedFields`
