using Core.Attributes;

namespace Core.Models.GitHub;

public class GitRefObject
{
    [RequiredField]
    public string Sha { get; set; }

    public string Type { get; set; }
}
