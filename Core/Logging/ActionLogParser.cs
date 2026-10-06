namespace Core.Logging;

internal static class ActionLogParser
{
    private const int LogFieldCount = 4;
    private const int TestNameFieldIndex = 2;

    internal static Dictionary<string, List<string>> Read(
        IReadOnlyCollection<string> actionLogPaths,
        IReadOnlyDictionary<string, string> fixtureByTest)
    {
        var actions = fixtureByTest.Keys.ToDictionary(name => name, _ => new List<string>());

        foreach (var logPath in actionLogPaths)
        {
            foreach (var line in SharedFile.ReadLines(logPath))
            {
                var parts = line.Split('|', LogFieldCount);
                if (parts.Length != LogFieldCount)
                {
                    continue;
                }

                AppendToMatchingTests(actions, line, parts[TestNameFieldIndex], fixtureByTest);
            }
        }

        return actions;
    }

    private static void AppendToMatchingTests(
        Dictionary<string, List<string>> actions,
        string line,
        string loggedTestName,
        IReadOnlyDictionary<string, string> fixtureByTest)
    {
        foreach (var (test, fixture) in fixtureByTest)
        {
            var isTestAction = loggedTestName.EndsWith("." + test, StringComparison.Ordinal);
            var isFixtureAction = fixture != "-" && loggedTestName.EndsWith("." + fixture, StringComparison.Ordinal);

            if (isTestAction || isFixtureAction)
            {
                actions[test].Add(line);
            }
        }
    }
}
