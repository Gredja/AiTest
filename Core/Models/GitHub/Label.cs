using Core.Models.Generic;
using Core.Attributes;

namespace Core.Models.GitHub;

public class LabelModelResponse : IdModel<long>
{
    [RequiredField]
    public string Name { get; set; }

    public string Description { get; set; }

    [RequiredField]
    public string Color { get; set; }
}
