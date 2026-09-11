using Core.Attributes;

namespace Core.Models.GitHub;

public class CommitInfoModel
{
    [RequiredField]
    public string Message { get; set; }

    public CommitAuthorModel Author { get; set; }
}
