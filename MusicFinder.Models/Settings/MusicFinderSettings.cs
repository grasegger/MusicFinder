
namespace MusicFinder.Models.Settings;

using System.Reflection;

public class MusicFinderSettings
{
    public string Action { get; set; } = "Help";

    #region database settings
    public string DataDirectory { get; set; } = Xdg.Directories.BaseDirectory.DataHome ?? "." + Assembly.GetExecutingAssembly().GetName().Name ?? "MusicFinder";
    public string DatabaseName { get; set; } = "musicfinder.db";

    #endregion

    #region import settings
    public string ImportPath { get; set; } = string.Empty;

    public char ImportDelimiter { get; set; } = ',';
    #endregion

}
