using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Reflection;
using TestAdapter.Helpers;

namespace TestAdapter;

[SetUpFixture]
public class AllureGlobalSetup
{
    [OneTimeSetUp]
    public void GlobalSetup() { }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        var resultsDir = AllureHelper.GetResultsDir();
        var testAssemblies = GetTestAssemblies();

        foreach (var assembly in testAssemblies)
        {
            WriteSkippedTestsFromAssembly(assembly, resultsDir);
        }
    }

    private static void WriteSkippedTestsFromAssembly(Assembly assembly, string resultsDir)
    {
        var types = GetTypesSafely(assembly);

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<TestAttribute>() == null) continue;
                var ignoreAttr = method.GetCustomAttribute<IgnoreAttribute>();
                if (ignoreAttr == null) continue;

                WriteSkippedTestResult(type, method, ignoreAttr, resultsDir);
            }
        }
    }

    private static void WriteSkippedTestResult(Type type, MethodInfo method, IgnoreAttribute ignoreAttr, string resultsDir)
    {
        var fullName = $"{type.FullName}.{method.Name}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var uuid = Guid.NewGuid().ToString();
        var description = GetDescription(method);

        var testResult = AllureTestResultBuilder.BuildTestResult(
            uuid, fullName, method.Name,
            startMs: now, stopMs: now,
            status: "skipped",
            statusMessage: ignoreAttr.Reason ?? "Ignored by [Ignore] attribute",
            description: description,
            testClassName: type.Namespace);

        AllureJsonWriter.WriteResultFile(resultsDir, testResult);
    }

    private static string? GetDescription(MethodInfo method)
    {
        var attrs = method.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attrs.Length > 0)
            return attrs[0].GetType().GetProperty("Description")?.GetValue(attrs[0]) as string;
        return null;
    }

    private static List<Assembly> GetTestAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !IsSystemAssembly(a.GetName().Name!))
            .ToList();
    }

    private static bool IsSystemAssembly(string name)
    {
        return name.StartsWith("Microsoft.") || name.StartsWith("System.") || name.StartsWith("NUnit.");
    }

    private static Type[] GetTypesSafely(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException) { return Array.Empty<Type>(); }
    }
}
