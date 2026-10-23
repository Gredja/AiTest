using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateCommentModelRequest
{
    [RequiredField]
    public string Body { get; set; }
}
