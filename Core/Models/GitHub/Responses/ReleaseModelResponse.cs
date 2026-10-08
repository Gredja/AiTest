using Core.Models.Generic;
using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class ReleaseModelResponse : IdModel<long>
{
    public string Name { get; set; }

    [JsonPropertyName(JsonFields.TagName)]
    [RequiredField]
    public string TagName { get; set; }

    public string Body { get; set; }

    [JsonPropertyName(JsonFields.Draft)]
    public bool IsDraft { get; set; }

    [JsonPropertyName(JsonFields.Prerelease)]
    public bool IsPreRelease { get; set; }

    [JsonPropertyName(JsonFields.CreatedAt)]
    public DateTime CreatedAt { get; set; }
}
