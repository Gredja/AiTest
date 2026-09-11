using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class RepositoryModel
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Name { get; set; }

    [JsonPropertyName("full_name")]
    [RequiredField]
    public string FullName { get; set; }

    [RequiredField]
    public UserModel Owner { get; set; }

    public string Description { get; set; }

    [JsonPropertyName("private")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("html_url")]
    [RequiredField]
    public string HtmlUrl { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public string Language { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int StargazersCount { get; set; }

    [JsonPropertyName("forks_count")]
    public int ForksCount { get; set; }

    [JsonPropertyName("open_issues_count")]
    public int OpenIssuesCount { get; set; }
}
