using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CommitModel
{
    [RequiredField]
    public string Sha { get; set; }

    [RequiredField]
    public CommitInfoModel Commit { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }
}
