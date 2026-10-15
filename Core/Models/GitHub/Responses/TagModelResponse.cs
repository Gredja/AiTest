using Core.Attributes;

namespace Core.Models.GitHub;

public class TagModelResponse
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public BranchCommit Commit { get; set; }
}
