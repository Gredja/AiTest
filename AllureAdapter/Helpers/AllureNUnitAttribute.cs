using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System.Collections.Concurrent;
using AllureAdapter.Helpers;

namespace AllureAdapter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllureNUnitAttribute : Attribute, ITestAction
{
    private static readonly string _resultsDir;
    private static readonly ConcurrentDictionary<string, ContainerInfo> _containers = new();

    private long _startTicks;
    private string _testUuid = string.Empty;

    static AllureNUnitAttribute()
    {
        _resultsDir = AllureHelper.GetResultsDir();
    }

    public void BeforeTest(ITest test)
    {
        _startTicks = Environment.TickCount64;
        _testUuid = Guid.NewGuid().ToString();
    }

    public void AfterTest(ITest test) =>
        WriteTestResult(test, _testUuid, _startTicks);

    public ActionTargets Targets => ActionTargets.Test;

    private static void WriteTestResult(ITest test, string uuid, long startTicks)
    {
        var elapsed = Environment.TickCount64 - startTicks;
        var result = TestContext.CurrentContext.Result;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var status = MapTestStatus(result.Outcome.Status);
        var description = GetDescription(test);

        var testResult = AllureTestResultBuilder.BuildTestResult(new TestResultParams(
            Uuid: uuid,
            FullName: test.FullName,
            Name: test.Name,
            StartMilliseconds: now - elapsed,
            StopMilliseconds: now,
            Status: status,
            StatusMessage: status == AllureConstants.StatusFailed ? result.Message : null,
            StatusTrace: status == AllureConstants.StatusFailed ? result.StackTrace : null,
            Description: description,
            TestClassName: test.ClassName,
            Categories: GetCategories(test)));

        AllureJsonWriter.WriteResultFile(_resultsDir, testResult);
        AddToContainer(test, uuid);
    }

    private static string MapTestStatus(TestStatus outcome) => outcome switch
    {
        TestStatus.Passed => AllureConstants.StatusPassed,
        TestStatus.Failed => AllureConstants.StatusFailed,
        TestStatus.Skipped => AllureConstants.StatusSkipped,
        _ => AllureConstants.StatusBroken
    };

    private static void AddToContainer(ITest test, string uuid)
    {
        var containerUuid = EnsureContainer(test);
        if (!_containers.TryGetValue(containerUuid, out var info))
        {
            return;
        }

        lock (info.Children)
        {
            if (!info.Children.Contains(uuid))
            {
                info.Children.Add(uuid);
                var container = AllureTestResultBuilder.BuildContainer(containerUuid, info.Name, info.Children.ToList());
                AllureJsonWriter.WriteContainerFile(_resultsDir, containerUuid, container);
            }
        }
    }

    private static string EnsureContainer(ITest test)
    {
        var className = test.ClassName ?? "Unknown";
        var containerUuid = GetDeterministicUuid(className);

        if (!_containers.ContainsKey(containerUuid))
        {
            _containers[containerUuid] = new ContainerInfo
            {
                Uuid = containerUuid,
                Name = className,
                Children = []
            };
        }

        return containerUuid;
    }

    private const int GuidByteLength = 16;

    private static string GetDeterministicUuid(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));

        return new Guid(hash[..GuidByteLength]).ToString();
    }

    private static List<string>? GetCategories(ITest test)
    {
        var categories = new List<string>();

        // NUnit keeps fixture-level categories on the fixture test only — walk the parent chain
        for (var current = test; current is not null; current = current.Parent)
        {
            if (!current.Properties.ContainsKey(PropertyNames.Category))
            {
                continue;
            }

            categories.AddRange(current.Properties[PropertyNames.Category]
                .Cast<object>()
                .Select(value => value?.ToString() ?? string.Empty)
                .Where(value => value.Length > 0));
        }

        var distinct = categories.Distinct().ToList();
        return distinct.Count > 0 ? distinct : null;
    }

    private static string? GetDescription(ITest test)
    {
        if (test.Method?.MethodInfo is null)
        {
            return null;
        }

        return AllureHelper.GetDescription(test.Method.MethodInfo);
    }
}
