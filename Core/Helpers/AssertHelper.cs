using System.Net;
using System.Reflection;
using Core.Attributes;
using FluentAssertions;
using RestSharp;

namespace Core.Helpers;

public static class AssertHelper
{
    public const string JsonContentType = "application/json";

    public static void ShouldHaveStatusCode<T>(this RestResponse<T> response, HttpStatusCode expected) =>
        response.StatusCode.Should().Be(expected);

    public static void ShouldHaveValidFields<T>(this T entity) where T : class
    {
        entity.Should().NotBeNull();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(entity);

            foreach (var attr in prop.GetCustomAttributes())
            {
                ValidateProperty(prop.Name, value, attr);
            }
        }
    }

    public static void ShouldMatchRequest<TRequest, TResponse>(this TResponse response, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        response.Should().NotBeNull();
        request.Should().NotBeNull();

        var requestProps = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var responseProps = typeof(TResponse).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var reqProp in requestProps)
        {
            var reqValue = reqProp.GetValue(request);
            if (reqValue is null)
            {
                continue;
            }

            var resProp = responseProps.FirstOrDefault(p =>
                p.Name.Equals(reqProp.Name, StringComparison.OrdinalIgnoreCase));

            resProp.Should().NotBeNull($"response should have property '{reqProp.Name}' matching request");

            var resValue = resProp!.GetValue(response);
            resValue.Should().Be(reqValue, $"response.{resProp.Name} should match request.{reqProp.Name}");
        }
    }

    private static void ValidateProperty(string name, object? value, Attribute attr)
    {
        switch (attr)
        {
            case RequiredFieldAttribute:
                value.Should().NotBeNull($"{name} is marked [RequiredField]");
                if (value is string str)
                {
                    str.Should().NotBeNullOrWhiteSpace($"{name} is marked [RequiredField]");
                }
                break;

            case PositiveIdAttribute:
                value.Should().NotBeNull($"{name} is marked [PositiveId]");
                Convert.ToDouble(value).Should().BeGreaterThan(0, $"{name} is marked [PositiveId]");
                break;

            case ValueRangeAttribute range:
                value.Should().NotBeNull($"{name} is marked [ValueRange]");
                var doubleValue = Convert.ToDouble(value);
                if (range.Min != double.MinValue)
                {
                    doubleValue.Should().BeGreaterThanOrEqualTo(range.Min, $"{name} min is {range.Min}");
                }
                if (range.Max != double.MaxValue)
                {
                    doubleValue.Should().BeLessThanOrEqualTo(range.Max, $"{name} max is {range.Max}");
                }
                break;
        }
    }
}
