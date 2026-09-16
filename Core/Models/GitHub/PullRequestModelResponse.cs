using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class PullRequestModelResponse
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Title { get; set; }

    public string Body { get; set; }

    [RequiredField]
    public string State { get; set; }

    [RequiredField]
    public UserModelResponse User { get; set; }

    public PullRequestBranch Head { get; set; }

    public PullRequestBranch Base { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }
}
