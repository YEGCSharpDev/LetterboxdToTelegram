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
        if (_options.ApiId == 0 || string.IsNullOrEmpty(_options.ApiHash))
        {
            _logger.LogWarning("Telegram ApiId or ApiHash not configured. Skipping history seeding.");
            return;
        }

        _logger.LogInformation("Syncing history from Telegram channel...");

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
                // Use Messages_GetChats which is allowed for bots to get specific chats by ID
                var chats = await client.Messages_GetChats(id);
                resolved = chats.chats.Values.FirstOrDefault(c => c.ID == id);
            }
            else
            {
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
            int totalNew = 0;
            int consecutiveExisting = 0;
            var batchLetterboxdIds = new HashSet<string>();

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
                        if (movie != null && !batchLetterboxdIds.Contains(movie.LetterboxdId))
                        {
                            var exists = await _dbContext.ProcessedMovies.AnyAsync(m => m.LetterboxdId == movie.LetterboxdId, ct);
                            if (!exists)
                            {
                                _dbContext.ProcessedMovies.Add(movie);
                                totalNew++;
                                consecutiveExisting = 0;
                            }
                            else
                            {
                                consecutiveExisting++;
                            }
                            batchLetterboxdIds.Add(movie.LetterboxdId);
                        }
                    }
                    offsetId = msgBase.ID;
                }

                // If we've seen 50 existing movies in a row, we're likely caught up.
                if (consecutiveExisting > 50 || channelMsgs.messages.Length < 100)
                    break;
                
                await Task.Delay(1000, ct); // Rate limiting
            }

            if (totalNew > 0)
            {
                await _dbContext.SaveChangesAsync(ct);
                _logger.LogInformation("Successfully synced {Count} new movies from history.", totalNew);
            }
            else
            {
                _logger.LogInformation("History is already in sync.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during history syncing");
        }
    }

    private ProcessedMovie? ParseMessage(Message msg)
    {
        var text = msg.message ?? string.Empty;
        var urls = new List<string> { text };

        // Extract URLs from entities (like HTML links in the first line)
        if (msg.entities != null)
        {
            foreach (var entity in msg.entities)
            {
                if (entity is MessageEntityTextUrl textUrl)
                {
                    urls.Add(textUrl.url);
                }
                else if (entity is MessageEntityUrl urlEntity)
                {
                    // For plain text URLs, we need to extract them from the text
                    if (urlEntity.offset + urlEntity.length <= text.Length)
                    {
                        urls.Add(text.Substring(urlEntity.offset, urlEntity.length));
                    }
                }
            }
        }

        // Extract URLs from inline buttons (where Letterboxd link usually resides)
        if (msg.reply_markup is ReplyInlineMarkup inlineMarkup)
        {
            foreach (var row in inlineMarkup.rows)
            {
                foreach (var button in row.buttons)
                {
                    if (button is KeyboardButtonUrl buttonUrl)
                    {
                        urls.Add(buttonUrl.url);
                    }
                }
            }
        }

        string? letterboxdId = null;
        string? imdbId = null;
        string? title = null;
        int? year = null;

        foreach (var url in urls)
        {
            if (string.IsNullOrEmpty(letterboxdId))
            {
                var lbMatch = Regex.Match(url, @"letterboxd\.com/film/([^/\s\n]+)");
                if (lbMatch.Success)
                {
                    letterboxdId = lbMatch.Groups[1].Value.Trim().TrimEnd('/');
                }
            }

            if (string.IsNullOrEmpty(imdbId))
            {
                var imdbMatch = Regex.Match(url, @"(tt\d{7,})");
                if (imdbMatch.Success)
                {
                    imdbId = imdbMatch.Groups[1].Value.Trim();
                }
            }
        }

        // Extract Title and Year (usually first line)
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0)
        {
            var firstLine = lines[0].Replace("🎬", "").Replace("🎬", "").Trim();
            if (firstLine.Contains("(") && firstLine.EndsWith(")"))
            {
                var lastOpenParen = firstLine.LastIndexOf('(');
                var yearPart = firstLine.Substring(lastOpenParen + 1).TrimEnd(')');
                if (int.TryParse(yearPart, out var y))
                {
                    year = y;
                    title = firstLine.Substring(0, lastOpenParen).Trim();
                }
            }
            else
            {
                title = firstLine;
            }
        }

        // If we found neither ID, we can't reliably deduplicate
        if (string.IsNullOrEmpty(letterboxdId) && string.IsNullOrEmpty(imdbId)) return null;

        return new ProcessedMovie
        {
            LetterboxdId = letterboxdId ?? $"seeded-{imdbId}",
            ImdbId = imdbId,
            Title = title ?? "Unknown",
            Year = year,
            TelegramMessageId = msg.ID,
            ProcessedAt = msg.date
        };
    }
}
