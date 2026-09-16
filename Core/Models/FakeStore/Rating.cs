using Core.Attributes;

namespace Core.Models.FakeStore;

public class Rating
{
    [ValueRange(0, 5)]
    public double Rate { get; set; }

    [ValueRange(0)]
    public int Count { get; set; }
}
