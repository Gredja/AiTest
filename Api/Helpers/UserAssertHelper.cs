using Core.Helpers;
using Core.Models.FakeStore;
using FluentAssertions;
using RestSharp;

namespace Api.Helpers;

public static class UserAssertHelper
{
    public static void ShouldBeOkWithValidUser(this RestResponse<UserModel> response)
    {
        response.ShouldBeOk();
        response.Data!.ShouldHaveValidFields();
    }

    public static void ShouldAllHaveValidUserIds(this List<UserModel> users)
    {
        users.Should().OnlyContain(u => u.Id > 0, "all users must have positive Id");
    }

    public static void ShouldAllHaveValidEmail(this List<UserModel> users)
    {
        users.Should().OnlyContain(u => !string.IsNullOrWhiteSpace(u.Email), "all users must have non-empty Email");
    }

    public static void ShouldAllHaveValidUsername(this List<UserModel> users)
    {
        users.Should().OnlyContain(u => !string.IsNullOrWhiteSpace(u.Username), "all users must have non-empty Username");
    }

    public static void ShouldAllHaveValidName(this List<UserModel> users)
    {
        users.Should().OnlyContain(u =>
            u.Name != null &&
            !string.IsNullOrWhiteSpace(u.Name.Firstname) &&
            !string.IsNullOrWhiteSpace(u.Name.Lastname),
            "all users must have Name with non-empty Firstname and Lastname");
    }

    public static void ShouldAllHaveValidPhone(this List<UserModel> users)
    {
        users.Should().OnlyContain(u => !string.IsNullOrWhiteSpace(u.Phone), "all users must have non-empty Phone");
    }

    public static void ShouldAllHaveValidAddress(this List<UserModel> users)
    {
        users.Should().OnlyContain(u =>
            u.Address != null &&
            !string.IsNullOrWhiteSpace(u.Address.City) &&
            !string.IsNullOrWhiteSpace(u.Address.Street) &&
            !string.IsNullOrWhiteSpace(u.Address.Zipcode),
            "all users must have Address with non-empty City, Street, and Zipcode");
    }

    public static void ShouldAllHaveValidUsers(this List<UserModel> users)
    {
        users.ShouldAllHaveValidUserIds();
        users.ShouldAllHaveValidEmail();
        users.ShouldAllHaveValidUsername();
        users.ShouldAllHaveValidName();
        users.ShouldAllHaveValidPhone();
        users.ShouldAllHaveValidAddress();
    }
}
