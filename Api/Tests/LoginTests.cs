using NUnit.Framework;
using RestSharp;
using Core.Models;
using Core.Config;
using Core.Helpers;
using System.Net;
using FluentAssertions;
using TestAdapter;

namespace Api.Tests;

[TestFixture]
[AllureNUnit]
public class LoginTests : RequestHelper
{
    private static readonly LoginRequest ValidCredentials = new()
    {
        Username = TestConfig.LoginUsername,
        Password = TestConfig.LoginPassword
    };

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("5.1 Login with valid credentials — status code 201")]
    public async Task Login_ValidCredentials_ReturnsCreated()
    {
        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            ValidCredentials);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Test]
    [Category("Smoke")]
    [Category("Fast")]
    [Description("5.2 Login response contains token")]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            ValidCredentials);

        response.Data!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    [Category("Negative")]
    [Category("Fast")]
    [Description("5.3 Login with invalid username — status code 401")]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "invalid_user_999",
            Password = "83r5^_"
        };

        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            request);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Category("Fast")]
    [Description("5.4 Login with invalid password — status code 401")]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "mor_2314",
            Password = "wrong_password"
        };

        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            request);

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Category("Negative")]
    [Category("Fast")]
    [Description("5.5 Login with empty body — status code 400")]
    public async Task Login_EmptyBody_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = ""
        };

        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            request);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("EdgeCase")]
    [Category("Slow")]
    [Description("5.6 Login with null username — status code 400")]
    public async Task Login_NullUsername_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = null!,
            Password = "83r5^_"
        };

        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            request);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("EdgeCase")]
    [Category("Slow")]
    [Description("5.7 Login with null password — status code 400")]
    public async Task Login_NullPassword_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = "mor_2314",
            Password = null!
        };

        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            request);

        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Test]
    [Category("Performance")]
    [Category("Slow")]
    [Description("5.8 Response time < 5 seconds")]
    public async Task Login_ResponseTimeIsAcceptable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Post<LoginRequest, LoginResponse>(
            Endpoints.Login,
            ValidCredentials);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(Endpoints.MaxResponseTimeMs);
    }
}
