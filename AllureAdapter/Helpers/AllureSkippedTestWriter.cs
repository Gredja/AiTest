using System.Reflection;
using NUnit.Framework;

namespace AllureAdapter.Helpers;

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

                var ignoreAttribute = method.GetCustomAttribute<IgnoreAttribute>();
                if (ignoreAttribute is null)
                {
                    continue;
                }

                WriteSkippedTestResult(type, method, ignoreAttribute, resultsDir);
            }
        }
    }

    private static void WriteSkippedTestResult(Type type, MethodInfo method, IgnoreAttribute ignoreAttribute, string resultsDir)
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
            StatusMessage: ignoreAttribute.Reason ?? AllureConstants.DefaultIgnoreMessage,
            Description: description,
            TestClassName: type.FullName,
            Categories: GetCategories(type, method)));

        AllureJsonWriter.WriteResultFile(resultsDir, testResult);
    }

    private static List<string>? GetCategories(Type type, MethodInfo method)
    {
        var categories = ReadCategories(method).Concat(ReadCategories(type))
            .Select(category => category.Trim())
            .Where(category => category.Length > 0)
            .Distinct()
            .ToList();

        return categories.Count > 0 ? categories : null;
    }

    private static IEnumerable<string> ReadCategories(MemberInfo member) =>
        member.GetCustomAttributes(typeof(CategoryAttribute), true)
            .Cast<CategoryAttribute>()
            .SelectMany(attribute => attribute.Name.Split(','));

    private static Type[] GetTypesSafely(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException exception) { return exception.Types.Where(type => type is not null).ToArray()!; }
    }
}
