using System.Text.Json;

namespace Core.Config;

public static class TestConfig
{
    private static readonly Lazy<JsonDocument> Document = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "testsettings.json");
        var json = File.ReadAllText(path);
        return JsonDocument.Parse(json);
    });

    private static JsonElement Root => Document.Value.RootElement;

    public static string BaseUrl => Root.GetProperty("BaseUrl").GetString()!;
    public static int MaxResponseTimeMs => Root.GetProperty("MaxResponseTimeMs").GetInt32();
    public static int ExpectedProductCount => Root.GetProperty("ExpectedProductCount").GetInt32();
    public static int ExpectedUserCount => Root.GetProperty("ExpectedUserCount").GetInt32();
    public static int ExpectedCartCount => Root.GetProperty("ExpectedCartCount").GetInt32();
    public static int ExpectedCategoryCount => Root.GetProperty("ExpectedCategoryCount").GetInt32();
    public static int ExpectedProductsInCategoryCount => Root.GetProperty("ExpectedProductsInCategoryCount").GetInt32();
    public static string TestCategoryName => Root.GetProperty("TestCategoryName").GetString()!;
    public static string LoginUsername => Root.GetProperty("Login").GetProperty("Username").GetString()!;
    public static string LoginPassword => Root.GetProperty("Login").GetProperty("Password").GetString()!;
}
