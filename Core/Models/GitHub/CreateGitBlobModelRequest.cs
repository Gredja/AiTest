using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateGitBlobModelRequest
{
    [RequiredField]
    public string Content { get; set; }
}
