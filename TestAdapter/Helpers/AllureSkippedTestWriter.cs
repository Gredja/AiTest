using System.Reflection;
using NUnit.Framework;

namespace TestAdapter.Helpers;

public static class AllureSkippedTestWriter
{
    public static void WriteSkippedTests(Assembly assembly, string resultsDir)
    {
        var types = GetTypesSafely(assembly);

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<TestAttribute>() is null)
                {
                    continue;
                }

                var ignoreAttr = method.GetCustomAttribute<IgnoreAttribute>();
                if (ignoreAttr is null)
                {
                    continue;
                }

                WriteSkippedTestResult(type, method, ignoreAttr, resultsDir);
            }
        }
    }

    private static void WriteSkippedTestResult(Type type, MethodInfo method, IgnoreAttribute ignoreAttr, string resultsDir)
    {
        var fullName = $"{type.FullName}.{method.Name}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var uuid = Guid.NewGuid().ToString();
        var description = AllureHelper.GetDescription(method);

        var testResult = AllureTestResultBuilder.BuildTestResult(new TestResultParams(
            Uuid: uuid,
            FullName: fullName,
            Name: method.Name,
            StartMilliseconds: now,
            StopMilliseconds: now,
            Status: AllureConstants.StatusSkipped,
            StatusMessage: ignoreAttr.Reason ?? AllureConstants.DefaultIgnoreMessage,
            Description: description,
            TestClassName: type.Namespace));

        AllureJsonWriter.WriteResultFile(resultsDir, testResult);
    }

    private static Type[] GetTypesSafely(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null).ToArray()!; }
    }
}
