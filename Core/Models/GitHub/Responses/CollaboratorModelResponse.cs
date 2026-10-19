using Core.Attributes;
using Core.Models.Generic;
using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class CollaboratorModelResponse : IdModel<long>
{
    [RequiredField]
    public string Login { get; set; }

    public string Type { get; set; }

    [JsonPropertyName(JsonFields.RoleName)]
    public string RoleName { get; set; }

    public CollaboratorPermissions Permissions { get; set; }
}
