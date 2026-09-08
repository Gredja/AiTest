namespace Core.Models.JsonPlaceholder;

public class JsonPlaceholderAddressModel
{
    public string Street { get; set; }

    public string Suite { get; set; }

    public string City { get; set; }

    public string Zipcode { get; set; }

    public GeoModel Geo { get; set; }
}
