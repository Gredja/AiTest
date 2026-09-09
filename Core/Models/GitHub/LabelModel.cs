using Core.Attributes;

namespace Core.Models.GitHub;

public class LabelModel
{
    [PositiveId]
    public long Id { get; set; }

    [RequiredField]
    public string Name { get; set; }

    public string Description { get; set; }

    [RequiredField]
    public string Color { get; set; }
}
