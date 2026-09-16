using Core.Models.Generic;
using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class IssueModelResponse : IdModel<long>
{
    [RequiredField]
    public string Title { get; set; }

    public string Body { get; set; }

    [RequiredField]
    public string State { get; set; }

    [RequiredField]
    public UserModelResponse User { get; set; }

    public List<LabelModelResponse> Labels { get; set; }

    public UserModelResponse Assignee { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
