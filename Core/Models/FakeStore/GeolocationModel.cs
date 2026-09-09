using Core.Attributes;

namespace Core.Models.FakeStore;

public class GeolocationModel
{
    [RequiredField]
    public string Lat { get; set; }

    [RequiredField]
    public string Long { get; set; }
}
