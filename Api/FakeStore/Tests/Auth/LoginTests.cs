using RestSharp;
using Core.Models.FakeStore;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.FakeStore.Auth;

[TestFixture]
[AllureNUnit]
[Category("FakeStore")]
public class LoginTests : RequestHelper
{
    private static readonly AuthModelRequest ValidCredentials = new()
    {
        Username = TestConfig.LoginUsername,
        Password = TestConfig.LoginPassword
    };

    [Test]
    [Category("HealthCheck")]
    [Description("13.1 Login with valid credentials — status code 201")]
    public async Task Login_ValidCredentials_ReturnsCreated()
    {
        var request = new RestRequest(FakeStoreEndpoints.Login, Method.Post);
        request.AddJsonBody(ValidCredentials);

        var response = await Client.ExecuteAsync<AuthModelResponse>(request);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Test]
    [Category("Smoke")]
    [Description("13.2 Login with valid credentials — returns token")]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var request = new RestRequest(FakeStoreEndpoints.Login, Method.Post);
        request.AddJsonBody(ValidCredentials);

        var response = await Client.ExecuteAsync<AuthModelResponse>(request);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Smoke")]
    [Description("13.3 Content-Type is application/json")]
    public async Task Login_ValidCredentials_ContentTypeIsJson()
    {
        var request = new RestRequest(FakeStoreEndpoints.Login, Method.Post);
        request.AddJsonBody(ValidCredentials);

        var response = await Client.ExecuteAsync<AuthModelResponse>(request);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("13.4 Login with invalid username — status code 401")]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        var request = new RestRequest(FakeStoreEndpoints.Login, Method.Post);
        request.AddJsonBody(new AuthModelRequest { Username = "invalid_user", Password = TestConfig.LoginPassword });

        var response = await Client.ExecuteAsync<AuthModelResponse>(request);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("13.5 Login with invalid password — status code 401")]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var request = new RestRequest(FakeStoreEndpoints.Login, Method.Post);
        request.AddJsonBody(new AuthModelRequest { Username = TestConfig.LoginUsername, Password = "wrong_password" });

        var response = await Client.ExecuteAsync<AuthModelResponse>(request);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("13.6 Login with empty credentials — status code 400")]
    public async Task Login_EmptyCredentials_ReturnsBadRequest()
    {
        var request = new RestRequest(FakeStoreEndpoints.Login, Method.Post);
        request.AddJsonBody(new AuthModelRequest { Username = "", Password = "" });

        var response = await Client.ExecuteAsync<AuthModelResponse>(request);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }
}
