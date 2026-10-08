using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class UserModelResponse
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Login { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    [JsonPropertyName(JsonFields.AvatarUrl)]
    public string AvatarUrl { get; set; }

    public string Bio { get; set; }

    [JsonPropertyName(JsonFields.PublicRepos)]
    public int? PublicRepos { get; set; }

    public int? Followers { get; set; }

    public int? Following { get; set; }
}
