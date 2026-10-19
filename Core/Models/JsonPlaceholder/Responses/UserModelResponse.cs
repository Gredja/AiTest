using Core.Attributes;
using Core.Models.Generic;

namespace Core.Models.JsonPlaceholder;

public class UserModelResponse : IdModel<int>
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public string Username { get; set; }

    [RequiredField]
    public string Email { get; set; }

    [RequiredField]
    public Address Address { get; set; }

    [RequiredField]
    public string Phone { get; set; }

    [RequiredField]
    public string Website { get; set; }

    [RequiredField]
    public Company Company { get; set; }
}
