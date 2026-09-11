using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class ContributorModel
{
    [RequiredField]
    public string Login { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }

    [RequiredField]
    public int Contributions { get; set; }
}
