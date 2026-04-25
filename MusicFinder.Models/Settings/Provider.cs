using System;

namespace MusicFinder.Models.Settings;

public class Provider
{
    public required string Name { get; set; }
    public required string UrlTemplate { get; set; }
}
