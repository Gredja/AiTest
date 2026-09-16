using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class JsonPlaceholderAddress
{
    [RequiredField]
    public string Street { get; set; }

    [RequiredField]
    public string Suite { get; set; }

    [RequiredField]
    public string City { get; set; }

    [RequiredField]
    public string Zipcode { get; set; }

    [RequiredField]
    public Geo Geo { get; set; }
}
