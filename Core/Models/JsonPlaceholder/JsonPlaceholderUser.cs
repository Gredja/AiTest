using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class JsonPlaceholderUser
{
    [PositiveId]
    public int Id { get; set; }

    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public string Username { get; set; }

    [RequiredField]
    public string Email { get; set; }

    [RequiredField]
    public JsonPlaceholderAddress Address { get; set; }

    [RequiredField]
    public string Phone { get; set; }

    [RequiredField]
    public string Website { get; set; }

    [RequiredField]
    public Company Company { get; set; }
}
