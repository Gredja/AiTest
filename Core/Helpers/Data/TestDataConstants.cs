namespace Core.Helpers;

// Shared test-data constants: one value, one meaning, declared once —
// per-class redeclaration of the same semantic value violates Rules/code-style.md → Magic strings
public static class TestDataConstants
{
    public const int TitleRandomLength = 8;
    public const int BodyRandomLength = 16;
    public const int SingleItemPageSize = 1;
    public const string NonNumericValue = "abc";
    public const string SpecialCharsSegment = "sp@ec!al";
}
