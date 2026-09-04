namespace TestAdapter.Helpers;

internal static class AllureTestResultBuilder
{
    public static Dictionary<string, object> BuildTestResult(
        string uuid,
        string fullName,
        string name,
        long startMs,
        long stopMs,
        string status,
        string? statusMessage = null,
        string? statusTrace = null,
        string? description = null,
        string? testClassName = null)
    {
        var result = CreateBaseResult(uuid, fullName, name, startMs, stopMs, status, testClassName);

        if (statusMessage != null)
            result["statusDetails"] = BuildStatusDetails(statusMessage, statusTrace);

        if (description != null)
            result["description"] = description;

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
        long startMs, long stopMs, string status, string? testClassName)
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
            ["labels"] = BuildLabels(testClassName),
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

    private static List<Dictionary<string, string>> BuildLabels(string? testClassName)
    {
        var projectName = ExtractProjectName(testClassName);

        return new List<Dictionary<string, string>>
        {
            new() { ["name"] = "framework", ["value"] = "nunit" },
            new() { ["name"] = "host", ["value"] = Environment.MachineName },
            new() { ["name"] = "package", ["value"] = testClassName ?? "Tests" },
            new() { ["name"] = "parentSuite", ["value"] = projectName },
            new() { ["name"] = "suite", ["value"] = testClassName ?? "Tests" }
        };
    }

    private static string ExtractProjectName(string? namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName))
            return "Tests";

        var parts = namespaceName.Split('.');
        return parts.Length > 0 ? parts[0] : "Tests";
    }
}
