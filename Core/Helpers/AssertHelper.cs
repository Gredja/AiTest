using System.Net;
using System.Reflection;
using Core.Attributes;
using FluentAssertions;
using RestSharp;

namespace Core.Helpers;

public static class AssertHelper
{
    public const string JsonContentType = "application/json";

    public static void ShouldHaveStatusCode(this RestResponse response, HttpStatusCode expected) =>
        response.StatusCode.Should().Be(expected);

    public static void ShouldHaveValidContract<T>(this T entity) where T : class
    {
        entity.Should().BeJsonSerializable();
        entity.ShouldHaveValidFields();
    }

    public static void ShouldHaveValidFields<T>(this T entity) where T : class
    {
        entity.Should().NotBeNull();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(entity);

            foreach (var attribute in property.GetCustomAttributes())
            {
                ValidateProperty(property.Name, value, attribute);
            }
        }
    }

    public static void ShouldMatchRequest<TRequest, TResponse>(this TResponse response, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        response.Should().NotBeNull();
        request.Should().NotBeNull();

        var requestProperties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var responseProperties = typeof(TResponse).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var requestProperty in requestProperties)
        {
            var requestValue = requestProperty.GetValue(request);
            if (requestValue is null)
            {
                continue;
            }

            var responseProperty = responseProperties.FirstOrDefault(property =>
                property.Name.Equals(requestProperty.Name, StringComparison.OrdinalIgnoreCase));

            responseProperty.Should().NotBeNull($"response should have property '{requestProperty.Name}' matching request");

            var responseValue = responseProperty!.GetValue(response);
            responseValue.Should().Be(requestValue, $"response.{responseProperty.Name} should match request.{requestProperty.Name}");
        }
    }

    private static void ValidateProperty(string name, object? value, Attribute attribute)
    {
        switch (attribute)
        {
            case RequiredFieldAttribute:
                value.Should().NotBeNull($"{name} is marked [RequiredField]");
                if (value is string text)
                {
                    text.Should().NotBeNullOrWhiteSpace($"{name} is marked [RequiredField]");
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
