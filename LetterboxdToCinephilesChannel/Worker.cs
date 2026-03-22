using LetterboxdToCinephilesChannel.Domain.Entities;
using LetterboxdToCinephilesChannel.Infrastructure.Data;
using LetterboxdToCinephilesChannel.Infrastructure.Http;
using LetterboxdToCinephilesChannel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LetterboxdToCinephilesChannel;

public class Worker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = configuration.GetValue("WORKER__POLLINGINTERVALMINUTES", 10);
        logger.LogInformation("Worker starting at: {time} with interval {interval} minutes", 
            DateTimeOffset.Now, intervalMinutes);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

        try
        {
            // Initial run immediately
            await DoWorkAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Worker crashed unexpectedly.");
            try
            {
                using var scope = scopeFactory.CreateScope();
                var reporter = scope.ServiceProvider.GetRequiredService<ErrorReportingService>();
                await reporter.ReportErrorAsync("Worker crashed unexpectedly.", ex, stoppingToken);
            }
            catch (Exception reportEx)
            {
                logger.LogError(reportEx, "Failed to report worker crash.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker is stopping, performing SQLite WAL checkpoint...");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);", cancellationToken);
            logger.LogInformation("SQLite WAL checkpoint (TRUNCATE) completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error performing SQLite WAL checkpoint during shutdown.");
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Checking for new Letterboxd items at: {time}", DateTimeOffset.Now);

        using var scope = scopeFactory.CreateScope();
        var rssClient = scope.ServiceProvider.GetRequiredService<RssClient>();
        var tmdbClient = scope.ServiceProvider.GetRequiredService<TmdbClient>();
        var telegramService = scope.ServiceProvider.GetRequiredService<TelegramService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reporter = scope.ServiceProvider.GetRequiredService<ErrorReportingService>();

        try
        {
            var feedItems = await rssClient.GetFeedAsync(stoppingToken);
            logger.LogInformation("Found {Count} items in the RSS feed", feedItems.Count);

            // Process items in reverse order (oldest first) to maintain chronological order in the channel
            foreach (var item in feedItems.AsEnumerable().Reverse())
            {
                if (stoppingToken.IsCancellationRequested) break;

                var isProcessed = await db.ProcessedMovies.AnyAsync(m => m.LetterboxdId == item.Guid, stoppingToken);
                if (isProcessed) continue;

                logger.LogInformation("Processing new item: {Title}", item.FilmTitle);

                TmdbMovieDetails? tmdbDetails = null;
                if (int.TryParse(item.TmdbId, out var tmdbId) && tmdbId > 0)
                {
                    tmdbDetails = await tmdbClient.GetMovieDetailsAsync(tmdbId, stoppingToken);
                }
                
                if (tmdbDetails == null && !string.IsNullOrEmpty(item.FilmTitle))
                {
                    if (int.TryParse(item.FilmYear, out var year))
                    {
                        var searchResult = await tmdbClient.GetMovieByTitleAndYearAsync(item.FilmTitle, year, stoppingToken);
                        if (searchResult != null)
                        {
                            tmdbDetails = await tmdbClient.GetMovieDetailsAsync(searchResult.Id, stoppingToken);
                        }
                    }
                }

                var message = await telegramService.SendMovieCardAsync(item, tmdbDetails, stoppingToken);
                if (message != null)
                {
                    db.ProcessedMovies.Add(new ProcessedMovie
                    {
                        LetterboxdId = item.Guid,
                        ImdbId = item.ImdbId,
                        Title = item.FilmTitle,
                        Year = int.TryParse(item.FilmYear, out var y) ? y : null,
                        ProcessedAt = DateTime.UtcNow,
                        TelegramMessageId = message.MessageId
                    });

                    await db.SaveChangesAsync(stoppingToken);
                    logger.LogInformation("Successfully processed and saved: {Title}", item.FilmTitle);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during feed processing");
            await reporter.ReportErrorAsync("Error occurred during feed processing", ex, stoppingToken);
        }
    }
}
