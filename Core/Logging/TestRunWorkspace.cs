namespace Core.Logging;

internal static class TestRunWorkspace
{
    internal const string MarkerFileName = ".run-start";

    // The marker is held open by the testhost that starts the run. A sibling testhost
    // (the second assembly of the same run) cannot open it for write and therefore
    // joins the run; when all testhosts exit the handle is released and the next run
    // takes ownership — this is what gives every run a fresh report without
    // distinguishing `dotnet test` from Visual Studio launches.
    private static FileStream? _runLock;

    internal static string Resolve()
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
        foreach (var pattern in new[] { "actions-*.log", "test-results-*.log" })
        {
            foreach (var file in Directory.EnumerateFiles(workspace, pattern))
            {
                if (File.GetLastWriteTime(file) < runStart)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
