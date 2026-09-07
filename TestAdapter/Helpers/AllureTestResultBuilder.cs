namespace TestAdapter.Helpers;

public record TestResultParams(
    string Uuid,
    string FullName,
    string Name,
    long StartMs,
    long StopMs,
    string Status,
    string? StatusMessage = null,
    string? StatusTrace = null,
    string? Description = null,
    string? TestClassName = null,
    List<string>? Categories = null,
    string? Epic = null,
    string? Feature = null,
    string? Story = null);

public static class AllureTestResultBuilder
{
    public static Dictionary<string, object> BuildTestResult(TestResultParams p)
    {
        var result = CreateBaseResult(p.Uuid, p.FullName, p.Name, p.StartMs, p.StopMs, p.Status, p.TestClassName, p.Categories, p.Epic, p.Feature, p.Story);

        if (p.Categories != null && p.Categories.Count > 0)
            result["tags"] = p.Categories.Select(c => new Dictionary<string, string> { ["name"] = c }).ToList();

        if (p.StatusMessage != null)
            result["statusDetails"] = BuildStatusDetails(p.StatusMessage, p.StatusTrace);

        if (p.Description != null)
            result["description"] = p.Description;

        return result;
    }

    public static Dictionary<string, object> BuildContainer(string uuid, string name, List<string> children)
    {
        return new Dictionary<string, object>
        {
            ["uuid"] = uuid,
            ["id"] = name,
            ["name"] = name,
            ["children"] = children,
            ["time"] = new Dictionary<string, long> { ["start"] = 0, ["stop"] = 0, ["duration"] = 0 },
            ["labels"] = new List<Dictionary<string, string>>
            {
                new() { ["name"] = "suite", ["value"] = name }
            }
        };
    }

    private static Dictionary<string, object> CreateBaseResult(
        string uuid, string fullName, string name,
        long startMs, long stopMs, string status, string? testClassName, List<string>? categories,
        string? epic, string? feature, string? story)
    {
        return new Dictionary<string, object>
        {
            ["uuid"] = uuid,
            ["testCaseId"] = fullName,
            ["id"] = fullName,
            ["name"] = name,
            ["fullName"] = fullName,
            ["time"] = BuildTime(startMs, stopMs),
            ["status"] = status,
            ["labels"] = BuildLabels(testClassName, categories, epic, feature, story),
            ["links"] = new List<object>(),
            ["steps"] = new List<object>(),
            ["attachments"] = new List<object>()
        };
    }

    private static Dictionary<string, long> BuildTime(long startMs, long stopMs)
    {
        return new Dictionary<string, long>
        {
            ["start"] = startMs,
            ["stop"] = stopMs,
            ["duration"] = stopMs - startMs
        };
    }

    private static Dictionary<string, string> BuildStatusDetails(string message, string? trace)
    {
        return new Dictionary<string, string>
        {
            ["message"] = message,
            ["trace"] = trace ?? ""
        };
    }

    private static List<Dictionary<string, string>> BuildLabels(string? testClassName, List<string>? categories, string? epic, string? feature, string? story)
    {
        var projectName = ExtractProjectName(testClassName);

        var labels = new List<Dictionary<string, string>>
        {
            new() { ["name"] = "framework", ["value"] = "nunit" },
            new() { ["name"] = "host", ["value"] = Environment.MachineName },
            new() { ["name"] = "package", ["value"] = testClassName ?? "Tests" },
            new() { ["name"] = "parentSuite", ["value"] = projectName },
            new() { ["name"] = "suite", ["value"] = testClassName ?? "Tests" }
        };

        if (epic != null)
            labels.Add(new() { ["name"] = "epic", ["value"] = epic });

        if (feature != null)
            labels.Add(new() { ["name"] = "feature", ["value"] = feature });

        if (story != null)
            labels.Add(new() { ["name"] = "story", ["value"] = story });

        if (categories != null)
        {
            foreach (var category in categories)
            {
                labels.Add(new() { ["name"] = "tag", ["value"] = category });
            }
        }

        return labels;
    }

    private static string ExtractProjectName(string? namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName))
            return "Tests";

        var parts = namespaceName.Split('.');
        return parts.Length > 0 ? parts[0] : "Tests";
    }
}
