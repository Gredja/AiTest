using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class GeoModel
{
    [RequiredField]
    public string Lat { get; set; }

    [RequiredField]
    public string Lng { get; set; }
}
