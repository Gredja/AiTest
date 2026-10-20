using System.Net;
using Core.Config;

namespace Ui.Moodledemo.Tests.Support;

internal static class MoodledemoHttp
{
    private const int HttpTimeoutSeconds = 20;

    // No cookie jar: Moodle persists the language choice in a session cookie, which would
    // leak state between tests (order dependence — an unknown ?lang then falls back to the
    // session language instead of English). Every request must be a fresh visitor.
    private static readonly HttpClient _client = new(new HttpClientHandler { UseCookies = false })
    {
        Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds)
    };

    internal static async Task<(HttpStatusCode Status, string Body)> GetAsync(string pathAndQuery)
    {
        var response = await _client.GetAsync($"{MoodledemoEndpoints.BaseUrl}{pathAndQuery}");

        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    internal static string HomeWithLang(string? code) =>
        code is null
            ? MoodledemoEndpoints.HomePath
            : $"{MoodledemoEndpoints.HomePath}?{MoodledemoEndpoints.LangQueryKey}={code}";

    internal static string LoginWithLang(string code) =>
        $"{MoodledemoEndpoints.LoginPath}?{MoodledemoEndpoints.LangQueryKey}={code}";
}
