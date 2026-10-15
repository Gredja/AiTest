using System.Text.Json.Serialization;

namespace Core.Models.GitHub;

public class PullRequestMergeModelResponse
{
    [JsonPropertyName(JsonFields.Merged)]
    public bool Merged { get; set; }

    [JsonPropertyName(JsonFields.Message)]
    public string Message { get; set; }
}
