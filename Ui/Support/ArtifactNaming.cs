namespace Ui.Support;

internal static class ArtifactNaming
{
    internal static string SanitizeFileName(string name) =>
        string.Concat(name.Where(character => !Path.GetInvalidFileNameChars().Contains(character)));
}
