using Core.Models.Generic;
using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CommentModelResponse : IdModel<long>
{
    [RequiredField]
    public string Body { get; set; }

    [RequiredField]
    public UserModelResponse User { get; set; }

    [JsonPropertyName(JsonFields.CreatedAt)]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName(JsonFields.UpdatedAt)]
    public DateTime UpdatedAt { get; set; }
}
