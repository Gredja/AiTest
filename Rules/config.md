# Rules: Config

## Endpoints

- Base URL and all endpoint paths stored in `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs` and `Core/Config/MoodledemoEndpoints.cs`
- Never hardcode URLs or paths in tests — use constants from service-specific Endpoints classes

## Settings files

- `testsettings.json` — local config, gitignored; `testsettings.example.json` — tracked template, must stay in sync: when a key is added/renamed in `testsettings.json`, mirror it in the example in the same change (a clean clone builds and runs from the example alone)
- GitHub token precedence (`TestConfig.GitHubToken`): `GitHub.Token` in testsettings.json → environment variable `GITHUB_PAT`
- Secrets live in process environment variables only — no `.env` file in the repo (removed 2026-11-15) and **no local persistence** (the user-scope `setx` cache was removed 2026-11-16 — secrets must never sit on disk unencrypted; user decision). Source of truth: **Infisical** (project `Gredja`, env `dev`); every shell/session that needs secrets runs `Scripts/sync-secrets.ps1` (pull → `$env` of the current process only, no setx/registry/files; output lists key names only, never values), combined with the command in the same invocation; at session start the agent itself runs it when `GITHUB_PAT` is absent from the environment (AGENTS.md → Workflow → Session start). CI keeps repo secrets (GitHub Actions) — untouched. The Slack MCP server is spawned through the same load (`mimocode.jsonc` → `sync-secrets.ps1` → `server.mjs`)
- Never put real tokens in `testsettings.example.json` or any tracked file
- Required string keys throw a clear `InvalidOperationException` naming the missing key at first access (`TestConfig.GetRequiredString`) — never a bare `NullReferenceException` from `GetString()!`
- `SkipSslValidation` disables TLS certificate validation for the GitHub client (external audit Minor 13) — defaults to `false` (missing key = validation on); enable only as an explicit per-run opt-in for proxied dev runners, never in CI without a documented reason

## Moodle demo credentials

- `MoodleDemo.*` in `testsettings.json` / `testsettings.example.json` are the demo site's
  **publicly published** accounts (documented in `documentation/MoodleObservableBehaviour.md`,
  site resets hourly) — not real secrets; they may live in the tracked example on purpose

## GitHub identities

- Two identities, **do not merge**: API/E2E authenticate as `Gredja` (`GITHUB_PAT`, classic with `repo`) against `Gredja/AiTest`; UI tests authenticate as `gredjapl` (`GITHUB_UI_PAT`, fine-grained) against `gredjapl/ui-sandbox`
- Why they stay apart: gredjapl has read-only access to `Gredja/AiTest` (writes would 403), and a shared account would merge the 5000 req/h rate-limit quotas — `RateLimitDrainTests` would starve the UI suite (confirmed 2026-10-14)
- Git pushes of the project code use the git credential manager, never these tokens
