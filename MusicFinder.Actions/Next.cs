using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicFinder.Models;
using MusicFinder.Models.Settings;

namespace MusicFinder.Actions;

public class WhatToBuy(MusicFinderContext context, IOptions<MusicFinderSettings> settings, ILogger<WhatToBuy> logger)
{
    readonly MusicFinderSettings settings = settings.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {

        var artists = await context.ArtistWeights.Where(x => context.Albums.Where(a => a.Artist == x.Artist).Any(a => a.Prices.Any())).ToListAsync(cancellationToken).ConfigureAwait(false);

        var artist = artists.OrderByDescending(x => x.Weight).Select(a => a.Artist).First();

        var albums = await context.Albums.Where(x => x.Artist == artist).ToListAsync(cancellationToken).ConfigureAwait(false);

        var prices = await context.Prices.Where(x => albums.Select(a => a.Id).Contains(x.AlbumId) && x.Value > 0).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var price = prices.OrderBy(x => x.Value).FirstOrDefault();

        var album = albums.First(x => x.Id == price!.AlbumId);

        Console.WriteLine($"Next album to buy: {artist} - {album.Name} for {price!.Value}€ at {price!.Provider}");

        var provider = settings.Providers.FirstOrDefault(p => p.Name == price.Provider);

        if (provider != null)
        {
            var albumArtist = album.Artist.Replace("\"", "", StringComparison.InvariantCulture);
            var albumName = album.Name.Replace("\"", "", StringComparison.InvariantCulture);
            var url = provider.UrlTemplate.AbsoluteUri;
            var artistTemplate = Uri.EscapeDataString("{artist}");
            var albumTemplate = Uri.EscapeDataString("{album}");
            url = url.Replace(artistTemplate, Uri.EscapeDataString(albumArtist), StringComparison.InvariantCulture)
                                          .Replace(albumTemplate, Uri.EscapeDataString(albumName), StringComparison.InvariantCulture);

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
#pragma warning disable CA1849
                Thread.Sleep(1000);
#pragma warning restore CA1849

            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to open URL: {Url}", url);
            }
#pragma warning restore CA1031
        } else
        {
            logger.LogInformation("No matching provider in config.");
        }

    }

}
