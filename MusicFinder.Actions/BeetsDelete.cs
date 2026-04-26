using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MusicFinder.Models;
using YamlDotNet.Serialization;

namespace MusicFinder.Actions;

public class BeetsDelete(ILogger<BeetsDelete> logger, MusicFinderContext context)
{
    private readonly ILogger<BeetsDelete> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly MusicFinderContext context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running BeetsDelete action...");

        string output = await ReadBeetsConfig();

        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        var beetsConfig = deserializer.Deserialize<BeetsConfig>(output);
        if (beetsConfig == null || string.IsNullOrEmpty(beetsConfig.library))
        {
            logger.LogError("Failed to read beets config or library path is empty.");
            return;
        }

        var existing = GetAlbumsFromBeets(beetsConfig.library);
        if (!existing.Any())
        {
            logger.LogInformation("No albums found in beets library.");
            return;
        }

        logger.LogInformation($"Found {existing.Count()} albums in beets library. Deleting from MusicFinder database...");

        DeleteExistingAlbums(existing);

    }

    private void DeleteExistingAlbums(IEnumerable<Album> existing)
    {
        foreach (var album in existing)
        {
            var albumInDb = context.Albums.FirstOrDefault(a => a.Name == album.Name && a.Artist == album.Artist);
            if (albumInDb != null)
            {
                context.Albums.Remove(albumInDb);
            }
        }
    }

    private static async Task<string> ReadBeetsConfig()
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "beet",
                Arguments = "config -d",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        proc.Start();
        var output = await proc.StandardOutput.ReadToEndAsync();
        proc.WaitForExit();
        return output;
    }

    private IEnumerable<Album> GetAlbumsFromBeets(string dbPath)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT albumartist, album FROM albums";
        using var reader = command.ExecuteReader();
        var albums = new List<Album>();
        while (reader.Read())
        {
            var artist = reader.GetString(0);
            var name = reader.GetString(1);
            logger.LogDebug($"Found album in beets library: {artist} - {name}");
            albums.Add(new Album { Artist = artist, Name = name });
        }

        return albums;
    }
}
