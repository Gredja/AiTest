using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class JsonPlaceholderUserModel
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
    public JsonPlaceholderAddressModel Address { get; set; }

    [RequiredField]
    public string Phone { get; set; }

    [RequiredField]
    public string Website { get; set; }

    [RequiredField]
    public CompanyModel Company { get; set; }
}
