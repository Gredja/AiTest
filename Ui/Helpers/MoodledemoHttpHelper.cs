using System.Net;
using Core.Config;

namespace Ui.Helpers;

internal static class MoodledemoHttpHelper
{
    private const int HttpTimeoutSeconds = 20;
    private const int MaxAttempts = 3;
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

    // No cookie jar: Moodle persists the language choice in a session cookie, which would
    // leak state between tests (order dependence — an unknown ?lang then falls back to the
    // session language instead of English). Every request must be a fresh visitor.
    private static readonly HttpClient _client = new(new HttpClientHandler { UseCookies = false })
    {
        Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds)
    };

    // The external demo occasionally answers with a connection drop, an HTTP 200 foreign
    // page (observed ~5% under load), or its default-locale page for a ?lang request;
    // retry until the requested locale arrives so the suite reports persistence, not
    // transient noise. A still-bad last attempt is returned unchanged for the assertion.
    internal static async Task<(HttpStatusCode Status, string Body)> GetAsync(
        string pathAndQuery,
        string expectedLang)
    {
        var status = default(HttpStatusCode);
        var body = string.Empty;
        var expectedLangAttribute = $" lang=\"{expectedLang}\"";

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                var response = await _client.GetAsync($"{MoodledemoEndpoints.BaseUrl}{pathAndQuery}");
                status = response.StatusCode;
                body = await response.Content.ReadAsStringAsync();

                if (status == HttpStatusCode.OK
                    && body.Contains(MoodledemoEndpoints.SiteTitleMarker, StringComparison.Ordinal)
                    && body.Contains(expectedLangAttribute, StringComparison.Ordinal))
                {
                    return (status, body);
                }
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
                // transient connection failure — swallowed to reach the retry delay below;
                // without the filter the last attempt would rethrow instead
            }
            catch (TaskCanceledException) when (attempt < MaxAttempts)
            {
                // transient timeout — same retry path as above
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
