using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class CommentModel
{
    [PositiveId]
    public int Id { get; set; }

    public int PostId { get; set; }

    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public string Email { get; set; }

    [RequiredField]
    public string Body { get; set; }
}
