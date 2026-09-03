namespace Core.Models;

public class CartModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProductModel> Products { get; set; }
}
