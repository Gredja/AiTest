using System.Globalization;
using System.Text;

namespace Core.Logging;

public static class TestRunReportGenerator
{
    private const int MaxWriteAttempts = 3;
    private const int RetryDelayMilliseconds = 200;
    private const string ReportPrefix = "TestRunReport-";
    private const string ActionLogPrefix = "actions-";
    private const string TestResultLogPrefix = "test-results-";

    private static readonly Encoding _reportEncoding = new UTF8Encoding(false);

    public static void PrepareRun() => TestRunWorkspace.PrepareRun();

    public static void Generate()
    {
        var root = ResolveRepositoryRoot();
        if (root is null)
        {
            return;
        }

        var workspace = TestRunWorkspace.Resolve();
        var runStartFile = Path.Combine(workspace, TestRunWorkspace.MarkerFileName);
        if (!File.Exists(runStartFile))
        {
            return;
        }

        var runStart = File.GetLastWriteTime(runStartFile);
        var outcomeLogs = CollectLogs(workspace, runStart, TestResultLogPrefix);
        var outcomes = TestOutcomeReader.Read(outcomeLogs);
        if (outcomes.Count == 0)
        {
            return;
        }

        var actionLogs = CollectLogs(workspace, runStart, ActionLogPrefix);
        var categories = TestSourceCategoryReader.Read(root);
        var rows = BuildRows(outcomes, categories);
        var fixtureByTest = rows
            .Where(row => row.Result == "Failed")
            .GroupBy(row => row.Test)
            .ToDictionary(group => group.Key, group => group.First().Fixture);
        var failures = BuildFailures(fixtureByTest, outcomes);
        var failedActions = ActionLogParser.Read(actionLogs, fixtureByTest);

        var lines = new List<string>();
        lines.AddRange(BuildHeader(runStart, workspace, actionLogs.Count));
        lines.AddRange(BuildSummary(rows));
        lines.AddRange(BuildTests(rows));
        lines.AddRange(BuildFailedSection(failures, failedActions));

        var resultsDirectory = Path.Combine(root, "TestResults");
        Directory.CreateDirectory(resultsDirectory);
        var reportFile = Path.Combine(resultsDirectory, ReportPrefix + runStart.ToString("yyyyMMdd-HHmmss") + ".md");
        WriteWithRetry(reportFile, lines);
    }

    private static string? ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Gredja.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName;
    }

    private static List<string> CollectLogs(string workspace, DateTime runStart, string filePrefix) =>
        Directory.EnumerateFiles(workspace)
            .Where(path => Path.GetFileName(path).StartsWith(filePrefix, StringComparison.Ordinal))
            .Where(path => path.EndsWith(".log", StringComparison.Ordinal))
            .Where(path => File.GetLastWriteTime(path) >= runStart.AddSeconds(-1))
            .OrderBy(path => path)
            .ToList();

    private static List<TestRunRow> BuildRows(
        List<TestOutcome> outcomes,
        Dictionary<string, Queue<(string Fixture, string Category)>> categories)
    {
        var rows = new List<TestRunRow>();

        foreach (var outcome in outcomes)
        {
            rows.Add(CreateRow(outcome, categories));
        }

        return rows.OrderBy(row => row.Fixture).ThenBy(row => row.Test).ToList();
    }

    private static TestRunRow CreateRow(
        TestOutcome outcome,
        Dictionary<string, Queue<(string Fixture, string Category)>> categories)
    {
        var fixture = "-";
        var category = "-";

        if (categories.TryGetValue(outcome.Name, out var queue) && queue.Count > 0)
        {
            (fixture, category) = queue.Dequeue();
        }

        var duration = Math.Round(outcome.DurationMilliseconds / 1000.0, 3);
        return new TestRunRow(outcome.Name, fixture, category, MapStatus(outcome.Status), duration);
    }

    private static string MapStatus(string status) => status switch
    {
        "passed" => "Passed",
        "skipped" => "Skipped",
        _ => "Failed"
    };

    private static Dictionary<string, (string Message, string Trace)> BuildFailures(
        Dictionary<string, string> fixtureByTest,
        List<TestOutcome> outcomes)
    {
        var failures = fixtureByTest.Keys.ToDictionary(
            name => name,
            _ => (Message: string.Empty, Trace: string.Empty));

        foreach (var outcome in outcomes.Where(outcome => fixtureByTest.ContainsKey(outcome.Name)))
        {
            if (string.IsNullOrEmpty(failures[outcome.Name].Message))
            {
                failures[outcome.Name] = (outcome.Message, outcome.Trace);
            }
        }

        return failures;
    }

    private static IEnumerable<string> BuildHeader(DateTime runStart, string workspace, int actionLogCount)
    {
        yield return "# Test Run Report - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        yield return string.Empty;
        yield return "- Run started: " + runStart.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        yield return $"- Raw logs: {workspace}";
        yield return $"- Action logs: {actionLogCount}";
        yield return string.Empty;
    }

    private static IEnumerable<string> BuildSummary(List<TestRunRow> rows)
    {
        var total = rows.Count;
        var passed = rows.Count(row => row.Result == "Passed");
        var failed = rows.Count(row => row.Result == "Failed");
        var skipped = rows.Count(row => row.Result == "Skipped");

        yield return "## Summary";
        yield return string.Empty;
        yield return "| Result | Count | Percent |";
        yield return "|--------|------:|--------:|";
        yield return $"| Passed | {passed} | {Percent(passed, total)} |";
        yield return $"| Failed | {failed} | {Percent(failed, total)} |";
        yield return $"| Skipped | {skipped} | {Percent(skipped, total)} |";
        yield return $"| **Total** | **{total}** | **{Percent(total, total)}** |";
        yield return string.Empty;
    }

    private static IEnumerable<string> BuildTests(List<TestRunRow> rows)
    {
        yield return "## Tests";
        yield return string.Empty;
        yield return "| # | Test | Fixture | Category | Result | Duration (s) |";
        yield return "|---|------|---------|----------|--------|-------------|";

        var index = 0;
        foreach (var row in rows)
        {
            index++;
            var test = row.Test.Replace("|", "\\|");
            var duration = row.Duration.ToString("0.###", CultureInfo.InvariantCulture);
            yield return $"| {index} | {test} | {row.Fixture} | {row.Category} | {row.Result} | {duration} |";
        }

        yield return string.Empty;
    }

    private static IEnumerable<string> BuildFailedSection(
        IReadOnlyDictionary<string, (string Message, string Trace)> failures,
        IReadOnlyDictionary<string, List<string>> failedActions)
    {
        yield return "## Failed tests";
        yield return string.Empty;

        if (failures.Count == 0)
        {
            yield return "No failed tests.";
            yield return string.Empty;
            yield break;
        }

        foreach (var test in failures.Keys.OrderBy(name => name, StringComparer.Ordinal))
        {
            foreach (var line in BuildFailedTest(test, failures[test], failedActions[test]))
            {
                yield return line;
            }
        }
    }

    private static IEnumerable<string> BuildFailedTest(
        string test,
        (string Message, string Trace) failure,
        List<string> actions)
    {
        yield return $"### {test}";
        yield return string.Empty;
        yield return "```";

        if (string.IsNullOrWhiteSpace(failure.Message) && string.IsNullOrWhiteSpace(failure.Trace))
        {
            yield return "(no failure details found)";
        }
        else
        {
            foreach (var line in SplitLines(failure.Message))
            {
                yield return line;
            }

            foreach (var line in SplitLines(failure.Trace))
            {
                yield return line;
            }
        }

        yield return "```";
        yield return string.Empty;
        yield return "Actions:";
        yield return string.Empty;
        yield return "```";

        if (actions.Count == 0)
        {
            yield return "(no action log entries found for this test)";
        }
        else
        {
            foreach (var action in actions)
            {
                yield return action;
            }
        }

        yield return "```";
        yield return string.Empty;
    }

    private static IEnumerable<string> SplitLines(string text) =>
        text.Split(["\r\n", "\n"], StringSplitOptions.None);

    private static string Percent(int count, int total) =>
        total == 0 ? "0.0%" : FormattableString.Invariant($"{count * 100.0 / total:F1}%");

    private static void WriteWithRetry(string reportFile, List<string> lines)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                File.WriteAllLines(reportFile, lines, _reportEncoding);
                return;
            }
            catch (IOException) when (attempt < MaxWriteAttempts)
            {
                Thread.Sleep(RetryDelayMilliseconds);
            }
            catch (UnauthorizedAccessException) when (attempt < MaxWriteAttempts)
            {
                Thread.Sleep(RetryDelayMilliseconds);
            }
        }
    }
}
