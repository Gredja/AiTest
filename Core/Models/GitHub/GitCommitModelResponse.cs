using Core.Attributes;

namespace Core.Models.GitHub;

public class GitCommitModelResponse
{
    [RequiredField]
    public string Sha { get; set; }

    [RequiredField]
    public GitCommitTree Tree { get; set; }
}
