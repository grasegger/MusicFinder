using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicFinder.Models;

public class Price
{
    public long Id { get; set; }
    public int AlbumId { get; set; }
    public string Provider { get; set; } = string.Empty;
    [Column(name: "Price", TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }
    public DateTime LastUpdated { get; set; }

    public Album Album { get; set; } = null!;
}
