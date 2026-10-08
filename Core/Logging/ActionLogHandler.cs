using System.Diagnostics;
using System.Text;

namespace Core.Logging;

internal sealed class ActionLogHandler : DelegatingHandler
{
    private const int MaxLoggedBodyBytes = 4096;

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

        return await ReadForLogAsync(buffered, cancellationToken);
    }

    private static async Task<string> BufferResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return string.Empty;
        }

        var buffered = await BufferAsync(response.Content, cancellationToken);
        response.Content = buffered;

        return await ReadForLogAsync(buffered, cancellationToken);
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

    // The full body must stay buffered for the caller, but decoding it whole would double
    // memory and add latency to every request — decode only a bounded prefix for the log line
    private static async Task<string> ReadForLogAsync(HttpContent content, CancellationToken cancellationToken)
    {
        var bytes = await content.ReadAsByteArrayAsync(cancellationToken);
        if (bytes.Length <= MaxLoggedBodyBytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        return $"{Encoding.UTF8.GetString(bytes, 0, MaxLoggedBodyBytes)}...[truncated, total {bytes.Length} bytes]";
    }

    private static string Escape(string value) =>
        value.Replace("\r\n", "\\n").Replace("\r", "\\n").Replace("\n", "\\n");
}
