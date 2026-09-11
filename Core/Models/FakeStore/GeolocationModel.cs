using System.Text.Json.Serialization;
using Core.Attributes;

namespace Core.Models.FakeStore;

public class GeolocationModel
{
    [RequiredField]
    public string Lat { get; set; }

    [JsonPropertyName("long")]
    [RequiredField]
    public string Longitude { get; set; }
}
