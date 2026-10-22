using System.Net;
using Core.Config;

namespace Ui.Moodledemo.Tests.Support;

internal static class MoodledemoHttp
{
    private const int HttpTimeoutSeconds = 20;
    private const int MaxAttempts = 3;
    private const string SiteTitleMarker = "| Mount Orange";
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

    // No cookie jar: Moodle persists the language choice in a session cookie, which would
    // leak state between tests (order dependence — an unknown ?lang then falls back to the
    // session language instead of English). Every request must be a fresh visitor.
    private static readonly HttpClient _client = new(new HttpClientHandler { UseCookies = false })
    {
        Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds)
    };

    // The external demo occasionally answers with a connection drop or an HTTP 200 foreign
    // page (observed ~5% under load); retry until a real site page arrives so the suite
    // reports persistence, not transient noise. A still-bad last attempt is returned
    // unchanged for the test assertion to fail on.
    internal static async Task<(HttpStatusCode Status, string Body)> GetAsync(string pathAndQuery)
    {
        var status = default(HttpStatusCode);
        var body = string.Empty;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                var response = await _client.GetAsync($"{MoodledemoEndpoints.BaseUrl}{pathAndQuery}");
                status = response.StatusCode;
                body = await response.Content.ReadAsStringAsync();

                if (status == HttpStatusCode.OK && body.Contains(SiteTitleMarker, StringComparison.Ordinal))
                {
                    return (status, body);
                }
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
            }
            catch (TaskCanceledException) when (attempt < MaxAttempts)
            {
            }

            if (attempt < MaxAttempts)
            {
                await Task.Delay(_retryDelay);
            }
        }

        return (status, body);
    }

    internal static string HomeWithLang(string? code) =>
        code is null
            ? MoodledemoEndpoints.HomePath
            : $"{MoodledemoEndpoints.HomePath}?{MoodledemoEndpoints.LangQueryKey}={code}";

    internal static string LoginWithLang(string code) =>
        $"{MoodledemoEndpoints.LoginPath}?{MoodledemoEndpoints.LangQueryKey}={code}";
}
