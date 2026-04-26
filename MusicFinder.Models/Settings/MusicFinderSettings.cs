using System.Collections.ObjectModel;
using System.Reflection;

namespace MusicFinder.Models.Settings;

public class MusicFinderSettings
{
    public string Action { get; set; } = "CliHelp";

    #region database settings
    public string DataDirectory { get; set; } = Xdg.Directories.BaseDirectory.DataHome ?? "." + Assembly.GetExecutingAssembly().GetName().Name ?? "MusicFinder";
    public string DatabaseName { get; set; } = "musicfinder.db";

    #endregion

    #region import settings
    public string ImportPath { get; set; } = string.Empty;

    public char ImportDelimiter { get; set; } = ',';
    #endregion

    public Collection<Provider> Providers { get; } = [];
    public int AlbumsToSearchPricesFor { get; set; } = 5;
}
