using System.Reflection;
using Core.Config;
using NUnit.Framework;

namespace AllureAdapter.Helpers;

public static class AllureHelper
{
    private const string SolutionSearchPattern = "*.sln";

    // NUnit 4 keeps [Description] in the PropertyBag, not a CLR property
    private const string DescriptionPropertyKey = "Description";

    private static string FindProjectRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory, TestConfig.SolutionFileName)) || Directory.EnumerateFiles(directory, SolutionSearchPattern).Any())
            {
                return directory;
            }
            directory = Directory.GetParent(directory)?.FullName;
        }

        return AppContext.BaseDirectory;
    }

    public static string GetResultsDirectory()
    {
        var projectDirectory = FindProjectRoot();
        var resultsDirectory = Path.Combine(projectDirectory, TestConfig.AllureResultsDirName);
        Directory.CreateDirectory(resultsDirectory);

        return resultsDirectory;
    }

    internal static string? GetDescription(MethodInfo method)
    {
        var attributes = method.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes.Length == 0)
        {
            return null;
        }

        return attributes[0] is DescriptionAttribute descriptionAttribute
            ? descriptionAttribute.Properties.Get(DescriptionPropertyKey) as string
            : null;
    }
}
