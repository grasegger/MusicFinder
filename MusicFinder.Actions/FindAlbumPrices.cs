using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicFinder.Models;
using MusicFinder.Models.Settings;

namespace MusicFinder.Actions;

public class FindAlbumPrices(ILogger<FindAlbumPrices> logger, MusicFinderContext dbContext, IOptions<MusicFinderSettings> settings)
{
    readonly MusicFinderSettings settings = settings.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {

        var albums = new List<Album>();
        var artists = dbContext.ArtistWeights.OrderByDescending(a => a.Weight).Select(a => a.Artist);

        while (albums.Count < settings.AlbumsToSearchPricesFor && artists.Any())
        {
            var artist = artists.First();
            artists = artists.Skip(1);

            var artistAlbums = dbContext.Albums.Where(a => a.Artist == artist).ToList();

            var prices = dbContext.Prices.Where(p => artistAlbums.Select(a => a.Id).Contains(p.AlbumId) && p.LastUpdated > DateTime.UtcNow.AddDays(-10)).ToList();

            artistAlbums = [.. artistAlbums.Where(a => !prices.Any(p => p.AlbumId == a.Id)).Take(0..5)];

            albums.AddRange(artistAlbums);
        }

        if (dbContext.Prices.Count() > settings.AlbumsToSearchPricesFor)
        {
            await SearchPricesAsync([albums.First()], cancellationToken);
        }
        else
        {
            await SearchPricesAsync(albums, cancellationToken);
        }
    }

    private Task SearchPricesAsync(IEnumerable<Album> albums, CancellationToken cancellationToken)
    {
        foreach (var album in albums)
        {
            foreach (var provider in settings.Providers)
            {
                var albumArtist = album.Artist.Replace("\"", "");
                var albumName = album.Name.Replace("\"", "");
                var url = provider.UrlTemplate.Replace("{artist}", Uri.EscapeDataString(albumArtist))
                                              .Replace("{album}", Uri.EscapeDataString(albumName));

                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to open URL: {Url}", url);
                }

                Console.Write($"Enter price for {album.Artist} - {album.Name} on {provider}: ");

                var input = Console.ReadLine() ?? string.Empty;
                input = input.Replace(".", ","); // handle comma as decimal separator

                if (input == "e")
                {
                    continue;
                }
                decimal price;

                while (!decimal.TryParse(input, out price))
                {
                    Console.Write("Invalid price. Please enter a valid decimal number: ");
                    input = Console.ReadLine() ?? string.Empty;
                    input = input.Replace(".", ","); // handle comma as decimal separator
                    if (input == "e")
                    {
                        continue;
                    }
                }

                Common.SubmitPrice(album, provider.Name, price, dbContext, cancellationToken).Wait(cancellationToken);

            }
        }

        return Task.CompletedTask;
    }
}
