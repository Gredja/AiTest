using Core.Models.JsonPlaceholder;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using AllureAdapter;
using static Core.Helpers.ParamHelper;

namespace Api.JsonPlaceholder.Users;

[TestFixture]
[AllureNUnit]
[Category("JsonPlaceholder")]
public class GetUserPostsTests : JsonPlaceholderRequestHelper
{
    private const int TestUserId = 1;

    [Test]
    [Category("HealthCheck")]
    [Description("3.1 Status code is 200")]
    public async Task GetUserPosts_ReturnsOk()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.UsersPosts,
            IdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Test]
    [Category("ContractCheck")]
    [Description("3.2 Response matches expected contract")]
    public async Task GetUserPosts_ResponseMatchesContract()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.UsersPosts,
            IdParam(TestUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().NotBeEmpty();
        response.Data!.First().ShouldHaveValidContract();
    }

    [Test]
    [Category("Smoke")]
    [Description("3.3 Response body is not empty")]
    public async Task GetUserPosts_ReturnsNonEmptyList()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.UsersPosts,
            IdParam(TestUserId));

        response.Data.Should().NotBeEmpty();
    }

    [Test]
    [Category("Regression")]
    [Description("3.4 Each item has valid fields")]
    public async Task GetUserPosts_EachItemHasValidFields()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.UsersPosts,
            IdParam(TestUserId));

        response.Data!.ForEach(post => post.ShouldHaveValidFields());
    }

    [Test]
    [Category("Smoke")]
    [Description("3.5 All posts belong to same user")]
    public async Task GetUserPosts_AllBelongToSameUser()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.UsersPosts,
            IdParam(TestUserId));

        response.Data!.Should().OnlyContain(post => post.UserId == TestUserId);
    }

    [Test]
    [Category("Negative")]
    [Description("3.6 Returns empty list for non-existent userId")]
    public async Task GetUserPosts_NonExistentUserId_ReturnsEmpty()
    {
        var allUsers = await Get<List<UserModelResponse>>(JsonPlaceholderEndpoints.Users);
        allUsers.ShouldHaveStatusCode(HttpStatusCode.OK);
        allUsers.Data.Should().NotBeEmpty();
        var maxUserId = allUsers.Data!.Max(user => user.Id);
        var nonExistentUserId = maxUserId + 1;

        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.UsersPosts,
            IdParam(nonExistentUserId));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }

    [Test]
    [Category("Negative")]
    [Description("3.7 Returns empty list for userId=0")]
    public async Task GetUserPosts_ZeroUserId_ReturnsEmpty()
    {
        var response = await Get<List<PostModelResponse>>(JsonPlaceholderEndpoints.UsersPosts,
            IdParam(0));

        response.ShouldHaveStatusCode(HttpStatusCode.OK);
        response.Data.Should().BeEmpty();
    }
}
