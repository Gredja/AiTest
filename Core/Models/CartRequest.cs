namespace Core.Models;

public class CartRequest
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProductModel> Products { get; set; }
}
