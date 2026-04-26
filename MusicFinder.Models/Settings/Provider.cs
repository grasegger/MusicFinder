namespace MusicFinder.Models.Settings;

public class Provider
{
    public required string Name { get; set; }
    public required Uri UrlTemplate { get; set; }
}
