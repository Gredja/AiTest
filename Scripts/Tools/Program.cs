namespace Gredja.Tools;

internal static class Program
{
    private const string ValidateSkillVerb = "validate-skill";

    private static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == ValidateSkillVerb)
        {
            return SkillValidator.Run(args[1]);
        }

        Console.WriteLine(
            "Validate an agent skill folder against the skill spec.\n" +
            "\n" +
            "Usage: dotnet run --project Scripts/Tools -- validate-skill /path/to/skill-folder\n" +
            "\n" +
            "Exit code 0 = PASS (warnings allowed), 1 = FAIL (errors found), 2 = usage error.");
        return 2;
    }
}
