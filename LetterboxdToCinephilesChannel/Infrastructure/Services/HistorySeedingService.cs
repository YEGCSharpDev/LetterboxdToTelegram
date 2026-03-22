using LetterboxdToCinephilesChannel.Domain.Entities;
using LetterboxdToCinephilesChannel.Infrastructure.Data;
using LetterboxdToCinephilesChannel.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LetterboxdToCinephilesChannel.Infrastructure.Services;

public class HistorySeedingService(
    AppDbContext dbContext,
    ILogger<HistorySeedingService> logger,
    IServiceScopeFactory scopeFactory)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Only run if the database is empty
        if (await dbContext.ProcessedMovies.AnyAsync(ct))
        {
            logger.LogInformation("Database already initialized. Skipping seeding.");
            return;
        }

        logger.LogInformation("First run detected. Seeding database from RSS feed to establish baseline...");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var rssClient = scope.ServiceProvider.GetRequiredService<RssClient>();
            var items = await rssClient.GetFeedAsync(ct);

            foreach (var item in items)
            {
                dbContext.ProcessedMovies.Add(new ProcessedMovie
                {
                    LetterboxdId = item.Guid,
                    ImdbId = item.ImdbId,
                    Title = item.FilmTitle,
                    Year = int.TryParse(item.FilmYear, out var y) ? y : null,
                    ProcessedAt = DateTime.UtcNow
                });
            }

            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("Successfully initialized database with {Count} movies from RSS. Moving forward, only new movies will be posted.", items.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to perform initial database seeding from RSS.");
            // We don't throw here to allow the app to attempt to start anyway
        }
    }
}
