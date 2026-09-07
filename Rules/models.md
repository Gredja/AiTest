# Rules: Models

## Types by purpose

### Response models — suffix `Model`

Describe JSON that the API **returns**. Used for deserialization of API responses.

- Always includes `Id` field (server-generated)
- May include nested models (e.g. `RatingModel` inside `ProductModel`)
- Match the full JSON structure from the API response

### Request models — suffix `Request`

Describe JSON that we **send** to the API. Used as request body for POST/PUT.

- No `Id` field (server generates it)
- Only fields the client must provide
- For PUT — may include `Id` if the endpoint expects it in the body

## Property rules

| Type category | Rule | Example |
|---------------|------|---------|
| **Reference** (string, object, List, arrays) | No `?`, no initialization | `public string Title { get; set; }` |
| **Value** (int, decimal, double, DateTime) | Add `?` only if JSON field can be null or absent | `public DateTime? Date { get; set; }` |

How to decide for value types: check the actual JSON response from the API. If the field can be null or absent — make it nullable. If it always has a value (even if 0) — keep it non-nullable.

## Naming

- Model suffix for responses, Request suffix for requests
- No abbreviations in class names

## Namespaces

- Regular models: `Core.Models`
- Reusable generics: `Core.Models.Generic`

## Generic models

For reusable structures shared across multiple entities — place in `Core/Models/Generic/`.

Example: `IdNameModel<T>` for any entity with `Id` + `Name`.

## Nested models

When an entity contains another entity as a field — create a separate model for the nested part.

Example: `ProductModel` has `RatingModel Rating` — not inline anonymous type.

## What NOT to do

- Don't add constructors unless needed for deserialization
- Don't add validation attributes (validation is in tests, not in models)
- Don't add methods or logic — models are pure data containers
- Don't use `init` — use `{ get; set; }` for all properties

## Attributes

Place attributes on a separate line above the property, not on the same line:

```csharp
// Correct
[RequiredField]
public string Title { get; set; }

// Wrong
[RequiredField] public string Title { get; set; }
```
