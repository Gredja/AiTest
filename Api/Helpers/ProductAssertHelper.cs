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

    public static void ShouldAllHaveValidId(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p => p.Id > 0, "all products must have positive Id");
    }

    public static void ShouldAllHaveValidTitle(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Title), "all products must have non-empty Title");
    }

    public static void ShouldAllHaveValidPrice(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p => p.Price >= 0, "all products must have Price >= 0");
    }

    public static void ShouldAllHaveValidDescription(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Description), "all products must have non-empty Description");
    }

    public static void ShouldAllHaveValidCategory(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Category), "all products must have non-empty Category");
    }

    public static void ShouldAllHaveValidImage(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Image), "all products must have non-empty Image");
    }

    public static void ShouldAllHaveValidRating(this List<ProductModel> products)
    {
        products.Should().OnlyContain(p =>
            p.Rating != null &&
            p.Rating.Rate >= 0 && p.Rating.Rate <= 5,
            "all products must have Rating with Rate in [0, 5]");
    }

    public static void ShouldAllHaveValidProducts(this List<ProductModel> products)
    {
        products.ShouldAllHaveValidId();
        products.ShouldAllHaveValidTitle();
        products.ShouldAllHaveValidPrice();
        products.ShouldAllHaveValidDescription();
        products.ShouldAllHaveValidCategory();
        products.ShouldAllHaveValidImage();
        products.ShouldAllHaveValidRating();
    }
}
