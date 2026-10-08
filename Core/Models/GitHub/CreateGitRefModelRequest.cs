using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateGitRefModelRequest
{
    [RequiredField]
    public string Ref { get; set; }

    [RequiredField]
    public string Sha { get; set; }
}
