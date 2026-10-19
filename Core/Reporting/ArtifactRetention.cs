using Core.Config;
using Core.Logging;

namespace Core.Reporting;

internal static class ArtifactRetention
{
    private const int RetentionDays = 7;

    internal static void CleanupAccumulated(string? root)
    {
        if (root is null)
        {
            return;
        }

        DeleteOlderThan(Path.Combine(root, TestConfig.AllureResultsDirName));
        DeleteOlderThan(Path.Combine(root, TestConfig.ResultsDirName));
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
