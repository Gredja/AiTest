using Core.Attributes;

namespace Core.Models.GitHub;

public class BranchCommit
{
    [RequiredField]
    public string Sha { get; set; }

    public string Url { get; set; }
}
