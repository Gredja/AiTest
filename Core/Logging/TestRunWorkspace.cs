using Core.Reporting;

namespace Core.Logging;

public static class TestRunWorkspace
{
    internal const string MarkerFileName = ".run-start";
    internal const string ActionLogPrefix = "actions-";
    internal const string TestResultLogPrefix = "test-results-";

    // The marker is held open by the testhost that starts the run. A sibling testhost
    // (the second assembly of the same run) cannot open it for write and therefore
    // joins the run; when all testhosts exit the handle is released and the next run
    // takes ownership — this is what gives every run a fresh report without
    // distinguishing `dotnet test` from Visual Studio launches.
    private static FileStream? _runLock;

    public static string Resolve()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "GredjaTestRun");
        Directory.CreateDirectory(workspace);

        return workspace;
    }

    internal static void PrepareRun()
    {
        var workspace = Resolve();
        var marker = Path.Combine(workspace, MarkerFileName);

        if (!TryOwnRun(marker))
        {
            return;
        }

        CleanupPreviousRunLogs(workspace, File.GetLastWriteTime(marker));
        CleanupArtifactSubdirectories(workspace);
    }

    // Artifact subdirectories under the workspace (e.g. moodledemo-artifacts) are not run
    // logs — without this sweep they grow unboundedly across runs (Rules/test-practices.md
    // → Temp and artifact cleanup: age-based, never delete-all)
    private static void CleanupArtifactSubdirectories(string workspace)
    {
        var cutoff = DateTime.UtcNow.AddDays(-ArtifactRetention.RetentionDays);

        foreach (var directory in Directory.EnumerateDirectories(workspace))
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (File.GetLastWriteTimeUtc(file) >= cutoff)
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (IOException exception)
                {
                    ActionLogger.Logger.Warning(exception, "Failed to delete old workspace artifact {File}", file);
                }
                catch (UnauthorizedAccessException exception)
                {
                    ActionLogger.Logger.Warning(exception, "Failed to delete old workspace artifact {File}", file);
                }
            }
        }
    }

    private static bool TryOwnRun(string marker)
    {
        if (_runLock is not null)
        {
            return false;
        }

        try
        {
            _runLock = new FileStream(marker, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            File.SetLastWriteTime(marker, DateTime.Now);

            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void CleanupPreviousRunLogs(string workspace, DateTime runStart)
    {
        foreach (var pattern in new[] { $"{ActionLogPrefix}*.log", $"{TestResultLogPrefix}*.log" })
        {
            foreach (var file in Directory.EnumerateFiles(workspace, pattern))
            {
                if (File.GetLastWriteTime(file) >= runStart)
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (IOException exception)
                {
                    ActionLogger.Logger.Warning(exception, "Failed to delete previous-run log {File}", file);
                }
                catch (UnauthorizedAccessException exception)
                {
                    ActionLogger.Logger.Warning(exception, "Failed to delete previous-run log {File}", file);
                }
            }
        }
    }
}
