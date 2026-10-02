using System.Text.Json;

namespace Core.Logging;

internal static class TestOutcomeReader
{
    internal static List<TestOutcome> Read(IReadOnlyCollection<string> logPaths)
    {
        var outcomes = new List<TestOutcome>();

        foreach (var logPath in logPaths)
        {
            foreach (var line in SharedFile.ReadLines(logPath))
            {
                var outcome = Parse(line);
                if (outcome is not null)
                {
                    outcomes.Add(outcome);
                }
            }
        }

        return outcomes;
    }

    private static TestOutcome? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;

            return new TestOutcome(
                ReadString(root, "test"),
                ReadString(root, "status"),
                ReadInt64(root, "durationMs"),
                ReadString(root, "message"),
                ReadString(root, "trace"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ReadString(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static long ReadInt64(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt64()
            : 0;
}
