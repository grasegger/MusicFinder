using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MusicFinder.Models;

public class MusicFinderContext() : DbContext("MusicFinder")
{
    public DbSet<Album> Albums { get; set; } = null!;

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
    }
}
