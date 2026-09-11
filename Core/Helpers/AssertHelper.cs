using System.Net;
using System.Reflection;
using Core.Attributes;
using FluentAssertions;
using RestSharp;

namespace Core.Helpers;

public static class AssertHelper
{
    public const string JsonContentType = "application/json";
    public static void ShouldBeOkWithData<T>(this RestResponse<T> response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
    }

    public static void ShouldHaveStatusCode<T>(this RestResponse<T> response, HttpStatusCode expected)
    {
        response.StatusCode.Should().Be(expected);
    }

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
