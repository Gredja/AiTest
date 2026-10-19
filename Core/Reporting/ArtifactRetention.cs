using Core.Logging;

namespace Core.Reporting;

internal static class ArtifactRetention
{
    internal const string ResultsDirName = "TestResults";
    internal const string AllureResultsDirName = "allure-results";

    private const int RetentionDays = 7;

    internal static void CleanupAccumulated(string? root)
    {
        if (root is null)
        {
            return;
        }

        DeleteOlderThan(Path.Combine(root, AllureResultsDirName));
        DeleteOlderThan(Path.Combine(root, ResultsDirName));
    }

    private static void DeleteOlderThan(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        var cutoff = DateTime.Now.AddDays(-RetentionDays);
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            if (File.GetLastWriteTime(file) < cutoff)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException exception)
                {
                    ActionLogger.Logger.Warning(exception, "Failed to delete old artifact {File}", file);
                }
                catch (UnauthorizedAccessException exception)
                {
                    ActionLogger.Logger.Warning(exception, "Failed to delete old artifact {File}", file);
                }
            }
        }
    }
}
