using Core.Attributes;

namespace Core.Models;

public class LoginResponse
{
    [RequiredField]
    public string Token { get; set; }
}
