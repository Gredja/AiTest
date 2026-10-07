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

    [JsonPropertyName("blob_url")]
    public string BlobUrl { get; set; }

    [JsonPropertyName("raw_url")]
    public string RawUrl { get; set; }

    [JsonPropertyName("contents_url")]
    public string ContentsUrl { get; set; }

    public string Patch { get; set; }
}
