using System;
using Elo;
using Microsoft.Extensions.Logging;
using MusicFinder.Models;

namespace MusicFinder.Actions;

/// <summary>
/// Uses https://github.com/smcl/Elo.Net to score artists based on user comparisons. The user is presented with two artists and asked to choose which one they prefer (or if they are equal). The ELO ratings are updated based on the user's input, and the process continues until the ratings stabilize or the user chooses to exit. Ratings are saved back to the database after each comparison.
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
public class ScoreArtists(ILogger<ScoreArtists> logger, MusicFinderContext dbContext)
{
    private const double InitialRating = 1600.0;
    private const double KValue = 32.0;
    private const int StabilityThreshold = 5;
    private const int MinComparisonsForStability = 10;
    private EloSystem? eloSystem;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Scoring artists based on ELO system...");

        List<string> artists = [];

        var topTen = dbContext.ArtistWeights.OrderByDescending(aw => aw.Weight).Take(10).Select(aw => aw.Artist).ToList();
        var bottomTen = dbContext.ArtistWeights.OrderBy(aw => aw.Weight).Take(10).Select(aw => aw.Artist).ToList();

        var nonUniqueScores = dbContext.ArtistWeights.ToList().GroupBy(aw => aw.Weight).Where(g => g.Count() > 1).SelectMany(g => g.Select(aw => aw.Artist)).ToList();

        artists.AddRange(topTen);
        artists.AddRange(bottomTen);
        artists.AddRange(nonUniqueScores);

        if (artists.Count < dbContext.Albums.Select(a => a.Artist).Distinct().Count() / 10)
        {
            var random = new Random();
            var allArtists = dbContext.Albums.Select(a => a.Artist).Distinct().ToList();
            while (artists.Count < dbContext.Albums.Select(a => a.Artist).Distinct().Count() / 10)
            {
                var randomArtist = allArtists[random.Next(allArtists.Count)];
                if (!artists.Contains(randomArtist))
                {
                    artists.Add(randomArtist);
                }
            }
        }


        if (artists.Count < 2)
        {
            logger.LogInformation("Not enough artists to score. Need at least 2 artists.");

        }
        else
        {

            eloSystem = new EloSystem(new EloSettings(KValue, InitialRating));
            var artistNames = InitializeRatings(artists, eloSystem);
            var artistRatings = new Dictionary<string, double>();
            var random = new Random();
            var comparisonsWithoutChange = 0;
            var totalComparisons = 0;


            // Initial ratings
            foreach (var artist in artistNames)
            {
                var existing = dbContext.ArtistWeights.FirstOrDefault(aw => aw.Artist == artist);
                artistRatings[artist] = existing != null ? (double)existing.Weight : InitialRating;
            }

            var previousRatings = new Dictionary<string, double>(artistRatings);

            while (true)
            {
                var (artist1, artist2) = GetRandomPairing(artistNames, random);

                DisplayComparison(artist1, artist2, artistRatings);
                var userInput = GetUserInput();

                if (userInput == "e")
                {
                    logger.LogInformation("ELO scoring completed by user.");
                    break;
                }

                if (!int.TryParse(userInput, out var choice) || choice < 0 || choice > 3)
                {
                    logger.LogWarning("Invalid input {UserInput}. Please enter 0, 1, or 2.", userInput);
                    break;
                }

                var result = ConvertChoiceToResult(choice);
                eloSystem.AddResults([new Result(artist1, artist2, result)]);
                totalComparisons++;

                // Update ratings from eloSystem
                UpdateRatingsFromEloSystem(artistRatings);

                if (totalComparisons >= MinComparisonsForStability && HasRatingsStabilized(previousRatings, artistRatings))
                {
                    comparisonsWithoutChange++;
                    if (comparisonsWithoutChange >= StabilityThreshold)
                    {
                        break;
                    }
                }
                else
                {
                    comparisonsWithoutChange = 0;
                    previousRatings = new Dictionary<string, double>(artistRatings);
                }
            }

            SaveRatings(artistRatings);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Finished scoring artists.");
        }
    }

    private List<string> InitializeRatings(List<string> artists, EloSystem eloSystem)
    {
        foreach (var artist in artists)
        {
            var existing = dbContext.ArtistWeights.FirstOrDefault(aw => aw.Artist == artist);

            if (existing == null)
            {
                var newWeight = new ArtistWeight { Artist = artist, Weight = (decimal)InitialRating };
                dbContext.ArtistWeights.Add(newWeight);
            }

            eloSystem.AddResults([new Result(artist, artist, 0.5f)]);
        }

        return artists;
    }

    private static (string, string) GetRandomPairing(List<string> artists, Random random)
    {
        var index1 = random.Next(artists.Count);
        var index2 = random.Next(artists.Count);

        while (index2 == index1)
        {
            index2 = random.Next(artists.Count);
        }

        return (artists[index1], artists[index2]);
    }

    private static void DisplayComparison(string artist1, string artist2, Dictionary<string, double> ratings)
    {
        var rating1 = ratings.TryGetValue(artist1, out var r1) ? r1 : InitialRating;
        var rating2 = ratings.TryGetValue(artist2, out var r2) ? r2 : InitialRating;

        Console.WriteLine($"\nCompare: {artist1} ({rating1:F1}) vs {artist2} ({rating2:F1})");
        Console.WriteLine("Enter: 1 = prefer left, 3 = prefer right, anything else = tie, or 'exit' to stop");
        Console.Write("> ");
    }

    private static string GetUserInput()
    {

        // dont wait for enter and read single key input
        var keyInfo = Console.ReadKey(intercept: true);
        Console.WriteLine(); // move to next line after key press   

        return keyInfo.KeyChar.ToString();
    }

    private static float ConvertChoiceToResult(int choice)
    {
        return choice switch
        {
            1 => 1.0f,      // Losss
            3 => 0.0f,      // Exit treated as loss for artist1
            _ => 0.5f       // Default to draw
        };
    }

    private void UpdateRatingsFromEloSystem(Dictionary<string, double> artistRatings)
    {
        foreach (var competitor in eloSystem!.Rankings)
        {
            if (artistRatings.ContainsKey(competitor.CompetitorId))
            {
                artistRatings[competitor.CompetitorId] = competitor.Rating;
            }
        }
    }

    private static bool HasRatingsStabilized(Dictionary<string, double> previous, Dictionary<string, double> current)
    {
        const double threshold = 0.1;

        foreach (var artist in previous.Keys)
        {
            if (!current.TryGetValue(artist, out double value))
                return false;

            if (Math.Abs(previous[artist] - value) > threshold)
            {
                return false;
            }
        }

        return true;
    }

    private void SaveRatings(Dictionary<string, double> artistRatings)
    {
        foreach (var kvp in artistRatings)
        {
            var existing = dbContext.ArtistWeights.FirstOrDefault(aw => aw.Artist == kvp.Key);
            existing?.Weight = (decimal)kvp.Value;
        }
    }
}
