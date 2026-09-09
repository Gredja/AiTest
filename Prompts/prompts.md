# Gredja — Prompts Library

Reusable prompt templates for the Gredja project.
Each prompt is versioned, reviewable, and reusable — an artefact, not a chat.

---

## Prompts: API Test Generation

> Generate a full set of NUnit API test cases for the {endpoint} endpoint of FakeStoreAPI.
> Include: status code validation, response structure checks, edge cases (empty body, invalid ID, missing fields).
> Use RestSharp for HTTP calls. Output as a single C# test class with [Test] methods.

## Prompts: Model Generation

> Given this JSON response: {json}
> Generate a C# model class with the suffix "Model", nullable properties, no default initializations.
> One class per file. Namespace: Core.Models.
