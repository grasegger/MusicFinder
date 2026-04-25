using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicFinder.Models.Settings;
using MusicFinder.Models;
using nietras.SeparatedValues;
namespace MusicFinder.Actions;

public class Import(IOptions<MusicFinderSettings> options, MusicFinderContext context, ILogger<Import> logger)
{
    private readonly IOptions<MusicFinderSettings> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly MusicFinderContext context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<Import> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        var path = options.Value.ImportPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogInformation("No import path configured; skipping import.");
            return Task.CompletedTask;
        }

        var delimiter = options.Value.ImportDelimiter;

        ImportFromCsv(path, delimiter);

        return Task.CompletedTask;
    }

    private void ImportFromCsv(string path, char delimiter)
    {
        var albums = new List<Album>();

        using var reader = Sep.New(delimiter).Reader().FromFile(path);
        foreach (var row in reader)
        {
            var name = row["Name"].ToString();
            var artist = row["Artist"].ToString();
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(artist))
            {
                if (!context.Albums.Any(a => a.Name == name && a.Artist == artist))
                {
                    albums.Add(new Album { Name = name, Artist = artist });
                }
            }
        }

        if (albums.Count > 0)
        {
            context.Albums.AddRange(albums);
            context.SaveChanges();
        }
    }
}
