namespace Ui.Helpers;

internal static class ArtifactNamingHelper
{
    internal static string SanitizeFileName(string name) =>
        string.Concat(name.Where(character => !Path.GetInvalidFileNameChars().Contains(character)));
}
