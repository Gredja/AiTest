using Core.Attributes;

namespace Core.Models.FakeStore;

public class CartModel
{
    [PositiveId]
    public int Id { get; set; }

    [PositiveId]
    public int UserId { get; set; }

    public DateTime Date { get; set; }

    [RequiredField]
    public List<CartProductModel> Products { get; set; }
}
