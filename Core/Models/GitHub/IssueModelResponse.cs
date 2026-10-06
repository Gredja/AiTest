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

    [PositiveId]
    public int Number { get; set; }

    [RequiredField]
    public UserModelResponse User { get; set; }

    public List<Label> Labels { get; set; }

    public UserModelResponse Assignee { get; set; }

    [JsonPropertyName(JsonFields.CreatedAt)]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName(JsonFields.UpdatedAt)]
    public DateTime UpdatedAt { get; set; }
}
