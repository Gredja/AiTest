using System.Text.Json;

namespace Ui.Moodledemo.Tests.Support;

internal static class MoodledemoFixtures
{
    private const string EnglishCode = "en";

    private static readonly string _fixturePath = Path.Combine(
        AppContext.BaseDirectory, "moodledemo", "Fixtures", "expected-langs.json");

    private static readonly FixtureRoot _root = JsonSerializer.Deserialize<FixtureRoot>(
        System.IO.File.ReadAllText(_fixturePath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException($"fixture file not found or invalid: {_fixturePath}");

    internal static IReadOnlyList<LocaleExpectation> Home => _root.Home;

    internal static IReadOnlyList<LocaleExpectation> LoginPage => _root.LoginPage;

    internal static LocaleExpectation English =>
        Home.Single(expectation => expectation.Code == EnglishCode);

    private sealed record FixtureRoot(
        List<LocaleExpectation> Home,
        List<LocaleExpectation> LoginPage);
}
