using Core.Helpers;
using Core.Models;
using FluentAssertions;
using RestSharp;

namespace Api.Helpers;

public static class ProductAssertHelper
{
    public static void ShouldBeOkWithValidProduct(this RestResponse<ProductModel> response)
    {
        response.ShouldBeOk();
        response.Data!.ShouldHaveValidFields();
    }

    public static void ShouldAllHaveValidProducts(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p =>
            p.Id > 0 &&
            !string.IsNullOrWhiteSpace(p.Title) &&
            p.Price >= 0 &&
            !string.IsNullOrWhiteSpace(p.Category) &&
            p.Rating != null &&
            p.Rating.Rate >= 0 && p.Rating.Rate <= 5);
    }
}
