using Core.Attributes;

namespace Core.Models.GitHub;

public class TagModel
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public BranchCommitModel Commit { get; set; }
}
