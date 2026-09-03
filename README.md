# Gredja

Test application for learning AI-Native SDLC. Project for API (FakeStoreAPI) and UI (Playwright) test automation.

## Technologies

- .NET 10.0
- NUnit 4.6.1
- RestSharp 114.0.0 (API tests)
- Microsoft.Playwright 1.62.0 (UI tests)
- Graphify 0.9.53 (knowledge graph)

## API Under Test

FakeStoreAPI — https://fakestoreapi.com

Resources:
- Products — products (GET, POST, PUT, DELETE)
- Carts — carts
- Users — users
- Auth — authentication (JWT)

## Running Tests

```bash
dotnet test Api/Api.csproj
dotnet test Ui/Ui.csproj
```

## Deadline

2026-12-31
