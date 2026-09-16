using Core.Attributes;

namespace Core.Models.GitHub;

public class CommitInfo
{
    [RequiredField]
    public string Message { get; set; }

    public CommitAuthor Author { get; set; }
}
