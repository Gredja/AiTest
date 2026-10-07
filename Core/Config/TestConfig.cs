using System.Text.Json;

namespace Core.Config;

public static class TestConfig
{
    private const string BaseUrlKey = "BaseUrl";

    private static readonly JsonDocument _root = JsonDocument.Parse(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "testsettings.json")));

    private static JsonElement JsonPlaceholder => _root.RootElement.GetProperty("JsonPlaceholder");
    private static JsonElement GitHub => _root.RootElement.GetProperty("GitHub");

    public static int MaxResponseTimeMs => _root.RootElement.GetProperty("MaxResponseTimeMs").GetInt32();
    public static bool IsSslValidationSkipped => _root.RootElement.GetProperty("SkipSslValidation").GetBoolean();

    public static string JsonPlaceholderBaseUrl => JsonPlaceholder.GetProperty(BaseUrlKey).GetString()!;

    public static string GitHubBaseUrl => GitHub.GetProperty(BaseUrlKey).GetString()!;
    public static string GitHubToken
    {
        get
        {
            var token = GitHub.GetProperty("Token").GetString();
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            return ReadTokenFromEnvFile();
        }
    }

    public static string GitHubTestRepo => GitHub.GetProperty("TestRepo").GetString()!;
    public static string GitHubTestUsername => GitHub.GetProperty("TestUsername").GetString()!;

    private static string ReadTokenFromEnvFile()
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

            var line = File.ReadAllLines(envFile).FirstOrDefault(line => line.StartsWith("GITHUB_PAT="));
            if (line is not null)
            {
                return line["GITHUB_PAT=".Length..];
            }

            break;
        }

        return string.Empty;
    }
}
