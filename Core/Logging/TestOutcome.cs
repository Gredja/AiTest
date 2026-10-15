namespace Core.Logging;

internal sealed record TestOutcome(
    string Name,
    string Status,
    long DurationMilliseconds,
    string Message,
    string Trace);
