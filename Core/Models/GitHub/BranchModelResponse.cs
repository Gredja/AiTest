using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class BranchModelResponse
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public BranchCommit Commit { get; set; }

    [JsonPropertyName("protected")]
    public bool IsProtected { get; set; }
}
