using Core.Models.Generic;
using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class ContributorModelResponse : IdModel<long>
{
    [RequiredField]
    public string Login { get; set; }

    [JsonPropertyName(JsonFields.AvatarUrl)]
    public string AvatarUrl { get; set; }

    [JsonPropertyName(JsonFields.HtmlUrl)]
    public string HtmlUrl { get; set; }

    [RequiredField]
    public int Contributions { get; set; }
}
