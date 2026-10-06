using static TestAdapter.Helpers.AllureConstants;

namespace TestAdapter.Helpers;

internal static class AllureTestResultBuilder
{
    public static Dictionary<string, object> BuildTestResult(TestResultParams resultParams)
    {
        var result = CreateBaseResult(resultParams);

        if (resultParams.Categories is not null && resultParams.Categories.Any())
        {
            result["tags"] = resultParams.Categories.Select(category => new Dictionary<string, string> { [NameKey] = category }).ToList();
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

    public static Dictionary<string, object> BuildContainer(string uuid, string name, List<string> children) =>
        new Dictionary<string, object>
        {
            [UuidKey] = uuid,
            [IdKey] = name,
            [NameKey] = name,
            ["children"] = children,
            [TimeKey] = new Dictionary<string, long> { [StartKey] = 0, [StopKey] = 0, [DurationKey] = 0 },
            [LabelsKey] = new List<Dictionary<string, string>>
            {
                new() { [NameKey] = AllureConstants.LabelSuite, [ValueKey] = name }
            }
        };

    private static Dictionary<string, object> CreateBaseResult(TestResultParams resultParams) =>
        new Dictionary<string, object>
        {
            [UuidKey] = resultParams.Uuid,
            ["testCaseId"] = resultParams.FullName,
            [IdKey] = resultParams.FullName,
            [NameKey] = resultParams.Name,
            ["fullName"] = resultParams.FullName,
            [TimeKey] = BuildTime(resultParams.StartMilliseconds, resultParams.StopMilliseconds),
            ["status"] = resultParams.Status,
            [LabelsKey] = BuildLabels(resultParams),
            ["links"] = new List<object>(),
            ["steps"] = new List<object>(),
            ["attachments"] = new List<object>()
        };

    private static Dictionary<string, long> BuildTime(long startMilliseconds, long stopMilliseconds) =>
        new Dictionary<string, long>
        {
            [StartKey] = startMilliseconds,
            [StopKey] = stopMilliseconds,
            [DurationKey] = stopMilliseconds - startMilliseconds
        };

    private static Dictionary<string, string> BuildStatusDetails(string message, string? trace) =>
        new Dictionary<string, string>
        {
            ["message"] = message,
            ["trace"] = trace ?? ""
        };

    private static List<Dictionary<string, string>> BuildLabels(TestResultParams resultParams)
    {
        var serviceName = ExtractServiceName(resultParams.TestClassName);

        var labels = new List<Dictionary<string, string>>
        {
            new() { [NameKey] = "framework", [ValueKey] = "nunit" },
            new() { [NameKey] = "host", [ValueKey] = Environment.MachineName },
            new() { [NameKey] = "package", [ValueKey] = resultParams.TestClassName ?? "Tests" },
            new() { [NameKey] = "parentSuite", [ValueKey] = serviceName },
            new() { [NameKey] = AllureConstants.LabelSuite, [ValueKey] = resultParams.TestClassName ?? "Tests" }
        };

        AddOptionalLabels(labels, resultParams);

        return labels;
    }

    private static void AddOptionalLabels(List<Dictionary<string, string>> labels, TestResultParams resultParams)
    {
        if (resultParams.Epic is not null)
        {
            labels.Add(new() { [NameKey] = "epic", [ValueKey] = resultParams.Epic });
        }

        if (resultParams.Feature is not null)
        {
            labels.Add(new() { [NameKey] = "feature", [ValueKey] = resultParams.Feature });
        }

        if (resultParams.Story is not null)
        {
            labels.Add(new() { [NameKey] = "story", [ValueKey] = resultParams.Story });
        }

        if (resultParams.Categories is not null)
        {
            foreach (var category in resultParams.Categories)
            {
                labels.Add(new() { [NameKey] = "tag", [ValueKey] = category });
            }
        }
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
