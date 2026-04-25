
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MusicFinder.Models.Settings;

namespace MusicFinder.Models;

public class MusicFinderContext(DbContextOptions<MusicFinderContext> options) : DbContext(options)
{
    public DbSet<Album> Albums { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}
