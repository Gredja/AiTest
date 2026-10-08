using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateGitTreeModelRequest
{
    // snake_case wire name — camelCase policy alone would serialize this as "baseTree" and
    // GitHub would silently drop the field (scratch tree lost the whole base tree)
    [JsonPropertyName(JsonFields.BaseTree)]
    [RequiredField]
    public string BaseTree { get; set; }

    [RequiredField]
    public List<GitTreeEntry> Tree { get; set; }
}
