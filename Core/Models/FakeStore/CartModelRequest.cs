namespace Core.Models.FakeStore;

public class CartModelRequest
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProduct> Products { get; set; }
}
