using Core.Attributes;

namespace Core.Models.GitHub;

public class BranchModel
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public BranchCommit Commit { get; set; }

    public bool Protected { get; set; }
}

public class BranchCommit
{
    [RequiredField]
    public string Sha { get; set; }

    public string Url { get; set; }
}
