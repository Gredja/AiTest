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
        var result = CreateBaseResult(p);

        if (p.Categories != null && p.Categories.Count > 0)
            result["tags"] = p.Categories.Select(c => new Dictionary<string, string> { ["name"] = c }).ToList();

        if (p.StatusMessage != null)
        {
            result["statusDetails"] = BuildStatusDetails(p.StatusMessage, p.StatusTrace);
        }

        if (p.Description != null)
        {
            result["description"] = p.Description;
        }

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

    private static Dictionary<string, object> CreateBaseResult(TestResultParams p)
    {
        return new Dictionary<string, object>
        {
            ["uuid"] = p.Uuid,
            ["testCaseId"] = p.FullName,
            ["id"] = p.FullName,
            ["name"] = p.Name,
            ["fullName"] = p.FullName,
            ["time"] = BuildTime(p.StartMs, p.StopMs),
            ["status"] = p.Status,
            ["labels"] = BuildLabels(p),
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

    private static List<Dictionary<string, string>> BuildLabels(TestResultParams p)
    {
        var serviceName = ExtractServiceName(p.TestClassName);

        var labels = new List<Dictionary<string, string>>
        {
            new() { ["name"] = "framework", ["value"] = "nunit" },
            new() { ["name"] = "host", ["value"] = Environment.MachineName },
            new() { ["name"] = "package", ["value"] = p.TestClassName ?? "Tests" },
            new() { ["name"] = "parentSuite", ["value"] = serviceName },
            new() { ["name"] = "suite", ["value"] = p.TestClassName ?? "Tests" }
        };

        if (p.Epic != null)
            labels.Add(new() { ["name"] = "epic", ["value"] = p.Epic });

        if (p.Feature != null)
            labels.Add(new() { ["name"] = "feature", ["value"] = p.Feature });

        if (p.Story != null)
            labels.Add(new() { ["name"] = "story", ["value"] = p.Story });

        if (p.Categories != null)
        {
            foreach (var category in p.Categories)
            {
                labels.Add(new() { ["name"] = "tag", ["value"] = category });
            }
        }

        return labels;
    }

    private static string ExtractServiceName(string? testClassName)
    {
        if (string.IsNullOrEmpty(testClassName))
        {
            return "Tests";
        }

        var parts = testClassName.Split('.');

        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "Tests" && i > 0)
            {
                return parts[i - 1];
            }
        }

        return parts.Length > 1 ? parts[1] : "Tests";
    }
}
