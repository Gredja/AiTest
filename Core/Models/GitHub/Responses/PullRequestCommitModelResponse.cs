using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class PullRequestCommitModelResponse
{
    [RequiredField]
    public string Sha { get; set; }

    [RequiredField]
    public CommitInfo Commit { get; set; }

    [JsonPropertyName(JsonFields.HtmlUrl)]
    public string HtmlUrl { get; set; }
}
