using Core.Attributes;

namespace Core.Models;

public class LoginRequest
{
    [RequiredField]
    public string Username { get; set; }

    [RequiredField]
    public string Password { get; set; }
}
