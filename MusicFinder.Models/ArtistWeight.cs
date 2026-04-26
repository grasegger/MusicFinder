using System;

namespace MusicFinder.Models;

public class ArtistWeight
{
    public int Id { get; set; }
    public string Artist { get; set; } = string.Empty;
    public decimal Weight { get; set; }
}
