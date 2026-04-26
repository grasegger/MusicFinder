using System.IO;

namespace MusicFinder.Models;

public class BeetsConfig
{
#pragma warning disable IDE1006 // Naming Styles
    private string libraryField = string.Empty;

    public required string library
    {
        get => ExpandPath(libraryField);
        set => libraryField = value;
    }
#pragma warning restore IDE1006 // Naming Styles

    private static string ExpandPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        path = Environment.ExpandEnvironmentVariables(path.Trim());

        if (path == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        if (path.StartsWith("~" + Path.DirectorySeparatorChar) || path.StartsWith("~" + Path.AltDirectorySeparatorChar))
        {
            var remainder = path[2..];
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), remainder);
        }

        return path;
    }
}