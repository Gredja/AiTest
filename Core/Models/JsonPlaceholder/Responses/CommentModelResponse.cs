using Core.Attributes;
using Core.Models.Generic;

namespace Core.Models.JsonPlaceholder;

public class CommentModelResponse : IdModel<int>
{
    public int PostId { get; set; }

    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public string Email { get; set; }

    [RequiredField]
    public string Body { get; set; }
}
