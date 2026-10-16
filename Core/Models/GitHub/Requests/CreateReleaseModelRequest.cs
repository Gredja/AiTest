using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateReleaseModelRequest
{
    [JsonPropertyName(JsonFields.TagName)]
    [RequiredField]
    public string TagName { get; set; }

    public string Name { get; set; }

    [JsonPropertyName(JsonFields.Draft)]
    public bool IsDraft { get; set; }

    [JsonPropertyName(JsonFields.Prerelease)]
    public bool IsPreRelease { get; set; }
}
