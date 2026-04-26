using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicFinder.Models;

namespace MusicFinder.Actions;

public class AddPrice(ILogger<AddPrice> logger, MusicFinderContext dbContext)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding price to albums...");

        Console.Write("Enter the album title: ");
        var title = Console.ReadLine();

        while (string.IsNullOrEmpty(title))
        {
            Console.Write("Album title cannot be empty. Please enter the album title: ");
            title = Console.ReadLine();
        }

        Console.Write("Enter the artist name: ");
        var artist = Console.ReadLine();
        while (string.IsNullOrEmpty(artist))
        {
            Console.Write("Artist name cannot be empty. Please enter the artist name: ");
            artist = Console.ReadLine();
        }

        var album = await dbContext.Albums.FirstOrDefaultAsync(a => a.Name == title && a.Artist == artist, cancellationToken);
        if (album == null)
        {
            logger.LogWarning("Album '{Title}' by '{Artist}' not found in the database.", title, artist);
            return;
        }

        Console.Write("Enter the provider name: ");
        var provider = Console.ReadLine();
        while (string.IsNullOrEmpty(provider))
        {
            Console.Write("Provider name cannot be empty. Please enter the provider name: ");
            provider = Console.ReadLine();
        }

        Console.Write("Enter the price: ");
        var priceInput = Console.ReadLine();
        decimal price;

        // allow both dot and comma as decimal separator
        priceInput = priceInput?.Replace('.', ',');

        while (!decimal.TryParse(priceInput, out price) || price < 0)
        {
            Console.Write("Invalid price. Please enter a valid price: ");
            priceInput = Console.ReadLine();

            priceInput = priceInput?.Replace('.', ',');
        }

        await SubmitPrice(album, provider, price, dbContext, cancellationToken);

        await Task.CompletedTask;
    }

    internal static async Task SubmitPrice(Album album, string provider, decimal price, MusicFinderContext dbContext, CancellationToken cancellationToken)
    {
        var existingPrice = await dbContext.Prices.FirstOrDefaultAsync(p => p.AlbumId == album.Id && p.Provider == provider, cancellationToken);
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
            await dbContext.Prices.AddAsync(newPrice, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
