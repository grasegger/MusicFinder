using Microsoft.Extensions.Logging;
using MusicFinder.Models;

namespace MusicFinder.Actions;

public class AddAlbum(ILogger<AddAlbum> logger, MusicFinderContext context)
{

    public Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Running AddAlbum action");

        Console.Write("Enter album name: ");
        var album = Console.ReadLine();

        while (string.IsNullOrEmpty(album))
        {
            Console.Write("Album name cannot be empty. Please enter album name: ");
            album = Console.ReadLine();
        }

        Console.Write("Enter artist name: ");
        var artist = Console.ReadLine();

        while (string.IsNullOrEmpty(artist))
        {
            Console.Write("Artist name cannot be empty. Please enter artist name: ");
            artist = Console.ReadLine();
        }

        var newAlbum = new Album
        {
            Name = album,
            Artist = artist
        };

        if (context.Albums.Any(a => a.Name == album && a.Artist == artist))
        {
            logger.LogWarning("Album '{Album}' by '{Artist}' already exists in the database", album, artist);
            return Task.CompletedTask;
        }
        
        context.Albums.Add(newAlbum);
        context.SaveChanges();

        logger.LogInformation("Added album '{Album}' by '{Artist}' to the database", album, artist);

        return Task.CompletedTask;
    }
}