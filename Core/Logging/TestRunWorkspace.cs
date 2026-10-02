namespace Core.Logging;

internal static class TestRunWorkspace
{
    internal static string Resolve()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "GredjaTestRun");
        Directory.CreateDirectory(workspace);
        return workspace;
    }
}
