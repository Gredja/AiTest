namespace Core.Config;

public static class MoodledemoEndpoints
{
    // Official public Moodle demo (Mount Orange) — fixed site, no per-run override needed
    public const string BaseUrl = "https://school.moodledemo.net";
    public const string HomePath = "/";
    public const string LoginPath = "/login/index.php";
    public const string LangQueryKey = "lang";
    public const string UnknownLangCode = "xx_notreal";
}
