using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.Generic;

public class ErrorMessageModelResponse
{
    [RequiredField]
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("documentation_url")]
    public string DocumentationUrl { get; set; }
}
