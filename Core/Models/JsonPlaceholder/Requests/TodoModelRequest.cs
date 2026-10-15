using System.Text.Json.Serialization;

namespace Core.Models.JsonPlaceholder;

public class TodoModelRequest
{
    [JsonPropertyName(JsonFields.Completed)]
    public bool IsCompleted { get; set; }

    public string Title { get; set; }

    public int UserId { get; set; }
}
