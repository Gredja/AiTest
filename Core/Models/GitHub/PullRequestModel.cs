using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class PullRequestModel
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

    public PullRequestBranchModel Head { get; set; }

    public PullRequestBranchModel Base { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }
}
