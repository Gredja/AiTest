using Core.Attributes;

namespace Core.Models;

public class ProductModel
{
    [PositiveId]
    public int Id { get; set; }

    [RequiredField]
    public string Title { get; set; }

    [ValueRange(0)]
    public decimal Price { get; set; }

    [RequiredField]
    public string Description { get; set; }

    [RequiredField]
    public string Category { get; set; }

    [RequiredField]
    public string Image { get; set; }

    [RequiredField]
    public RatingModel Rating { get; set; }
}
