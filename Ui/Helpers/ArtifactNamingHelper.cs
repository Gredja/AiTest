using System.Text;

namespace Ui.Helpers;

internal static class ArtifactNamingHelper
{
    internal static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(name.Length);

        foreach (var character in name)
        {
            if (!invalidChars.AsSpan().Contains(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
