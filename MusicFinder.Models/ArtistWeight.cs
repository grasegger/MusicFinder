using System.ComponentModel.DataAnnotations;

namespace MusicFinder.Models;

public class ArtistWeight
{
    public int Id { get; set; }

    [Required]
    public string Artist { get; set; } = string.Empty;

    [Required]
    public decimal Weight { get; set; }
}
