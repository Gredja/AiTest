using Core.Models.Generic;

namespace Core.Models.JsonPlaceholder;

public class TodoModel : UserOwnedModel
{
    public bool Completed { get; set; }
}
