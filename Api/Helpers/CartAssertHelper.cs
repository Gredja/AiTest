using Core.Helpers;
using Core.Models;
using FluentAssertions;
using RestSharp;

namespace Api.Helpers;

public static class CartAssertHelper
{
    public static void ShouldBeOkWithValidCart(this RestResponse<CartModel> response)
    {
        response.ShouldBeOk();
        response.Data!.ShouldHaveValidFields();
    }

    public static void ShouldAllHaveValidCartIds(this List<CartModel> carts)
    {
        carts.Should().OnlyContain(c => c.Id > 0, "all carts must have positive Id");
    }

    public static void ShouldAllHaveValidUserId(this List<CartModel> carts)
    {
        carts.Should().OnlyContain(c => c.UserId > 0, "all carts must have positive UserId");
    }

    public static void ShouldAllHaveValidDate(this List<CartModel> carts)
    {
        carts.Should().OnlyContain(c => c.Date != default, "all carts must have valid Date");
    }

    public static void ShouldAllHaveValidProducts(this List<CartModel> carts)
    {
        carts.Should().OnlyContain(c =>
            c.Products != null && c.Products.Count > 0,
            "all carts must have non-empty Products list");
    }

    public static void ShouldAllHaveValidCarts(this List<CartModel> carts)
    {
        carts.ShouldAllHaveValidCartIds();
        carts.ShouldAllHaveValidUserId();
        carts.ShouldAllHaveValidDate();
        carts.ShouldAllHaveValidProducts();
    }
}
