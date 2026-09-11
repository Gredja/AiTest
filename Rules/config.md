# Rules: Config

## Endpoints

- Base URL and all endpoint paths stored in `Core/Config/FakeStoreEndpoints.cs`, `Core/Config/JsonPlaceholderEndpoints.cs`, and `Core/Config/GitHubEndpoints.cs`
- Never hardcode URLs or paths in tests — use constants from service-specific Endpoints classes
