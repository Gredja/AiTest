using Core.Attributes;

namespace Core.Models.GitHub;

public class GitTreeEntry
{
    [RequiredField]
    public string Path { get; set; }

    [RequiredField]
    public string Mode { get; set; }

    [RequiredField]
    public string Type { get; set; }

    [RequiredField]
    public string Sha { get; set; }
}
