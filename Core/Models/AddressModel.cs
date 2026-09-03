namespace Core.Models;

public class AddressModel
{
    public GeolocationModel Geolocation { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public int Number { get; set; }
    public string Zipcode { get; set; }
}
