using Core.Attributes;

namespace Core.Models.Generic;

public class ShaModel
{
    [RequiredField]
    public string Sha { get; set; }
}
