using Core.Models.Generic;
using Core.Attributes;

namespace Core.Models.FakeStore;

public class ProductModelResponse : IdModel<int>
{
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
    public Rating Rating { get; set; }
}
