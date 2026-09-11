using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CommentModel
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Body { get; set; }

    [RequiredField]
    public UserModel User { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
