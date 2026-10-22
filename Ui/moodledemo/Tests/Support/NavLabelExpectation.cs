namespace Ui.Moodledemo.Tests.Support;

public sealed record NavLabelExpectation(
    string Code,
    string Home,
    string Calendar,
    string More,
    bool HomeIsEnglishGap = false);
