using Core.Models.Generic;
using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class Comment : IdModel<long>
{
    [RequiredField]
    public string Body { get; set; }

    [RequiredField]
    public UserModelResponse User { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
