using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Gredja.Tools;

// Long-running poller: reads new human messages from a Slack channel and injects
// them into a live MiMoCode session through a fixed-port `mimo serve` listener
// (POST /session/{id}/message). Polling happens on the Slack side (plain HTTP,
// zero model tokens); a delivered message becomes a regular session turn.
internal static class SlackInbox
{
    private const string SlackApi = "https://slack.com/api";
    private const string TokenVariable = "SLACK_BOT_TOKEN";
    private const string MessagePrefix = "[Slack]";
    private const string LoopbackHost = "127.0.0.1";
    private const int HistoryLimit = 10;
    private const int DefaultPollSeconds = 2;

    private sealed record ChannelMessage(string Timestamp, decimal SortKey, string User, string Text);

    private sealed record InboxConfig(string Channel, string BotUser, int Port, int PollSeconds);

    internal static async Task<int> RunAsync(string? sessionId)
    {
        if (sessionId is not null && string.IsNullOrEmpty(sessionId))
        {
            Console.Error.WriteLine("usage: slack-inbox [sessionID]  (omit sessionID to target the newest main session)");
            return 2;
        }

        var token = Environment.GetEnvironmentVariable(TokenVariable);
        if (string.IsNullOrEmpty(token))
        {
            Console.Error.WriteLine($"{TokenVariable} is not set — run Scripts/sync-secrets.ps1 in this shell first");
            return 1;
        }

        var config = ReadInboxConfig();
        var target = sessionId ?? $"auto newest main session via {LoopbackHost}:{config.Port}";

        Console.Error.WriteLine(
            $"slack-inbox: {config.Channel} → {target}, every {config.PollSeconds}s");

        await RunPollLoopAsync(config, sessionId, token);
        return 0;
    }

    private static async Task RunPollLoopAsync(InboxConfig config, string? sessionId, string token)
    {
        var interval = TimeSpan.FromSeconds(config.PollSeconds);
        var lastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000m;

        while (true)
        {
            try
            {
                lastTimestamp = await DeliverNewMessagesAsync(config, sessionId, token, lastTimestamp);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"slack-inbox: {exception.Message}");
            }

            await Task.Delay(interval);
        }
    }

    private static async Task<decimal> DeliverNewMessagesAsync(
        InboxConfig config, string? sessionId, string token, decimal lastTimestamp)
    {
        var messages = await FetchNewMessagesAsync(config.Channel, config.BotUser, lastTimestamp, token);
        if (messages.Count == 0)
        {
            return lastTimestamp;
        }

        var targetSessionId = sessionId ?? await ResolveSessionIdAsync(config.Port);
        foreach (var message in messages)
        {
            await InjectPromptAsync(targetSessionId, config.Port, $"{message.User}: {message.Text}");
            lastTimestamp = message.SortKey;
            Console.Error.WriteLine($"slack-inbox: injected {message.Timestamp} from {message.User} → {targetSessionId}");
        }

        return lastTimestamp;
    }

    private static async Task<string> ResolveSessionIdAsync(int port)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync($"http://{LoopbackHost}:{port}/session");
        response.EnsureSuccessStatusCode();
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        string? newestSessionId = null;
        var newestUpdate = -1L;
        foreach (var session in payload.RootElement.EnumerateArray())
        {
            // subagent/checkpoint child sessions carry parentID — only main sessions receive prompts
            if (session.TryGetProperty("parentID", out _))
            {
                continue;
            }

            var id = session.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            var updated = session.TryGetProperty("time", out var timeElement)
                          && timeElement.TryGetProperty("updated", out var updatedElement)
                ? updatedElement.GetInt64()
                : -1L;
            if (!string.IsNullOrEmpty(id) && updated > newestUpdate)
            {
                newestUpdate = updated;
                newestSessionId = id;
            }
        }

        return newestSessionId
            ?? throw new InvalidOperationException(
                $"no main session in GET /session on {LoopbackHost}:{port} — open a MiMoCode session first");
    }

    private static async Task<List<ChannelMessage>> FetchNewMessagesAsync(
        string channel, string botUser, decimal lastTimestamp, string token)
    {
        using var client = CreateSlackClient(token);
        var path = $"/conversations.history?channel={Uri.EscapeDataString(channel)}&limit={HistoryLimit}";
        using var response = await client.GetAsync($"{SlackApi}{path}");
        var body = await response.Content.ReadAsStringAsync();
        using var payload = JsonDocument.Parse(body);

        if (!response.IsSuccessStatusCode || !payload.RootElement.TryGetProperty("ok", out var ok)
            || ok.ValueKind is JsonValueKind.False)
        {
            throw new InvalidOperationException($"Slack API {path} failed: {ReadSlackError(payload.RootElement, response.StatusCode)}");
        }

        var messages = ParseNewMessages(payload.RootElement, botUser, lastTimestamp);
        messages.Reverse();
        return messages;
    }

    private static List<ChannelMessage> ParseNewMessages(JsonElement root, string botUser, decimal lastTimestamp)
    {
        var messages = new List<ChannelMessage>();
        if (!root.TryGetProperty("messages", out var messagesElement)
            || messagesElement.ValueKind is not JsonValueKind.Array)
        {
            return messages;
        }

        foreach (var messageElement in messagesElement.EnumerateArray())
        {
            var message = ParseMessage(messageElement, botUser, lastTimestamp);
            if (message is not null)
            {
                messages.Add(message);
            }
        }

        return messages;
    }

    private static ChannelMessage? ParseMessage(JsonElement messageElement, string botUser, decimal lastTimestamp)
    {
        if (messageElement.TryGetProperty("subtype", out _))
        {
            return null;
        }

        var user = messageElement.TryGetProperty("user", out var userElement)
            ? userElement.GetString()
            : null;
        if (string.IsNullOrEmpty(user) || user == botUser)
        {
            return null;
        }

        var timestamp = messageElement.TryGetProperty("ts", out var tsElement)
            ? tsElement.GetString()
            : null;
        if (string.IsNullOrEmpty(timestamp)
            || !decimal.TryParse(timestamp, NumberStyles.Number, CultureInfo.InvariantCulture, out var sortKey)
            || sortKey <= lastTimestamp)
        {
            return null;
        }

        var text = messageElement.TryGetProperty("text", out var textElement)
            ? textElement.GetString()
            : string.Empty;
        return new ChannelMessage(timestamp, sortKey, user, text ?? string.Empty);
    }

    private static async Task InjectPromptAsync(string sessionId, int port, string text)
    {
        var payload = JsonSerializer.Serialize(new
        {
            parts = new[] { new { type = "text", text = $"{MessagePrefix} {text}" } }
        });

        using var client = new HttpClient();
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(
            $"http://{LoopbackHost}:{port}/session/{sessionId}/message", content);
        response.EnsureSuccessStatusCode();
    }

    private static HttpClient CreateSlackClient(string token)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string ReadSlackError(JsonElement payload, System.Net.HttpStatusCode statusCode)
    {
        var slackError = payload.TryGetProperty("error", out var errorElement)
            && errorElement.ValueKind is JsonValueKind.String
            ? errorElement.GetString()
            : null;
        return slackError ?? $"HTTP {(int)statusCode}";
    }

    private static InboxConfig ReadInboxConfig()
    {
        var config = ReadConfig();
        return new InboxConfig(
            GetRequiredString(config, "watchChannel"),
            GetRequiredString(config, "botUser"),
            GetRequiredInt(config, "servePort"),
            GetOptionalInt(config, "pollSeconds") ?? DefaultPollSeconds);
    }

    private static JsonElement ReadConfig()
    {
        var roots = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        foreach (var root in roots)
        {
            var directory = new DirectoryInfo(root);
            while (directory is not null)
            {
                var configPath = Path.Combine(directory.FullName, "Scripts", "mcp-slack", "config.json");
                if (File.Exists(configPath))
                {
                    using var document = JsonDocument.Parse(File.ReadAllText(configPath));
                    return document.RootElement.Clone();
                }

                directory = directory.Parent;
            }
        }

        throw new InvalidOperationException("Scripts/mcp-slack/config.json not found above the tool or working directory");
    }

    private static string GetRequiredString(JsonElement config, string key)
    {
        if (config.TryGetProperty(key, out var element) && element.ValueKind is JsonValueKind.String)
        {
            var value = element.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        throw new InvalidOperationException($"config key '{key}' is missing in Scripts/mcp-slack/config.json");
    }

    private static int GetRequiredInt(JsonElement config, string key) =>
        GetOptionalInt(config, key)
        ?? throw new InvalidOperationException($"config key '{key}' is missing in Scripts/mcp-slack/config.json");

    private static int? GetOptionalInt(JsonElement config, string key)
    {
        if (config.TryGetProperty(key, out var element) && element.ValueKind is JsonValueKind.Number)
        {
            return element.GetInt32();
        }

        return null;
    }
}
