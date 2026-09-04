namespace TestAdapter.Helpers;

internal static class AllureHelper
{
    public static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Gredja.slnx")) || Directory.GetFiles(dir, "*.sln").Length > 0)
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        return AppContext.BaseDirectory;
    }

    public static string GetResultsDir()
    {
        var projectDir = FindProjectRoot();
        var resultsDir = Path.Combine(projectDir, "allure-results");
        Directory.CreateDirectory(resultsDir);
        return resultsDir;
    }
}
