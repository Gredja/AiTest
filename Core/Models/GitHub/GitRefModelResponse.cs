using Core.Attributes;

namespace Core.Models.GitHub;

public class GitRefModelResponse
{
    [RequiredField]
    public string Ref { get; set; }

    [RequiredField]
    public GitRefObject Object { get; set; }
}
