using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System.Collections.Concurrent;
using TestAdapter.Helpers;

namespace TestAdapter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllureNUnitAttribute : Attribute, ITestAction
{
    private static readonly string ResultsDir;
    private static readonly ConcurrentDictionary<string, ContainerInfo> Containers = new();
    internal static readonly ConcurrentDictionary<string, StartedTest> StartedTests = new();
    private long _startTicks;
    private string _testUuid = null!;

    static AllureNUnitAttribute()
    {
        ResultsDir = AllureHelper.GetResultsDir();
    }

    public void BeforeTest(ITest test)
    {
        _startTicks = Environment.TickCount64;
        _testUuid = Guid.NewGuid().ToString();
        StartedTests[test.FullName] = new StartedTest(test, _testUuid, _startTicks);
    }

    public void AfterTest(ITest test)
    {
        StartedTests.TryRemove(test.FullName, out _);
        WriteTestResult(test, _testUuid, _startTicks);
    }

    public ActionTargets Targets => ActionTargets.Test;

    internal static void WriteSkippedTests(IEnumerable<ITest> skippedTests)
    {
        foreach (var test in skippedTests)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var uuid = Guid.NewGuid().ToString();
            WriteSkippedResult(test, uuid, now);
        }
    }

    private static void WriteSkippedResult(ITest test, string uuid, long now)
    {
        var description = GetDescription(test);
        var testResult = AllureTestResultBuilder.BuildTestResult(new TestResultParams(
            Uuid: uuid,
            FullName: test.FullName,
            Name: test.Name,
            StartMs: now,
            StopMs: now,
            Status: "skipped",
            StatusMessage: "Ignored by [Ignore] attribute",
            Description: description,
            TestClassName: test.ClassName));

        AllureJsonWriter.WriteResultFile(ResultsDir, testResult);
        AddToContainer(test, uuid);
    }

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
            StartMs: now - elapsed,
            StopMs: now,
            Status: status,
            StatusMessage: status == "failed" ? result.Message : null,
            StatusTrace: status == "failed" ? result.StackTrace : null,
            Description: description,
            TestClassName: test.ClassName));

        AllureJsonWriter.WriteResultFile(ResultsDir, testResult);
        AddToContainer(test, uuid);
    }

    private static string MapTestStatus(TestStatus outcome) => outcome switch
    {
        TestStatus.Passed => "passed",
        TestStatus.Failed => "failed",
        TestStatus.Skipped => "skipped",
        _ => "broken"
    };

    private static void AddToContainer(ITest test, string uuid)
    {
        var containerUuid = EnsureContainer(test);
        if (Containers.TryGetValue(containerUuid, out var info))
        {
            lock (info.Children)
            {
                if (!info.Children.Contains(uuid))
                {
                    info.Children.Add(uuid);
                    var container = AllureTestResultBuilder.BuildContainer(containerUuid, info.Name, info.Children.ToList());
                    AllureJsonWriter.WriteContainerFile(ResultsDir, containerUuid, container);
                }
            }
        }
    }

    private static string EnsureContainer(ITest test)
    {
        var className = test.ClassName ?? "Unknown";
        var containerUuid = GetDeterministicUuid(className);

        if (!Containers.ContainsKey(containerUuid))
        {
            Containers[containerUuid] = new ContainerInfo
            {
                Uuid = containerUuid,
                Name = className,
                Children = new ConcurrentBag<string>()
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

    private static string? GetDescription(ITest test)
    {
        if (test.Method?.MethodInfo == null) return null;
        return AllureHelper.GetDescription(test.Method.MethodInfo);
    }

    private class ContainerInfo
    {
        public string Uuid { get; set; } = "";
        public string Name { get; set; } = "";
        public ConcurrentBag<string> Children { get; set; } = new();
    }

    internal record StartedTest(ITest Test, string Uuid, long StartTicks);
}
