using System.Reflection;
using Core.Config;
using NUnit.Framework;

namespace AllureAdapter.Helpers;

public static class AllureHelper
{
    private static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, TestConfig.SolutionFileName)) || Directory.EnumerateFiles(dir, "*.sln").Any())
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
        var resultsDir = Path.Combine(projectDir, TestConfig.AllureResultsDirName);
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
