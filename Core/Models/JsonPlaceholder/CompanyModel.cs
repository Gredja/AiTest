using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class CompanyModel
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public string CatchPhrase { get; set; }

    [RequiredField]
    [JsonPropertyName("bs")]
    public string BusinessSlogan { get; set; }
}
