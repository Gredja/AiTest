using Core.Models.Generic;
using Core.Attributes;

namespace Core.Models.FakeStore;

public class CartModelResponse : IdModel<int>
{
    [PositiveId]
    public int UserId { get; set; }

    public DateTime Date { get; set; }

    [RequiredField]
    public List<CartProduct> Products { get; set; }
}
