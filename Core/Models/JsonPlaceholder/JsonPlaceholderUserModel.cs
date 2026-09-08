namespace Core.Models.JsonPlaceholder;

public class JsonPlaceholderUserModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public JsonPlaceholderAddressModel Address { get; set; }

    public string Phone { get; set; }

    public string Website { get; set; }

    public CompanyModel Company { get; set; }
}
