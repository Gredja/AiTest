using Core.Attributes;
using Core.Models.Generic;
using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class InvitationModelResponse : IdModel<long>
{
    [RequiredField]
    public UserModelResponse Invitee { get; set; }

    [RequiredField]
    public UserModelResponse Inviter { get; set; }

    public string Permissions { get; set; }

    [JsonPropertyName(JsonFields.Expired)]
    public bool IsExpired { get; set; }

    [JsonPropertyName(JsonFields.CreatedAt)]
    public DateTime CreatedAt { get; set; }
}
