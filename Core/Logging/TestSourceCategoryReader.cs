using System.Text.RegularExpressions;

namespace Core.Logging;

internal static class TestSourceCategoryReader
{
    private static readonly string[] _serviceCategories = ["JsonPlaceholder", "GitHub", "GitHubE2E"];
    private static readonly Regex _classRegex = new(@"public\s+(?:abstract\s+)?class\s+(\w+)", RegexOptions.Compiled);
    private static readonly Regex _methodRegex = new(@"public\s+async\s+Task\s+(\w+)", RegexOptions.Compiled);
    private static readonly Regex _categoryRegex = new(@"Category\(""(\w+)""\)", RegexOptions.Compiled);

    internal static Dictionary<string, Queue<(string Fixture, string Category)>> Read(string root)
    {
        var categories = new Dictionary<string, Queue<(string Fixture, string Category)>>();

        foreach (var sourcePath in EnumerateTestSources(root))
        {
            CollectFromFile(sourcePath, categories);
        }

        return categories;
    }

    private static IEnumerable<string> EnumerateTestSources(string root) =>
        new[] { Path.Combine(root, "Api"), Path.Combine(root, "E2E") }
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            .Where(path => !IsBuildOutput(path));

    private static bool IsBuildOutput(string path) =>
        path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "obj" or "bin");

    private static void CollectFromFile(string sourcePath, Dictionary<string, Queue<(string Fixture, string Category)>> categories)
    {
        var content = File.ReadAllText(sourcePath);
        if (!content.Contains("[Test]"))
        {
            return;
        }

        var classMatch = _classRegex.Match(content);
        if (!classMatch.Success)
        {
            return;
        }

        var className = classMatch.Groups[1].Value;
        var chunks = content.Split("[Test]");

        foreach (var chunk in chunks.Skip(1))
        {
            CollectFromChunk(chunk, className, categories);
        }
    }

    private static void CollectFromChunk(
        string chunk,
        string className,
        Dictionary<string, Queue<(string Fixture, string Category)>> categories)
    {
        var methodMatch = _methodRegex.Match(chunk);
        if (!methodMatch.Success)
        {
            return;
        }

        var categoryText = ReadCategoryText(chunk);
        var methodName = methodMatch.Groups[1].Value;

        if (!categories.TryGetValue(methodName, out var queue))
        {
            queue = new Queue<(string Fixture, string Category)>();
            categories[methodName] = queue;
        }

        queue.Enqueue((className, categoryText));
    }

    private static string ReadCategoryText(string chunk)
    {
        var categoryNames = _categoryRegex.Matches(chunk)
            .Select(match => match.Groups[1].Value)
            .Where(name => !_serviceCategories.Contains(name))
            .ToArray();

        return categoryNames.Length == 0 ? "-" : string.Join(", ", categoryNames);
    }
}
