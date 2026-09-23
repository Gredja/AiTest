using Core.Attributes;

namespace Core.Models.FakeStore;

public class AuthModelResponse
{
    [RequiredField]
    public string Token { get; set; }
}
