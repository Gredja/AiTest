using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.GitHub;

public class BranchModel
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public BranchCommitModel Commit { get; set; }

    [JsonPropertyName("protected")]
    public bool IsProtected { get; set; }
}
