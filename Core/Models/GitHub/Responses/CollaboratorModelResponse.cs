using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CollaboratorModelResponse
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Login { get; set; }

    public string Type { get; set; }

    [JsonPropertyName(JsonFields.RoleName)]
    public string RoleName { get; set; }

    public CollaboratorPermissions Permissions { get; set; }
}
