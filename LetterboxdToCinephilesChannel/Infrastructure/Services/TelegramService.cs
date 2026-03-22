using System.Net;
using System.Text;
using LetterboxdToCinephilesChannel.Configuration;
using LetterboxdToCinephilesChannel.Infrastructure.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace LetterboxdToCinephilesChannel.Infrastructure.Services;

public class TelegramService(ITelegramBotClient botClient, IOptions<TelegramOptions> options, ILogger<TelegramService> logger)
{
    private readonly TelegramOptions _options = options.Value;

    public async Task<Message?> SendMovieCardAsync(RssItem rssItem, TmdbMovieDetails? tmdbDetails, CancellationToken ct = default)
    {
        try
        {
            var caption = BuildHtmlCaption(rssItem, tmdbDetails);
            
            var buttons = new List<InlineKeyboardButton>();
            
            var imdbId = !string.IsNullOrEmpty(rssItem.ImdbId) ? rssItem.ImdbId : tmdbDetails?.ImdbId;
            if (!string.IsNullOrEmpty(imdbId))
            {
                buttons.Add(InlineKeyboardButton.WithUrl("IMDb", $"https://www.imdb.com/title/{imdbId}"));
            }
            else if (tmdbDetails != null)
            {
                buttons.Add(InlineKeyboardButton.WithUrl("TMDB", $"https://www.themoviedb.org/movie/{tmdbDetails.Id}"));
            }

            buttons.Add(InlineKeyboardButton.WithUrl("Letterboxd", rssItem.Link));

            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

            if (!string.IsNullOrEmpty(rssItem.PosterUrl))
            {
                logger.LogInformation("Sending photo to Telegram for {Movie}", rssItem.FilmTitle);
                return await botClient.SendPhotoAsync(
                    chatId: _options.ChannelId,
                    photo: InputFile.FromUri(rssItem.PosterUrl),
                    caption: caption,
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct);
            }
            else
            {
                logger.LogInformation("Sending message to Telegram for {Movie}", rssItem.FilmTitle);
                return await botClient.SendTextMessageAsync(
                    chatId: _options.ChannelId,
                    text: caption,
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send Telegram message for {Movie}", rssItem.FilmTitle);
            return null;
        }
    }

    private string BuildHtmlCaption(RssItem rssItem, TmdbMovieDetails? tmdbDetails)
    {
        var sb = new StringBuilder();

        var imdbId = !string.IsNullOrEmpty(rssItem.ImdbId) ? rssItem.ImdbId : tmdbDetails?.ImdbId;
        var movieUrl = !string.IsNullOrEmpty(imdbId) 
            ? $"https://www.imdb.com/title/{imdbId}" 
            : tmdbDetails != null 
                ? $"https://www.themoviedb.org/movie/{tmdbDetails.Id}"
                : rssItem.Link;

        // <b>Title (Year)</b>
        sb.AppendLine($"<a href=\"{movieUrl}\"><b>{WebUtility.HtmlEncode(rssItem.FilmTitle)} ({rssItem.FilmYear})</b></a>");

        // Release Date
        if (!string.IsNullOrEmpty(tmdbDetails?.ReleaseDate))
        {
            sb.AppendLine($"📅 <b>Release Date:</b> {tmdbDetails.ReleaseDate}");
        }

        // Runtime
        if (tmdbDetails?.Runtime > 0)
        {
            sb.AppendLine($"⏱ <b>Runtime:</b> {tmdbDetails.Runtime} min");
        }

        // Ratings & Liked
        var ratings = new List<string>();
        if (!string.IsNullOrEmpty(rssItem.MemberRating))
        {
            ratings.Add($"⭐ {rssItem.MemberRating}/5.0");
        }
        if (tmdbDetails != null)
        {
            ratings.Add($"🎬 TMDB: {tmdbDetails.VoteAverage:F1}/10");
        }
        if (rssItem.Liked == "Yes")
        {
            ratings.Add("❤️ Liked");
        }

        if (ratings.Count > 0)
        {
            sb.AppendLine(string.Join(" | ", ratings));
        }

        sb.AppendLine();

        // Genres (hashtags)
        if (tmdbDetails?.Genres.Count > 0)
        {
            var hashtags = tmdbDetails.Genres.Select(g => $"#{g.Name.Replace(" ", "").Replace("-", "")}");
            sb.AppendLine(string.Join(" ", hashtags));
            sb.AppendLine();
        }

        // Original Language
        if (!string.IsNullOrEmpty(tmdbDetails?.OriginalLanguage))
        {
            sb.AppendLine($"🌐 <b>Language:</b> {tmdbDetails.OriginalLanguage.ToUpper()}");
        }

        // Plot (truncated)
        var plot = tmdbDetails?.Overview ?? string.Empty;
        if (!string.IsNullOrEmpty(plot))
        {
            if (plot.Length > 500)
            {
                plot = plot.Substring(0, 497) + "...";
            }
            sb.AppendLine();
            sb.AppendLine($"<i>{WebUtility.HtmlEncode(plot)}</i>");
        }

        return sb.ToString();
    }
}
