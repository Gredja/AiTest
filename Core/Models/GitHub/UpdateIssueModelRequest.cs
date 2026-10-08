using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class UpdateIssueModelRequest
{
    // PATCH sends an arbitrary subset — null fields must be omitted, not sent as JSON null (OB §19)
    [JsonPropertyName(JsonFields.State)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string State { get; set; }

    [JsonPropertyName(JsonFields.Title)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Title { get; set; }
}
