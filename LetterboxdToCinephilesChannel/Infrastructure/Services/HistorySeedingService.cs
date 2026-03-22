using System.Text.RegularExpressions;
using LetterboxdToCinephilesChannel.Configuration;
using LetterboxdToCinephilesChannel.Domain.Entities;
using LetterboxdToCinephilesChannel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TL;
using WTelegram;

namespace LetterboxdToCinephilesChannel.Infrastructure.Services;

public class HistorySeedingService
{
    private readonly AppDbContext _dbContext;
    private readonly TelegramOptions _options;
    private readonly ILogger<HistorySeedingService> _logger;

    public HistorySeedingService(
        AppDbContext dbContext,
        IOptions<TelegramOptions> options,
        ILogger<HistorySeedingService> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _dbContext.ProcessedMovies.AnyAsync(ct))
        {
            _logger.LogInformation("Database is not empty. Skipping history seeding.");
            return;
        }

        if (_options.ApiId == 0 || string.IsNullOrEmpty(_options.ApiHash))
        {
            _logger.LogWarning("Telegram ApiId or ApiHash not configured. Skipping history seeding.");
            return;
        }

        _logger.LogInformation("Starting history seeding from Telegram...");

        using var client = new Client(config => config switch
        {
            "api_id" => _options.ApiId.ToString(),
            "api_hash" => _options.ApiHash,
            "bot_token" => _options.BotToken,
            _ => null
        });

        try
        {
            await client.LoginBotIfNeeded();
            
            IObject? resolved = null;
            if (_options.ChannelId.StartsWith("@"))
            {
                resolved = await client.Contacts_ResolveUsername(_options.ChannelId.Substring(1));
            }
            else if (long.TryParse(_options.ChannelId, out long id) || 
                     (_options.ChannelId.StartsWith("-100") && long.TryParse(_options.ChannelId.Substring(4), out id)))
            {
                // For numeric IDs, we need to find the chat in the dialogs/chats to get the access_hash
                var chats = await client.Messages_GetAllChats();
                resolved = chats.chats.Values.FirstOrDefault(c => c.ID == id);
            }
            else
            {
                // Try resolving as username without @
                resolved = await client.Contacts_ResolveUsername(_options.ChannelId);
            }

            Channel? channel = resolved switch
            {
                Contacts_ResolvedPeer rp => rp.Chat as Channel,
                Channel c => c,
                _ => null
            };

            if (channel == null)
            {
                _logger.LogError("Could not resolve channel {ChannelId}", _options.ChannelId);
                return;
            }

            int offsetId = 0;
            int totalProcessed = 0;
            var processedLetterboxdIds = new HashSet<string>();

            while (!ct.IsCancellationRequested)
            {
                var history = await client.Messages_GetHistory(channel, offset_id: offsetId, limit: 100);
                if (history is not Messages_ChannelMessages channelMsgs || channelMsgs.messages.Length == 0)
                    break;

                foreach (var msgBase in channelMsgs.messages)
                {
                    if (msgBase is Message msg && !string.IsNullOrEmpty(msg.message))
                    {
                        var movie = ParseMessage(msg);
                        if (movie != null && !processedLetterboxdIds.Contains(movie.LetterboxdId))
                        {
                            _dbContext.ProcessedMovies.Add(movie);
                            processedLetterboxdIds.Add(movie.LetterboxdId);
                            totalProcessed++;
                        }
                    }
                    offsetId = msgBase.ID;
                }

                if (channelMsgs.messages.Length < 100)
                    break;
                
                await Task.Delay(1000, ct); // Rate limiting
            }

            await _dbContext.SaveChangesAsync(ct);
            _logger.LogInformation("Successfully seeded {Count} movies from history.", totalProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during history seeding");
        }
    }

    private ProcessedMovie? ParseMessage(Message msg)
    {
        // Example legacy message: 
        // 🎬 Movie Title (Year)
        // Letterboxd: https://letterboxd.com/film/movie-slug/
        // IMDb: https://www.imdb.com/title/tt1234567/
        
        var text = msg.message ?? string.Empty;
        string? letterboxdId = null;
        string? imdbId = null;
        string? title = null;
        int? year = null;

        // Extract Letterboxd ID
        var lbMatch = Regex.Match(text, @"letterboxd\.com/film/([^/\s\n]+)");
        if (lbMatch.Success)
        {
            letterboxdId = lbMatch.Groups[1].Value.Trim();
        }

        // Extract IMDb ID (tt followed by 7-8 digits)
        var imdbMatch = Regex.Match(text, @"(tt\d{7,})");
        if (imdbMatch.Success)
        {
            imdbId = imdbMatch.Groups[1].Value.Trim();
        }

        // Extract Title and Year from line with emoji
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("🎬") || line.Contains("🎬"))
            {
                var cleanLine = line.Replace("🎬", "").Trim();
                if (cleanLine.Contains("(") && cleanLine.EndsWith(")"))
                {
                    var lastOpenParen = cleanLine.LastIndexOf('(');
                    var yearPart = cleanLine.Substring(lastOpenParen + 1).TrimEnd(')');
                    if (int.TryParse(yearPart, out var y))
                    {
                        year = y;
                        title = cleanLine.Substring(0, lastOpenParen).Trim();
                    }
                }
                else
                {
                    title = cleanLine;
                }
            }
        }

        // If we found neither ID, we can't reliably deduplicate
        if (string.IsNullOrEmpty(letterboxdId) && string.IsNullOrEmpty(imdbId)) return null;

        return new ProcessedMovie
        {
            // If LetterboxdId is missing, use a placeholder based on ImdbId to satisfy DB requirement
            LetterboxdId = letterboxdId ?? $"seeded-{imdbId}",
            ImdbId = imdbId,
            Title = title ?? "Unknown",
            Year = year,
            TelegramMessageId = msg.ID,
            ProcessedAt = msg.date
        };
    }
}
