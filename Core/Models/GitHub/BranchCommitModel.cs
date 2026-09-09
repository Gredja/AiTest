using Core.Attributes;

namespace Core.Models.GitHub;

public class BranchCommitModel
{
    [RequiredField]
    public string Sha { get; set; }

    public string Url { get; set; }
}
