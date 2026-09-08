using Core.Attributes;

namespace Core.Models.Generic;

public class UserOwnedModel
{
    [PositiveId]
    public int Id { get; set; }

    public int UserId { get; set; }

    [RequiredField]
    public string Title { get; set; }
}
