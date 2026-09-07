using Core.Attributes;

namespace Core.Models;

public class UserModel
{
    [PositiveId]
    public int Id { get; set; }

    [RequiredField]
    public string Email { get; set; }

    [RequiredField]
    public string Username { get; set; }

    [RequiredField]
    public string Password { get; set; }

    [RequiredField]
    public UserNameModel Name { get; set; }

    [RequiredField]
    public string Phone { get; set; }

    [RequiredField]
    public AddressModel Address { get; set; }
}
