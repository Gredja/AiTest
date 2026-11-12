namespace Core.Logging;

public static class LogText
{
    // Warning-prefixed progress lines are informational — cleanup failures and flaky
    // external signals must never fail the run (Rules/test-practices.md → E2E cleanup)
    public const string Warning = "Warning: ";
}
