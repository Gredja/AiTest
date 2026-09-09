using System.Text.Json;

namespace Core.Config;

public static class TestConfig
{
    private static readonly JsonDocument Root = JsonDocument.Parse(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "testsettings.json")));

    private static JsonElement FakeStore => Root.RootElement.GetProperty("FakeStore");
    private static JsonElement JsonPlaceholder => Root.RootElement.GetProperty("JsonPlaceholder");

    public static int MaxResponseTimeMs => Root.RootElement.GetProperty("MaxResponseTimeMs").GetInt32();
    public static bool SkipSslValidation => Root.RootElement.GetProperty("SkipSslValidation").GetBoolean();

    public static string FakeStoreBaseUrl => FakeStore.GetProperty("BaseUrl").GetString()!;
    public static int ExpectedProductCount => FakeStore.GetProperty("ExpectedProductCount").GetInt32();
    public static int ExpectedUserCount => FakeStore.GetProperty("ExpectedUserCount").GetInt32();
    public static int ExpectedCartCount => FakeStore.GetProperty("ExpectedCartCount").GetInt32();
    public static int ExpectedCategoryCount => FakeStore.GetProperty("ExpectedCategoryCount").GetInt32();
    public static int ExpectedProductsInCategoryCount => FakeStore.GetProperty("ExpectedProductsInCategoryCount").GetInt32();
    public static string TestCategoryName => FakeStore.GetProperty("TestCategoryName").GetString()!;
    public static string LoginUsername => FakeStore.GetProperty("Login").GetProperty("Username").GetString()!;
    public static string LoginPassword => FakeStore.GetProperty("Login").GetProperty("Password").GetString()!;

    public static string JsonPlaceholderBaseUrl => JsonPlaceholder.GetProperty("BaseUrl").GetString()!;
    public static int ExpectedPostCount => JsonPlaceholder.GetProperty("ExpectedPostCount").GetInt32();
    public static int ExpectedTodoCount => JsonPlaceholder.GetProperty("ExpectedTodoCount").GetInt32();
    public static int ExpectedAlbumCount => JsonPlaceholder.GetProperty("ExpectedAlbumCount").GetInt32();
    public static int ExpectedCommentCount => JsonPlaceholder.GetProperty("ExpectedCommentCount").GetInt32();
    public static int ExpectedPhotoCount => JsonPlaceholder.GetProperty("ExpectedPhotoCount").GetInt32();
    public static int JsonPlaceholderExpectedUserCount => JsonPlaceholder.GetProperty("ExpectedUserCount").GetInt32();
}
