using System.Net;
using System.Reflection;
using System.Text.Json;
using Core.Attributes;
using Core.Models.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using RestSharp;

namespace Core.Helpers;

public static class AssertHelper
{
    public const string JsonContentType = "application/json";

    public static void ShouldHaveStatusCode(this RestResponse response, HttpStatusCode expected) =>
        response.StatusCode.Should().Be(expected);

    public static ErrorMessageModelResponse ShouldHaveError(
        this RestResponse response, HttpStatusCode expected, string expectedMessage)
    {
        response.StatusCode.Should().Be(expected);

        var content = response.Content;
        content.Should().NotBeNullOrWhiteSpace("error response must carry a body");

        var error = JsonSerializer.Deserialize<ErrorMessageModelResponse>(content!);
        error.Should().NotBeNull("error body must be valid JSON");
        error!.Message.Should().NotBeNullOrWhiteSpace("error message must be readable and explain the problem");
        error.Message.Should().Be(expectedMessage, "error message must match the documented Observable Behaviour value");

        return error;
    }

    public static void ShouldHaveValidContract<T>(this T entity) where T : class
    {
        // Soft: serializability + every field violation reported in one failure, not one per rerun
        using (new AssertionScope())
        {
            entity.Should().BeJsonSerializable();
            entity.ShouldHaveValidFields();
        }
    }

    public static void ShouldHaveValidFields<T>(this T entity) where T : class
    {
        entity.Should().NotBeNull();

        using (new AssertionScope())
        {
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
    }

    public static void ShouldMatchRequest<TRequest, TResponse>(this TResponse response, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        response.Should().NotBeNull();
        request.Should().NotBeNull();

        var requestProperties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var responseProperties = typeof(TResponse).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        using (new AssertionScope())
        {
            foreach (var requestProperty in requestProperties)
            {
                var requestValue = requestProperty.GetValue(request);
                if (requestValue is null)
                {
                    continue;
                }

                var responseProperty = responseProperties.FirstOrDefault(property =>
                    property.Name.Equals(requestProperty.Name, StringComparison.OrdinalIgnoreCase));

                if (responseProperty is null)
                {
                    responseProperty.Should().NotBeNull(
                        $"response should have property '{requestProperty.Name}' matching request");
                    continue;
                }

                var responseValue = responseProperty.GetValue(response);
                CompareValues(responseValue, requestValue, responseProperty.Name);
            }
        }
    }

    private static void CompareValues(object? responseValue, object? requestValue, string propertyName)
    {
        var because = $"response.{propertyName} should match request.{propertyName}";

        if (requestValue is string or ValueType)
        {
            responseValue.Should().Be(requestValue, because);
            return;
        }

        // Collections and nested objects: Be() is reference equality for them — compare structurally
        responseValue.Should().BeEquivalentTo(requestValue, because);
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
