using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateIssueModelRequest
{
    [RequiredField]
    public string Title { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Body { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Labels { get; set; }
}
