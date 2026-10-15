using Core.Attributes;

namespace Core.Models.GitHub;

public class PullRequestBranch
{
    [RequiredField]
    public string Label { get; set; }

    [RequiredField]
    public string Ref { get; set; }

    [RequiredField]
    public string Sha { get; set; }

    public RepositoryModelResponse Repo { get; set; }
}
