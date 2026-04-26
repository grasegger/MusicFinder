using Microsoft.Extensions.Logging;
using MusicFinder.Models;

namespace MusicFinder.Actions;

public class RemoveAlbum(ILogger<RemoveAlbum> logger, MusicFinderContext context)
{

    public Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Running RemoveAlbum action");

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

        var existingAlbum = context.Albums.FirstOrDefault(a => a.Name == album && a.Artist == artist);

        if (existingAlbum == null)
        {
            logger.LogWarning("Album '{Album}' by '{Artist}' does not exist in the database", album, artist);
            return Task.CompletedTask;
        }
        
        context.Albums.Remove(existingAlbum);
        context.SaveChanges();

        return Task.CompletedTask;
    }
}
