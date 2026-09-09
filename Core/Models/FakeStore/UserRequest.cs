namespace Core.Models.FakeStore;

public class UserRequest
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public UserNameModel Name { get; set; }
    public AddressModel Address { get; set; }
    public string Phone { get; set; }
}
