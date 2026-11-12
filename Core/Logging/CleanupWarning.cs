namespace Core.Logging;

public static class CleanupWarning
{
    // Cleanup failures are warnings, never run failures (Rules/test-practices.md → E2E cleanup)
    public const string Prefix = "Warning: ";
}
