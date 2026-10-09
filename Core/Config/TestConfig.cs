using System.Text.Json;

namespace Core.Config;

public static class TestConfig
{
    private const string BaseUrlKey = "BaseUrl";

    private static readonly JsonDocument _root = JsonDocument.Parse(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "testsettings.json")));

    private static JsonElement JsonPlaceholder => _root.RootElement.GetProperty("JsonPlaceholder");
    private static JsonElement GitHub => _root.RootElement.GetProperty("GitHub");
    private static JsonElement Ui => _root.RootElement.GetProperty("Ui");

    public static int MaxResponseTimeMs => _root.RootElement.GetProperty("MaxResponseTimeMs").GetInt32();
    public static bool IsSslValidationSkipped => _root.RootElement.GetProperty("SkipSslValidation").GetBoolean();

    public static string JsonPlaceholderBaseUrl => GetRequiredString(JsonPlaceholder, BaseUrlKey);

    public static string GitHubBaseUrl => GetRequiredString(GitHub, BaseUrlKey);
    public static string GitHubToken
    {
        get
        {
            var token = GitHub.GetProperty("Token").GetString();
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            return ReadEnvironmentValue("GITHUB_PAT") ?? string.Empty;
        }
    }

    public static string GitHubTestRepo => GetRequiredString(GitHub, "TestRepo");
    public static string GitHubTestUsername => GetRequiredString(GitHub, "TestUsername");
    public static string GitHubCollaboratorUser => GetRequiredString(GitHub, "CollaboratorUser");

    public static string UiBaseUrl => GetRequiredString(Ui, BaseUrlKey);
    public static bool UiHeadless => GetRequiredBool(Ui, "Headless");
    public static string UiStatePath => GetRequiredString(Ui, "StatePath");
    public static string UiUsername => GetRequiredString(Ui, "Username");
    public static string UiLogin => GetRequiredEnvironmentValue("GITHUB_UI_EMAIL");
    public static string UiPassword => GetRequiredEnvironmentValue("GITHUB_UI_PASSWORD");

    private static string GetRequiredString(JsonElement section, string key)
    {
        if (!section.TryGetProperty(key, out var value))
        {
            throw new InvalidOperationException($"testsettings.json: missing required key '{key}'");
        }

        return value.GetString()
            ?? throw new InvalidOperationException($"testsettings.json: key '{key}' must be a string");
    }

    private static bool GetRequiredBool(JsonElement section, string key)
    {
        if (!section.TryGetProperty(key, out var value))
        {
            throw new InvalidOperationException($"testsettings.json: missing required key '{key}'");
        }

        if (value.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
        {
            throw new InvalidOperationException($"testsettings.json: key '{key}' must be a boolean");
        }

        return value.GetBoolean();
    }

    private static string GetRequiredEnvironmentValue(string key) =>
        ReadEnvironmentValue(key)
            ?? throw new InvalidOperationException(
                $"environment variable '{key}' not found (process environment or .env)");

    private static string? ReadEnvironmentValue(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        return ReadEnvFileValue(key);
    }

    private static string? ReadEnvFileValue(string key)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var envFile = Path.Combine(dir, ".env");
            if (!File.Exists(envFile))
            {
                dir = Directory.GetParent(dir)?.FullName;
                continue;
            }

            var line = File.ReadAllLines(envFile).FirstOrDefault(candidate => candidate.StartsWith($"{key}="));
            if (line is not null)
            {
                return line[(key.Length + 1)..];
            }

            break;
        }

        return null;
    }
}
