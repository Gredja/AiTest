using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Gredja.Tools;

// Line-delimited JSON-RPC MCP server over stdio (post_message, list_channels,
// get_channel_history). Mirrors the retired mcp-slack/server.mjs one-to-one for the
// original two tools: same handshake, error codes, tool schemas, stderr-only
// diagnostics — stdout carries the protocol exclusively.
internal static class ServeSlack
{
    private const string ServerName = "slack-local";
    private const string ServerVersion = "1.3.0";
    private const string SlackApi = "https://slack.com/api";
    private const string DefaultProtocolVersion = "2024-11-05";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    internal static async Task<int> RunAsync()
    {
        var defaultChannel = ReadDefaultChannel();

        Console.Error.WriteLine($"{ServerName} {ServerVersion} listening on stdio");

        string? line;
        while ((line = await Console.In.ReadLineAsync()) is not null)
        {
            // JS trim() strips a leading U+FEFF (BOM some shells prepend to the pipe); .NET Trim() does not
            var trimmed = line.Trim().TrimStart('﻿');
            if (trimmed.Length == 0)
            {
                continue;
            }

            JsonDocument message;
            try
            {
                message = JsonDocument.Parse(trimmed);
            }
            catch (JsonException)
            {
                await SendAsync(new JsonObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = null,
                    ["error"] = new JsonObject { ["code"] = -32700, ["message"] = "Parse error" }
                });
                continue;
            }

            using (message)
            {
                try
                {
                    await HandleAsync(message.RootElement, defaultChannel);
                }
                catch (Exception exception)
                {
                    JsonElement? id = message.RootElement.TryGetProperty("id", out var idElement)
                                      && idElement.ValueKind is not JsonValueKind.Undefined
                        ? idElement.Clone()
                        : null;
                    if (id is not null)
                    {
                        await SendAsync(new JsonObject
                        {
                            ["jsonrpc"] = "2.0",
                            ["id"] = ToNode(id.Value),
                            ["error"] = new JsonObject { ["code"] = -32603, ["message"] = exception.Message }
                        });
                    }
                }
            }
        }

        return 0;
    }

    private static async Task HandleAsync(JsonElement message, string? defaultChannel)
    {
        if (!message.TryGetProperty("id", out var idElement) || idElement.ValueKind is JsonValueKind.Undefined)
        {
            return;
        }

        var id = idElement.Clone();
        var method = message.TryGetProperty("method", out var methodElement)
            ? methodElement.GetString() ?? string.Empty
            : string.Empty;

        if (method == "initialize")
        {
            var protocolVersion = DefaultProtocolVersion;
            if (message.TryGetProperty("params", out var initParams)
                && initParams.ValueKind is JsonValueKind.Object
                && initParams.TryGetProperty("protocolVersion", out var requested)
                && requested.ValueKind is JsonValueKind.String)
            {
                protocolVersion = requested.GetString() ?? DefaultProtocolVersion;
            }

            await ReplyAsync(id, new JsonObject
            {
                ["protocolVersion"] = protocolVersion,
                ["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
                ["serverInfo"] = new JsonObject { ["name"] = ServerName, ["version"] = ServerVersion }
            });
            return;
        }

        if (method == "ping")
        {
            await ReplyAsync(id, new JsonObject());
            return;
        }

        if (method == "tools/list")
        {
            await ReplyAsync(id, new JsonObject { ["tools"] = BuildToolSchemas() });
            return;
        }

        if (method == "tools/call")
        {
            var toolName = message.TryGetProperty("params", out var callParams)
                           && callParams.ValueKind is JsonValueKind.Object
                ? callParams.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty
                : string.Empty;
            var argumentsElement = callParams.ValueKind is JsonValueKind.Object
                                   && callParams.TryGetProperty("arguments", out var argsElement)
                ? argsElement
                : default;

            try
            {
                var result = await CallToolAsync(toolName, argumentsElement, defaultChannel);
                await ReplyAsync(id, result);
            }
            catch (Exception exception)
            {
                await ReplyAsync(id, ToolResult(exception.Message, isError: true));
            }

            return;
        }

        await ReplyAsyncError(id, -32601, $"Method not found: {method}");
    }

    private static async Task<JsonObject> CallToolAsync(string toolName, JsonElement arguments, string? defaultChannel)
    {
        if (toolName == "post_message")
        {
            var channel = ReadString(arguments, "channel") ?? defaultChannel;
            if (channel is null)
            {
                throw new InvalidOperationException(
                    "No channel given and no defaultChannel in Scripts/mcp-slack/config.json");
            }

            foreach (var field in new[] { "blocks", "attachments" })
            {
                // node guard verbatim: property present (even null) must be an array
                if (TryGetProperty(arguments, field, out var fieldElement)
                    && fieldElement.ValueKind is not JsonValueKind.Array)
                {
                    throw new InvalidOperationException($"Invalid {field}: expected an array of Slack objects");
                }
            }

            var payload = new JsonObject
            {
                ["channel"] = channel,
                ["text"] = ReadString(arguments, "text") ?? string.Empty
            };
            if (TryGetProperty(arguments, "blocks", out var blocks))
            {
                payload["blocks"] = ToNode(blocks);
            }

            if (TryGetProperty(arguments, "attachments", out var attachments))
            {
                payload["attachments"] = ToNode(attachments);
            }

            var response = await SlackPostAsync("/chat.postMessage", payload);
            var postedChannel = response.GetProperty("channel").GetString();
            var timestamp = response.GetProperty("ts").GetString();

            return ToolResult($"Message posted to {postedChannel} at {timestamp}");
        }

        if (toolName == "list_channels")
        {
            var limit = 50;
            if (TryGetProperty(arguments, "limit", out var limitElement)
                && limitElement.ValueKind is JsonValueKind.Number)
            {
                limit = limitElement.GetInt32();
            }

            var response = await SlackGetAsync(
                $"/conversations.list?types=public_channel&exclude_archived=true&limit={limit}");
            var lines = new List<string>();
            if (response.TryGetProperty("channels", out var channels)
                && channels.ValueKind is JsonValueKind.Array)
            {
                foreach (var channel in channels.EnumerateArray())
                {
                    lines.Add($"{channel.GetProperty("id").GetString()}\t{channel.GetProperty("name").GetString()}");
                }
            }

            return ToolResult(lines.Count > 0 ? string.Join('\n', lines) : "No channels visible to the bot");
        }

        if (toolName == "get_channel_history")
        {
            var channel = ReadString(arguments, "channel");
            if (string.IsNullOrEmpty(channel))
            {
                throw new InvalidOperationException(
                    "channel is required (channel ID C... — use list_channels to find it)");
            }

            var limit = 10;
            if (TryGetProperty(arguments, "limit", out var limitElement)
                && limitElement.ValueKind is JsonValueKind.Number)
            {
                limit = limitElement.GetInt32();
            }

            var response = await SlackGetAsync(
                $"/conversations.history?channel={Uri.EscapeDataString(channel)}&limit={limit}");
            var lines = new List<string>();
            if (response.TryGetProperty("messages", out var messages)
                && messages.ValueKind is JsonValueKind.Array)
            {
                foreach (var message in messages.EnumerateArray())
                {
                    var ts = message.TryGetProperty("ts", out var tsElement) ? tsElement.GetString() : string.Empty;
                    var user = message.TryGetProperty("user", out var userElement) ? userElement.GetString() : "unknown";
                    var text = message.TryGetProperty("text", out var textElement) ? textElement.GetString() : string.Empty;
                    lines.Add($"{ts}\t{user}\t{text}");
                }
            }

            return ToolResult(lines.Count > 0 ? string.Join('\n', lines) : "No messages in channel");
        }

        throw new InvalidOperationException($"Unknown tool: {toolName}");
    }

    private static JsonObject ToolResult(string text, bool isError = false)
    {
        var result = new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = text })
        };
        if (isError)
        {
            result["isError"] = true;
        }

        return result;
    }

    private static async Task<JsonElement> SlackPostAsync(string path, JsonObject payload)
    {
        using var client = CreateSlackClient();
        var content = new StringContent(payload.ToJsonString(_jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync($"{SlackApi}{path}", content);

        return await ReadSlackResponseAsync(response, path);
    }

    private static async Task<JsonElement> SlackGetAsync(string pathAndQuery)
    {
        using var client = CreateSlackClient();
        using var response = await client.GetAsync($"{SlackApi}{pathAndQuery}");

        return await ReadSlackResponseAsync(response, pathAndQuery);
    }

    private static HttpClient CreateSlackClient()
    {
        var token = Environment.GetEnvironmentVariable("SLACK_BOT_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException(
                "SLACK_BOT_TOKEN environment variable is not set — run Scripts/sync-secrets.ps1 in the launching shell (Infisical pull, in-memory)");
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static async Task<JsonElement> ReadSlackResponseAsync(HttpResponseMessage response, string path)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var payload = JsonDocument.Parse(body);
        var root = payload.RootElement.Clone();

        var isOk = response.IsSuccessStatusCode
                   && (!root.TryGetProperty("ok", out var ok) || ok.ValueKind is not JsonValueKind.False);
        if (isOk)
        {
            return root;
        }

        var slackError = root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind is JsonValueKind.String
            ? errorElement.GetString() ?? $"HTTP {(int)response.StatusCode}"
            : $"HTTP {(int)response.StatusCode}";
        var details = string.Empty;
        if (root.TryGetProperty("needed", out var needed))
        {
            var provided = root.TryGetProperty("provided", out var providedElement)
                ? providedElement.GetString() ?? "none"
                : "none";
            details = $" (needed: {needed.GetString()}; provided: {provided})";
        }

        throw new InvalidOperationException($"Slack API {path} failed: {slackError}{details}");
    }

    private static string ReadDefaultChannel()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var configPath = Path.Combine(directory.FullName, "Scripts", "mcp-slack", "config.json");
            if (File.Exists(configPath))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(configPath));
                return document.RootElement.TryGetProperty("defaultChannel", out var channel)
                    ? channel.GetString() ?? string.Empty
                    : string.Empty;
            }

            directory = directory.Parent;
        }

        return string.Empty;
    }

    private static JsonArray BuildToolSchemas()
    {
        var postMessage = new JsonObject
        {
            ["name"] = "post_message",
            ["description"] =
                "Post a message to a Slack channel the bot is a member of. Omit channel to use the defaultChannel from config.json.",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["channel"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Channel ID (C...) or name (#general); defaults to defaultChannel in config.json"
                    },
                    ["text"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Message text; mrkdwn is supported (fallback/notification preview)"
                    },
                    ["blocks"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = new JsonObject { ["type"] = "object" },
                        ["description"] = "Optional Block Kit blocks for a rich message (sections, header, actions)"
                    },
                    ["attachments"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = new JsonObject { ["type"] = "object" },
                        ["description"] = "Optional Slack attachments (e.g. color indicator with embedded blocks)"
                    }
                },
                ["required"] = new JsonArray("text")
            }
        };

        var listChannels = new JsonObject
        {
            ["name"] = "list_channels",
            ["description"] = "List Slack channels visible to the bot (id and name).",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["limit"] = new JsonObject
                    {
                        ["type"] = "integer",
                        ["description"] = "Maximum number of channels to return (1-200)",
                        ["minimum"] = 1,
                        ["maximum"] = 200
                    }
                },
                ["required"] = new JsonArray()
            }
        };

        var getChannelHistory = new JsonObject
        {
            ["name"] = "get_channel_history",
            ["description"] = "Read recent messages from a Slack channel (requires channels:history). Returns ts, user and text per line, newest first.",
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["channel"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Channel ID (C...) — use list_channels to look it up"
                    },
                    ["limit"] = new JsonObject
                    {
                        ["type"] = "integer",
                        ["description"] = "Maximum number of messages to return (1-100)",
                        ["minimum"] = 1,
                        ["maximum"] = 100
                    }
                },
                ["required"] = new JsonArray("channel")
            }
        };

        return [postMessage, listChannels, getChannelHistory];
    }

    private static async Task ReplyAsync(JsonElement id, JsonObject result)
    {
        await SendAsync(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = ToNode(id),
            ["result"] = result
        });
    }

    private static async Task ReplyAsyncError(JsonElement id, int code, string message)
    {
        await SendAsync(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = ToNode(id),
            ["error"] = new JsonObject { ["code"] = code, ["message"] = message }
        });
    }

    private static JsonNode ToNode(JsonElement element) =>
        JsonSerializer.SerializeToNode(element)!;

    private static async Task SendAsync(JsonObject payload)
    {
        await Console.Out.WriteLineAsync(payload.ToJsonString(_jsonOptions));
    }

    private static string? ReadString(JsonElement arguments, string property) =>
        arguments.ValueKind is JsonValueKind.Object
        && arguments.TryGetProperty(property, out var element)
        && element.ValueKind is JsonValueKind.String
            ? element.GetString()
            : null;

    private static bool TryGetProperty(JsonElement arguments, string property, out JsonElement element)
    {
        if (arguments.ValueKind is JsonValueKind.Object && arguments.TryGetProperty(property, out element))
        {
            return true;
        }

        element = default;
        return false;
    }
}
