using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateGitCommitModelRequest
{
    [RequiredField]
    public string Message { get; set; }

    [RequiredField]
    public string Tree { get; set; }

    [RequiredField]
    public List<string> Parents { get; set; }
}
