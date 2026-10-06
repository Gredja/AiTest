using System.Reflection;
using NUnit.Framework;

namespace TestAdapter.Helpers;

public static class AllureHelper
{
    private const string SolutionFileName = "Gredja.slnx";

    private static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, SolutionFileName)) || Directory.GetFiles(dir, "*.sln").Length > 0)
            {
                return dir;
            }
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

    internal static string? GetDescription(MethodInfo method)
    {
        var attributes = method.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes.Length == 0)
        {
            return null;
        }

        return attributes[0].GetType().GetProperty("Description")?.GetValue(attributes[0]) as string;
    }
}
