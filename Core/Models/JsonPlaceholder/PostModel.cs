using Core.Models.Generic;

namespace Core.Models.JsonPlaceholder;

public class PostModel : UserOwnedModel
{
    public string Body { get; set; }
}
