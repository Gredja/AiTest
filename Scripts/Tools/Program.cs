namespace Gredja.Tools;

internal static class Program
{
    private const string ValidateSkillVerb = "validate-skill";
    private const string LocatorDumpVerb = "locator-dump";
    private const string ServeSlackVerb = "serve-slack";
    private const string SlackInboxVerb = "slack-inbox";

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 2 && args[0] == ValidateSkillVerb)
        {
            return SkillValidator.Run(args[1]);
        }

        if (args.Length >= 1 && args[0] == LocatorDumpVerb)
        {
            return await LocatorDump.RunAsync(args.Skip(1).ToArray());
        }

        if (args.Length == 1 && args[0] == ServeSlackVerb)
        {
            return await ServeSlack.RunAsync();
        }

        if (args.Length == 2 && args[0] == SlackInboxVerb)
        {
            return await SlackInbox.RunAsync(args[1]);
        }

        Console.WriteLine(
            "Gredja tool console — skill validator, locator dump, local Slack MCP server, Slack inbox poller.\n" +
            "\n" +
            "Usage:\n" +
            "  dotnet run --project Scripts/Tools -- validate-skill /path/to/skill-folder\n" +
            "  dotnet run --project Scripts/Tools -- locator-dump <url> [options]   (see --help)\n" +
            "  dotnet run --project Scripts/Tools -- serve-slack                    (stdio MCP server)\n" +
            "  dotnet run --project Scripts/Tools -- slack-inbox <sessionID>        (Slack → live session)\n" +
            "\n" +
            "Exit code 0 = success, 1 = failure, 2 = usage error (validate-skill).");
        return 2;
    }
}
