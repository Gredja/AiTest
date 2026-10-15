using Core.Attributes;

namespace Core.Models.GitHub;

public class GitCommitTree
{
    [RequiredField]
    public string Sha { get; set; }
}
