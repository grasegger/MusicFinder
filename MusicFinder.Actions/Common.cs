using System;
using Microsoft.EntityFrameworkCore;
using MusicFinder.Models;

namespace MusicFinder.Actions;

internal static class Common
{

    internal static async Task SubmitPrice(Album album, string provider, decimal price, MusicFinderContext dbContext, CancellationToken cancellationToken)
    {
        var existingPrice = await dbContext.Prices.FirstOrDefaultAsync(p => p.AlbumId == album.Id && p.Provider == provider, cancellationToken).ConfigureAwait(false);
        if (existingPrice != null)
        {
            existingPrice.Value = price;
            existingPrice.LastUpdated = DateTime.UtcNow;
            dbContext.Prices.Update(existingPrice);
        }
        else
        {
            var newPrice = new Price
            {
                AlbumId = album.Id,
                Provider = provider,
                Value = price,
                LastUpdated = DateTime.UtcNow
            };
            await dbContext.Prices.AddAsync(newPrice, cancellationToken).ConfigureAwait(false);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
