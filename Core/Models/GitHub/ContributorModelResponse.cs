using Core.Models.Generic;
using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class ContributorModelResponse : IdModel<long>
{
    [RequiredField]
    public string Login { get; set; }

    [JsonPropertyName(GitHubJsonFields.AvatarUrl)]
    public string AvatarUrl { get; set; }

    [JsonPropertyName(GitHubJsonFields.HtmlUrl)]
    public string HtmlUrl { get; set; }

    [RequiredField]
    public int Contributions { get; set; }
}
