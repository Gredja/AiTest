namespace Core.Logging;

internal static class ActionLogParser
{
    internal static Dictionary<string, List<string>> Read(
        IReadOnlyCollection<string> actionLogPaths,
        IReadOnlyDictionary<string, string> fixtureByTest)
    {
        var actions = fixtureByTest.Keys.ToDictionary(name => name, _ => new List<string>());

        foreach (var logPath in actionLogPaths)
        {
            foreach (var line in SharedFile.ReadLines(logPath))
            {
                var parts = line.Split('|', 4);
                if (parts.Length != 4)
                {
                    continue;
                }

                AppendToMatchingTests(actions, line, parts[2], fixtureByTest);
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
