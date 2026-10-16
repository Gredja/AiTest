using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class SearchModelResponse<T>
{
    [JsonPropertyName(JsonFields.TotalCount)]
    public int TotalCount { get; set; }

    [JsonPropertyName(JsonFields.IncompleteResults)]
    public bool IncompleteResults { get; set; }

    [RequiredField]
    public List<T> Items { get; set; }
}
