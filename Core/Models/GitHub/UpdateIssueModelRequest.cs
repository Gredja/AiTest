using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class UpdateIssueModelRequest
{
    [JsonPropertyName(JsonFields.State)]
    public string State { get; set; }
}
