using Core.Attributes;

namespace Core.Models.FakeStore;

public class AddressModel
{
    [RequiredField]
    public GeolocationModel Geolocation { get; set; }

    [RequiredField]
    public string City { get; set; }

    [RequiredField]
    public string Street { get; set; }

    public int Number { get; set; }

    [RequiredField]
    public string Zipcode { get; set; }
}
