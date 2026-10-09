namespace AllureAdapter.Helpers;

internal record TestResultParams(
    string Uuid,
    string FullName,
    string Name,
    long StartMilliseconds,
    long StopMilliseconds,
    string Status,
    string? StatusMessage = null,
    string? StatusTrace = null,
    string? Description = null,
    string? TestClassName = null,
    List<string>? Categories = null);
