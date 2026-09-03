namespace Core.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public UserNameModel Name { get; set; }
    public AddressModel Address { get; set; }
    public string Phone { get; set; }
}
