using Core.Attributes;

namespace Core.Models;

public class CartProductModel
{
    [PositiveId]
    public int ProductId { get; set; }

    [ValueRange(1)]
    public int Quantity { get; set; }
}
