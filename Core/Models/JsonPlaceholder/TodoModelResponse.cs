using System.Text.Json.Serialization;
using Core.Models.Generic;

namespace Core.Models.JsonPlaceholder;

public class TodoModelResponse : UserOwnedModel
{
    [JsonPropertyName(JsonPlaceholderJsonFields.Completed)]
    public bool IsCompleted { get; set; }
}
