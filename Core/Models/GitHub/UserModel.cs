using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class UserModel
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Login { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    public string Bio { get; set; }

    [JsonPropertyName("public_repos")]
    public int PublicRepos { get; set; }

    public int Followers { get; set; }

    public int Following { get; set; }
}
