namespace Core.Models.FakeStore;

public class UserRequest
{
    public string Email { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public UserName Name { get; set; }

    public Address Address { get; set; }

    public string Phone { get; set; }
}
