using Core.Attributes;

namespace Core.Models.GitHub;

public class GitShaModelResponse
{
    [RequiredField]
    public string Sha { get; set; }
}
