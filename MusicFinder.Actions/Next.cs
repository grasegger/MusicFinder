using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicFinder.Models;

namespace MusicFinder.Actions;

public class WhatToBuy(MusicFinderContext context)
{

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // based on the highest score artist find the cheapest album (skipping 0 meaning not found)

        // artists that have albums with prices

        var artists = await context.ArtistWeights.Where(x => context.Albums.Where(a => a.Artist == x.Artist).Any(a => a.Prices.Any())).ToListAsync(cancellationToken).ConfigureAwait(false);

        var artist = artists.OrderByDescending(x => x.Weight).Select(a => a.Artist).First();

        var albums = await context.Albums.Where(x => x.Artist == artist).ToListAsync(cancellationToken).ConfigureAwait(false);

        var prices = await context.Prices.Where(x => albums.Select(a => a.Id).Contains(x.AlbumId) && x.Value > 0).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var price = prices.OrderBy(x => x.Value).FirstOrDefault();

        var album = albums.First(x => x.Id == price!.AlbumId);

        Console.WriteLine($"Next album to buy: {artist} - {album} for {price!.Value}€ at {price!.Provider}");

    }

}
