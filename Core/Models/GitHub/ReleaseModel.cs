using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class ReleaseModel
{
    [PositiveId]
    public long Id { get; set; }

    public string Name { get; set; }

    [JsonPropertyName("tag_name")]
    [RequiredField]
    public string TagName { get; set; }

    public string Body { get; set; }

    public bool Draft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool IsPreRelease { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
