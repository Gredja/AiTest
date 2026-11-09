# Output Format Instruction (report contract)

Model must return: test files (.cs), models (.cs), and constants (.cs) in C# format with namespace, class, methods. Format — code without comments.

After generation — return ONE JSON object as the FIRST part of the reply, no surrounding prose before it:

```json
{
  "files": {
    "created": ["path/to/NewTests.cs"],
    "modified": ["path/to/Existing.cs"]
  },
  "tests": {
    "total": 0,
    "positive": 0,
    "negative": 0,
    "edge": 0,
    "ignored": 0
  },
  "safetyCheck": {
    "format": "pass",
    "testRun": { "filter": "GetAll{Endpoint}", "passed": 0, "failed": 0 },
    "issues": []
  },
  "suggestedCommitMessage": "Add {Service} {endpoint} API tests"
}
```

**Contract validation — the reply is invalid unless ALL hold:**
1. JSON parses; every key above is present; `safetyCheck.format` is exactly `pass` or `fail`.
2. `tests.total = positive + negative + edge + ignored` (counters are disjoint).
3. `safetyCheck.format = "pass"` AND `safetyCheck.testRun.failed = 0` — otherwise `suggestedCommitMessage` MUST be omitted (no commit suggestion after a red gate).
4. `files.created` lists only paths that did NOT exist before this run — extending an existing file goes to `files.modified`; never re-create an existing file (duplicate-file kata: `documentation/Katas/model-selection-note.md`).
5. `suggestedCommitMessage` matches `action + object` (capitalized verb + object, e.g. "Add GitHub issue API tests").
6. **Automated check (rules 1–3):** write the JSON to a temp file and run `powershell -File Scripts/validate-skill-report.ps1 <file>` — it must print `PASS` (exit 0). On `FAIL`: fix the JSON and re-run; a report that fails the validator must not be sent.

A human-readable summary MAY follow the JSON; the JSON is the deliverable, the prose is optional.
