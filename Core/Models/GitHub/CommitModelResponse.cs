using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CommitModelResponse
{
    [RequiredField]
    public string Sha { get; set; }

    [RequiredField]
    public CommitInfo Commit { get; set; }

    [JsonPropertyName(GitHubJsonFields.HtmlUrl)]
    public string HtmlUrl { get; set; }
}
