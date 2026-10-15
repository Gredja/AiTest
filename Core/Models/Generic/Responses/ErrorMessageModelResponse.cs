using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.Generic;

public class ErrorMessageModelResponse
{
    [RequiredField]
    [JsonPropertyName(JsonFields.Message)]
    public string Message { get; set; }

    [JsonPropertyName(JsonFields.DocumentationUrl)]
    public string DocumentationUrl { get; set; }
}
