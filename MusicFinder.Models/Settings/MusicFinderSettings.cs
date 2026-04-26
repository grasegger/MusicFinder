using System.Reflection;

namespace MusicFinder.Models.Settings;

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

    public List<Provider> Providers { get; set; } = [];
}
