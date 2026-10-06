namespace AllureAdapter.Helpers;

internal static class AllureConstants
{
    public const string StatusSkipped = "skipped";
    public const string StatusFailed = "failed";
    public const string StatusPassed = "passed";
    public const string StatusBroken = "broken";
    public const string DefaultIgnoreMessage = "Ignored by [Ignore] attribute";
    public const string LabelSuite = "suite";
    public const string UuidKey = "uuid";
    public const string IdKey = "id";
    public const string NameKey = "name";
    public const string ValueKey = "value";
    public const string TimeKey = "time";
    public const string LabelsKey = "labels";
    public const string StartKey = "start";
    public const string StopKey = "stop";
    public const string DurationKey = "duration";
}
