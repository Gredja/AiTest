using Core.Attributes;

namespace Core.Models.GitHub;

public class CreatePullRequestModelRequest
{
    [RequiredField]
    public string Title { get; set; }

    [RequiredField]
    public string Head { get; set; }

    [RequiredField]
    public string Base { get; set; }
}
