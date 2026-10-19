using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class InvitationModelResponse
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public UserModelResponse Invitee { get; set; }

    [RequiredField]
    public UserModelResponse Inviter { get; set; }

    public string Permissions { get; set; }

    public bool Expired { get; set; }

    [JsonPropertyName(JsonFields.CreatedAt)]
    public DateTime CreatedAt { get; set; }
}
