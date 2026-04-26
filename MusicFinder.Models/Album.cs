using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MusicFinder.Models;

[Index(nameof(Name), nameof(Artist), IsUnique = true)]
public class Album
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Artist { get; set; } = string.Empty;

    public ICollection<Price> Prices { get; } = [];
}