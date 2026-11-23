namespace Gredja.Tools;

internal static class Program
{
    private const string ValidateSkillVerb = "validate-skill";
    private const string LocatorDumpVerb = "locator-dump";

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

        Console.WriteLine(
            "Gredja tool console — validate a skill folder or dump a page accessibility tree.\n" +
            "\n" +
            "Usage:\n" +
            "  dotnet run --project Scripts/Tools -- validate-skill /path/to/skill-folder\n" +
            "  dotnet run --project Scripts/Tools -- locator-dump <url> [options]   (see --help)\n" +
            "\n" +
            "Exit code 0 = success, 1 = failure, 2 = usage error (validate-skill).");
        return 2;
    }
}
