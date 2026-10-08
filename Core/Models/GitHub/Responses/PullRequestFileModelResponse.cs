using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class PullRequestFileModelResponse
{
    [RequiredField]
    public string Sha { get; set; }

    [RequiredField]
    public string Filename { get; set; }

    [RequiredField]
    public string Status { get; set; }

    [ValueRange(0)]
    public int Additions { get; set; }

    [ValueRange(0)]
    public int Deletions { get; set; }

    [ValueRange(0)]
    public int Changes { get; set; }

    [JsonPropertyName(JsonFields.BlobUrl)]
    public string BlobUrl { get; set; }

    [JsonPropertyName(JsonFields.RawUrl)]
    public string RawUrl { get; set; }

    [JsonPropertyName(JsonFields.ContentsUrl)]
    public string ContentsUrl { get; set; }

    public string Patch { get; set; }
}
