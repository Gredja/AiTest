using Core.Attributes;
using Core.Models.Generic;
using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class PullRequestModelResponse : IdModel<long>
{
    [PositiveId]
    public int Number { get; set; }

    [RequiredField]
    public string Title { get; set; }

    public string Body { get; set; }

    [RequiredField]
    public string State { get; set; }

    [RequiredField]
    public UserModelResponse User { get; set; }

    public PullRequestBranch Head { get; set; }

    public PullRequestBranch Base { get; set; }

    [JsonPropertyName(JsonFields.CreatedAt)]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName(JsonFields.MergedAt)]
    public DateTime? MergedAt { get; set; }
}
