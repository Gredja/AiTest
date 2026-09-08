using System.Net;
using System.Reflection;
using Core.Attributes;
using FluentAssertions;
using RestSharp;

namespace Core.Helpers;

public static class AssertHelper
{
    public static void ShouldBeOk<T>(this RestResponse<T> response)
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

            if (prop.GetCustomAttribute<RequiredFieldAttribute>() != null)
            {
                value.Should().NotBeNull($"{prop.Name} is marked [RequiredField]");
                if (value is string str)
                {
                    str.Should().NotBeNullOrWhiteSpace($"{prop.Name} is marked [RequiredField]");
                }
            }

            if (prop.GetCustomAttribute<PositiveIdAttribute>() != null)
            {
                value.Should().NotBeNull($"{prop.Name} is marked [PositiveId]");
                Convert.ToDouble(value).Should().BeGreaterThan(0, $"{prop.Name} is marked [PositiveId]");
            }

            var range = prop.GetCustomAttribute<ValueRangeAttribute>();
            if (range != null)
            {
                value.Should().NotBeNull($"{prop.Name} is marked [ValueRange]");
                var doubleValue = Convert.ToDouble(value);
                if (range.Min != double.MinValue)
                {
                    doubleValue.Should().BeGreaterThanOrEqualTo(range.Min, $"{prop.Name} min is {range.Min}");
                }

                if (range.Max != double.MaxValue)
                {
                    doubleValue.Should().BeLessThanOrEqualTo(range.Max, $"{prop.Name} max is {range.Max}");
                }
            }
        }
    }
}
