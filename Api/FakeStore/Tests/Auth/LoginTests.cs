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
public class LoginTests : FakeStoreRequestHelper
{
    private const string WhitespaceValue = "   ";
    private const string InvalidUsername = "invalid_user";
    private const string WrongPassword = "wrong_password";
    private static readonly AuthModelRequest _validCredentials = new()
    {
        Username = TestConfig.LoginUsername,
        Password = TestConfig.LoginPassword
    };

    [Test]
    [Category("HealthCheck")]
    [Description("13.1 Login with valid credentials — status code 201")]
    public async Task Login_ValidCredentials_ReturnsCreated()
    {
        var response = await Post<AuthModelRequest, AuthModelResponse>(FakeStoreEndpoints.Login, _validCredentials);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Test]
    [Category("Smoke")]
    [Description("13.2 Login with valid credentials — returns token")]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var response = await Post<AuthModelRequest, AuthModelResponse>(FakeStoreEndpoints.Login, _validCredentials);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Smoke")]
    [Description("13.3 Content-Type is application/json")]
    public async Task Login_ValidCredentials_ContentTypeIsJson()
    {
        var response = await Post<AuthModelRequest, AuthModelResponse>(FakeStoreEndpoints.Login, _validCredentials);

        response.ContentType.Should().Contain(AssertHelper.JsonContentType);
    }

    [Test]
    [Category("Negative")]
    [Description("13.4 Login with invalid username — status code 401")]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        var credentials = new AuthModelRequest { Username = InvalidUsername, Password = TestConfig.LoginPassword };
        var response = await Post<AuthModelRequest, AuthModelResponse>(FakeStoreEndpoints.Login, credentials);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("13.5 Login with invalid password — status code 401")]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var credentials = new AuthModelRequest { Username = TestConfig.LoginUsername, Password = WrongPassword };
        var response = await Post<AuthModelRequest, AuthModelResponse>(FakeStoreEndpoints.Login, credentials);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Description("13.6 Login with empty credentials — status code 400")]
    public async Task Login_EmptyCredentials_ReturnsBadRequest()
    {
        var credentials = new AuthModelRequest { Username = "", Password = "" };
        var response = await Post<AuthModelRequest, AuthModelResponse>(FakeStoreEndpoints.Login, credentials);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("Negative")]
    [Description("13.7 Login without username — status code 400")]
    public async Task Login_MissingUsername_ReturnsBadRequest()
    {
        var credentials = new Dictionary<string, string> { ["password"] = TestConfig.LoginPassword };
        var response = await Post<Dictionary<string, string>, AuthModelResponse>(FakeStoreEndpoints.Login, credentials);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("Negative")]
    [Description("13.8 Login without password — status code 400")]
    public async Task Login_MissingPassword_ReturnsBadRequest()
    {
        var credentials = new Dictionary<string, string> { ["username"] = TestConfig.LoginUsername };
        var response = await Post<Dictionary<string, string>, AuthModelResponse>(FakeStoreEndpoints.Login, credentials);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("Negative")]
    [Description("13.9 Login with empty body — status code 400")]
    public async Task Login_EmptyBody_ReturnsBadRequest()
    {
        var response = await Post<Dictionary<string, string>, AuthModelResponse>(
            FakeStoreEndpoints.Login, new Dictionary<string, string>());

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("Negative")]
    [Description("13.10 Login with whitespace credentials — status code 401")]
    public async Task Login_WhitespaceCredentials_ReturnsUnauthorized()
    {
        var credentials = new AuthModelRequest { Username = WhitespaceValue, Password = WhitespaceValue };
        var response = await Post<AuthModelRequest, AuthModelResponse>(FakeStoreEndpoints.Login, credentials);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }
}
