namespace Core.Models.FakeStore;

public class CartRequest
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProductModel> Products { get; set; }
}
