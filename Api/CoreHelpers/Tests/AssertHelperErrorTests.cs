using System.Net;
using Core.Helpers;
using FluentAssertions;
using RestSharp;

namespace Api.CoreHelpers;

[TestFixture]
[AllureNUnit]
[Category("Core")]
public class AssertHelperErrorTests
{
    private const string NotFoundBody = """{"message":"Not Found"}""";

    [Test]
    [Category("Regression")]
    public void ShouldHaveError_ValidErrorBody_ReturnsParsedModel()
    {
        var response = new RestResponse
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = NotFoundBody
        };

        var error = response.ShouldHaveError(HttpStatusCode.NotFound, "Not Found");

        error.Message.Should().Be("Not Found");
    }

    [TestCase(HttpStatusCode.OK, HttpStatusCode.NotFound, NotFoundBody, "Not Found",
        TestName = "ShouldHaveError_WrongStatus_Throws")]
    [TestCase(HttpStatusCode.NotFound, HttpStatusCode.NotFound, "   ", "Not Found",
        TestName = "ShouldHaveError_EmptyBody_Throws")]
    [TestCase(HttpStatusCode.NotFound, HttpStatusCode.NotFound, NotFoundBody, "Gone",
        TestName = "ShouldHaveError_MessageMismatch_Throws")]
    [Category("Negative")]
    public void ShouldHaveError_InvalidError_Throws(
        HttpStatusCode actualStatus,
        HttpStatusCode expectedStatus,
        string content,
        string expectedMessage)
    {
        var response = new RestResponse
        {
            StatusCode = actualStatus,
            Content = content
        };

        var act = () => response.ShouldHaveError(expectedStatus, expectedMessage);

        act.Should().Throw<AssertionException>();
    }
}
