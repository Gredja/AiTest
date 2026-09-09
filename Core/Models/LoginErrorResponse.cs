using Core.Attributes;

namespace Core.Models;

public class LoginErrorResponse
{
    [RequiredField]
    public string Error { get; set; }
}
