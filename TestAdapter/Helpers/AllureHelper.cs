using System.Reflection;
using NUnit.Framework;

namespace TestAdapter.Helpers;

public static class AllureHelper
{
    public static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Gredja.slnx")) || Directory.GetFiles(dir, "*.sln").Length > 0)
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

    public static string? GetDescription(MethodInfo method)
    {
        var attrs = method.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attrs.Length > 0)
        {
            return attrs[0].GetType().GetProperty("Description")?.GetValue(attrs[0]) as string;
        }
        return null;
    }
}
