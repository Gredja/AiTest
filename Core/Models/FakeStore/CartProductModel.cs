using Core.Attributes;

namespace Core.Models.FakeStore;

public class CartProductModel
{
    [PositiveId]
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
