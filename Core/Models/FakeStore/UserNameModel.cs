using Core.Attributes;

namespace Core.Models.FakeStore;

public class UserNameModel
{
    [RequiredField]
    public string Firstname { get; set; }

    [RequiredField]
    public string Lastname { get; set; }
}
