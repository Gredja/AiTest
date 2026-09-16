using Core.Models.Generic;
using Core.Attributes;

namespace Core.Models.FakeStore;

public class UserModelResponse : IdModel<int>
{
    [RequiredField]
    public string Email { get; set; }

    [RequiredField]
    public string Username { get; set; }

    [RequiredField]
    public string Password { get; set; }

    [RequiredField]
    public UserName Name { get; set; }

    [RequiredField]
    public string Phone { get; set; }

    [RequiredField]
    public Address Address { get; set; }
}
