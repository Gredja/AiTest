# Rules: Config

## Endpoints

- Base URL and all endpoint paths stored in `Core/Config/JsonPlaceholderEndpoints.cs` and `Core/Config/GitHubEndpoints.cs`
- Never hardcode URLs or paths in tests — use constants from service-specific Endpoints classes

## Settings files

- `testsettings.json` — local config, gitignored; `testsettings.example.json` — tracked template, must stay in sync: when a key is added/renamed in `testsettings.json`, mirror it in the example in the same change (a clean clone builds and runs from the example alone)
- GitHub token precedence (`TestConfig.GitHubToken`): `GitHub.Token` in testsettings.json → environment variable `GITHUB_PAT` (CI) → `GITHUB_PAT=` line in `.env` (dev)
- Never put real tokens in `testsettings.example.json` or any tracked file
