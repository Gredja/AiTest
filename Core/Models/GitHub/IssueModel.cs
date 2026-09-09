using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class IssueModel
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Title { get; set; }

    public string Body { get; set; }

    [RequiredField]
    public string State { get; set; }

    [RequiredField]
    public UserModel User { get; set; }

    public List<LabelModel> Labels { get; set; }

    public UserModel Assignee { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
