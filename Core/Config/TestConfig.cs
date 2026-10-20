using System.Text.Json;

namespace Core.Config;

public static class TestConfig
{
    private const string BaseUrlKey = "BaseUrl";

    public const string SolutionFileName = "Gredja.slnx";
    public const string AllureResultsDirName = "allure-results";
    public const string ResultsDirName = "TestResults";

    private static readonly JsonDocument _root = JsonDocument.Parse(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "testsettings.json")));

    private static JsonElement JsonPlaceholder => GetRequiredSection("JsonPlaceholder");
    private static JsonElement GitHub => GetRequiredSection("GitHub");
    private static JsonElement Ui => GetRequiredSection("Ui");

    public static int MaxResponseTimeMs => GetRequiredInt("MaxResponseTimeMs");

    // Off by default: TLS validation may only be weakened by an explicit per-run opt-in
    public static bool IsSslValidationSkipped =>
        _root.RootElement.TryGetProperty("SkipSslValidation", out var value)
        && value.ValueKind is JsonValueKind.True;

    public static string JsonPlaceholderBaseUrl => GetRequiredString(JsonPlaceholder, BaseUrlKey);

    public static int ExpectedPostCount => GetRequiredInt(JsonPlaceholder, "ExpectedPostCount");
    public static int ExpectedTodoCount => GetRequiredInt(JsonPlaceholder, "ExpectedTodoCount");
    public static int ExpectedAlbumCount => GetRequiredInt(JsonPlaceholder, "ExpectedAlbumCount");
    public static int ExpectedCommentCount => GetRequiredInt(JsonPlaceholder, "ExpectedCommentCount");
    public static int ExpectedPhotoCount => GetRequiredInt(JsonPlaceholder, "ExpectedPhotoCount");
    public static int ExpectedUserCount => GetRequiredInt(JsonPlaceholder, "ExpectedUserCount");

    public static string GitHubBaseUrl => GetRequiredString(GitHub, BaseUrlKey);
    public static string GitHubToken
    {
        get
        {
            var token = GitHub.TryGetProperty("Token", out var tokenValue) ? tokenValue.GetString() : null;
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            return GetRequiredEnvironmentValue("GITHUB_PAT");
        }
    }

    public static string GitHubTestRepo => GetRequiredString(GitHub, "TestRepo");
    public static string GitHubTestUsername => GetRequiredString(GitHub, "TestUsername");
    public static string GitHubCollaboratorUser => GetRequiredString(GitHub, "CollaboratorUser");

    public static string UiBaseUrl => GetRequiredString(Ui, BaseUrlKey);
    public static bool IsUiHeadless => GetRequiredBool(Ui, "Headless");
    public static string UiStatePath => GetRequiredString(Ui, "StatePath");
    public static string UiUsername => GetRequiredString(Ui, "Username");
    public static string UiTestRepo => GetRequiredString(Ui, "TestRepo");
    public static string UiLogin => GetRequiredEnvironmentValue("GITHUB_UI_EMAIL");
    public static string UiPassword => GetRequiredEnvironmentValue("GITHUB_UI_PASSWORD");
    public static string UiApiToken => GetRequiredEnvironmentValue("GITHUB_UI_PAT");

    private static JsonElement GetRequiredSection(string name) =>
        _root.RootElement.TryGetProperty(name, out var section)
            ? section
            : throw new InvalidOperationException($"testsettings.json: missing required section '{name}'");

    private static int GetRequiredInt(string key) => GetRequiredInt(_root.RootElement, key);

    private static int GetRequiredInt(JsonElement section, string key)
    {
        if (!section.TryGetProperty(key, out var value))
        {
            throw MissingRequiredKey(key);
        }

        if (value.ValueKind is not JsonValueKind.Number)
        {
            throw new InvalidOperationException($"testsettings.json: key '{key}' must be an integer");
        }

        return value.GetInt32();
    }

    private static string GetRequiredString(JsonElement section, string key)
    {
        if (!section.TryGetProperty(key, out var value))
        {
            throw MissingRequiredKey(key);
        }

        return value.GetString()
            ?? throw new InvalidOperationException($"testsettings.json: key '{key}' must be a string");
    }

    private static bool GetRequiredBool(JsonElement section, string key)
    {
        if (!section.TryGetProperty(key, out var value))
        {
            throw MissingRequiredKey(key);
        }

        if (value.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
        {
            throw new InvalidOperationException($"testsettings.json: key '{key}' must be a boolean");
        }

        return value.GetBoolean();
    }

    private static InvalidOperationException MissingRequiredKey(string key) =>
        new($"testsettings.json: missing required key '{key}'");

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
