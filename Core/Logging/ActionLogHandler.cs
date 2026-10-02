using System.Diagnostics;

namespace Core.Logging;

internal sealed class ActionLogHandler : DelegatingHandler
{
    public ActionLogHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = await BufferRequestAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);
        var responseBody = await BufferResponseAsync(response, cancellationToken);
        stopwatch.Stop();

        ActionLogger.Logger.Information(
            "{Method} {Url} -> {StatusCode} in {DurationMs} ms | {RequestBody} | {ResponseBody}",
            request.Method.Method,
            request.RequestUri?.ToString() ?? string.Empty,
            (int)response.StatusCode,
            (int)stopwatch.Elapsed.TotalMilliseconds,
            Escape(requestBody),
            Escape(responseBody));

        return response;
    }

    private static async Task<string> BufferRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is null)
        {
            return string.Empty;
        }

        var buffered = await BufferAsync(request.Content, cancellationToken);
        request.Content = buffered;
        return await buffered.ReadAsStringAsync(cancellationToken);
    }

    private static async Task<string> BufferResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return string.Empty;
        }

        var buffered = await BufferAsync(response.Content, cancellationToken);
        response.Content = buffered;
        return await buffered.ReadAsStringAsync(cancellationToken);
    }

    private static async Task<HttpContent> BufferAsync(HttpContent content, CancellationToken cancellationToken)
    {
        var bytes = await content.ReadAsByteArrayAsync(cancellationToken);
        var buffered = new ByteArrayContent(bytes);

        foreach (var header in content.Headers)
        {
            buffered.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return buffered;
    }

    private static string Escape(string value) =>
        value.Replace("\r\n", "\\n").Replace("\r", "\\n").Replace("\n", "\\n");
}
