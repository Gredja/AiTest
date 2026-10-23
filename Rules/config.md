# Rules: Config

## Endpoints

- Base URL and all endpoint paths stored in `Core/Config/JsonPlaceholderEndpoints.cs`, `Core/Config/GitHubEndpoints.cs` and `Core/Config/MoodledemoEndpoints.cs`
- Never hardcode URLs or paths in tests — use constants from service-specific Endpoints classes

## Settings files

- `testsettings.json` — local config, gitignored; `testsettings.example.json` — tracked template, must stay in sync: when a key is added/renamed in `testsettings.json`, mirror it in the example in the same change (a clean clone builds and runs from the example alone)
- GitHub token precedence (`TestConfig.GitHubToken`): `GitHub.Token` in testsettings.json → environment variable `GITHUB_PAT` (CI) → `GITHUB_PAT=` line in `.env` (dev)
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
