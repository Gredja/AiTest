using Core.Attributes;

namespace Core.Models.GitHub;

public class PullRequestBranchModel
{
    [RequiredField]
    public string Label { get; set; }

    [RequiredField]
    public string Ref { get; set; }

    [RequiredField]
    public string Sha { get; set; }

    public RepositoryModel Repo { get; set; }
}
