using System.Text.Json.Serialization;
using Core.Models.Generic;

namespace Core.Models.JsonPlaceholder;

public class TodoModel : UserOwnedModel
{
    [JsonPropertyName("completed")]
    public bool IsCompleted { get; set; }
}
