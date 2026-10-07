using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class UpdateIssueModelRequest
{
    [JsonPropertyName("state")]
    public string State { get; set; }
}
