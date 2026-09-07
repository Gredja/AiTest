using System.Text.Json;

namespace TestAdapter.Helpers;

public static class AllureJsonWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public static void WriteResultFile(string resultsDir, Dictionary<string, object> testResult)
    {
        var fileName = $"{Guid.NewGuid():N}-result.json";
        var filePath = Path.Combine(resultsDir, fileName);
        File.WriteAllText(filePath, JsonSerializer.Serialize(testResult, JsonOptions));
    }

    public static void WriteContainerFile(string resultsDir, string uuid, Dictionary<string, object> container)
    {
        var fileName = $"{uuid}-container.json";
        var filePath = Path.Combine(resultsDir, fileName);
        File.WriteAllText(filePath, JsonSerializer.Serialize(container, JsonOptions));
    }
}
