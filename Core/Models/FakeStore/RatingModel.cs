using Core.Attributes;

namespace Core.Models.FakeStore;

public class RatingModel
{
    [ValueRange(0, 5)]
    public double Rate { get; set; }

    [ValueRange(0)]
    public int Count { get; set; }
}
