using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class PermissionModelResponse
{
    [RequiredField]
    public string Permission { get; set; }

    [JsonPropertyName(JsonFields.RoleName)]
    public string RoleName { get; set; }

    [RequiredField]
    public UserModelResponse User { get; set; }
}
