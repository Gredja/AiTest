using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class UpdatePullRequestModelRequest
{
    // PATCH sends an arbitrary subset — null fields must be omitted (mirror of UpdateIssueModelRequest)
    [JsonPropertyName(JsonFields.State)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string State { get; set; }

    [JsonPropertyName(JsonFields.Title)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Title { get; set; }
}
