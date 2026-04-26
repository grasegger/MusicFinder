using Microsoft.EntityFrameworkCore;

namespace MusicFinder.Models;

public class MusicFinderContext(DbContextOptions<MusicFinderContext> options) : DbContext(options)
{
    public DbSet<Album> Albums { get; set; } = null!;
    public DbSet<Price> Prices { get; set; } = null!;
    public DbSet<ArtistWeight> ArtistWeights { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}
