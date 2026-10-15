using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class RepositoryModelResponse
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Name { get; set; }

    [JsonPropertyName(JsonFields.FullName)]
    [RequiredField]
    public string FullName { get; set; }

    [RequiredField]
    public UserModelResponse Owner { get; set; }

    public string Description { get; set; }

    [JsonPropertyName(JsonFields.Private)]
    public bool IsPrivate { get; set; }

    [JsonPropertyName(JsonFields.HtmlUrl)]
    [RequiredField]
    public string HtmlUrl { get; set; }

    [JsonPropertyName(JsonFields.CreatedAt)]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName(JsonFields.UpdatedAt)]
    public DateTime UpdatedAt { get; set; }

    public string Language { get; set; }

    [JsonPropertyName(JsonFields.DefaultBranch)]
    public string DefaultBranch { get; set; }

    [JsonPropertyName(JsonFields.StargazersCount)]
    public int StargazersCount { get; set; }

    [JsonPropertyName(JsonFields.ForksCount)]
    public int ForksCount { get; set; }

    [JsonPropertyName(JsonFields.OpenIssuesCount)]
    public int OpenIssuesCount { get; set; }
}
