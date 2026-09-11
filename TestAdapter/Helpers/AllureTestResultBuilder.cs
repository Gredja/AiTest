namespace TestAdapter.Helpers;

public static class AllureTestResultBuilder
{
    public static Dictionary<string, object> BuildTestResult(TestResultParams resultParams)
    {
        var result = CreateBaseResult(resultParams);

        if (resultParams.Categories is not null && resultParams.Categories.Count > 0)
        {
            result["tags"] = resultParams.Categories.Select(c => new Dictionary<string, string> { ["name"] = c }).ToList();
        }

        if (resultParams.StatusMessage is not null)
        {
            result["statusDetails"] = BuildStatusDetails(resultParams.StatusMessage, resultParams.StatusTrace);
        }

        if (resultParams.Description is not null)
        {
            result["description"] = resultParams.Description;
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
                new() { ["name"] = AllureConstants.LabelSuite, ["value"] = name }
            }
        };
    }

    private static Dictionary<string, object> CreateBaseResult(TestResultParams resultParams)
    {
        return new Dictionary<string, object>
        {
            ["uuid"] = resultParams.Uuid,
            ["testCaseId"] = resultParams.FullName,
            ["id"] = resultParams.FullName,
            ["name"] = resultParams.Name,
            ["fullName"] = resultParams.FullName,
            ["time"] = BuildTime(resultParams.StartMilliseconds, resultParams.StopMilliseconds),
            ["status"] = resultParams.Status,
            ["labels"] = BuildLabels(resultParams),
            ["links"] = new List<object>(),
            ["steps"] = new List<object>(),
            ["attachments"] = new List<object>()
        };
    }

    private static Dictionary<string, long> BuildTime(long startMilliseconds, long stopMilliseconds)
    {
        return new Dictionary<string, long>
        {
            ["start"] = startMilliseconds,
            ["stop"] = stopMilliseconds,
            ["duration"] = stopMilliseconds - startMilliseconds
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

    private static List<Dictionary<string, string>> BuildLabels(TestResultParams resultParams)
    {
        var serviceName = ExtractServiceName(resultParams.TestClassName);

        var labels = new List<Dictionary<string, string>>
        {
            new() { ["name"] = "framework", ["value"] = "nunit" },
            new() { ["name"] = "host", ["value"] = Environment.MachineName },
            new() { ["name"] = "package", ["value"] = resultParams.TestClassName ?? "Tests" },
            new() { ["name"] = "parentSuite", ["value"] = serviceName },
            new() { ["name"] = AllureConstants.LabelSuite, ["value"] = resultParams.TestClassName ?? "Tests" }
        };

        if (resultParams.Epic is not null)
        {
            labels.Add(new() { ["name"] = "epic", ["value"] = resultParams.Epic });
        }

        if (resultParams.Feature is not null)
        {
            labels.Add(new() { ["name"] = "feature", ["value"] = resultParams.Feature });
        }

        if (resultParams.Story is not null)
        {
            labels.Add(new() { ["name"] = "story", ["value"] = resultParams.Story });
        }

        if (resultParams.Categories is not null)
        {
            foreach (var category in resultParams.Categories)
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
