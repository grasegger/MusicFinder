using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicFinder.Models;

public class Price
{
    public long Id { get; set; }

    [Required]
    [ForeignKey(nameof(Album))]
    public long AlbumId { get; set; }

    [Required]
    public string Provider { get; set; } = string.Empty;

    [Column(name: "Price", TypeName = "decimal(18,2)")]
    [Required]
    public decimal Value { get; set; }

    [Required]
    public DateTime LastUpdated { get; set; }

    public Album Album { get; set; } = null!;
}
