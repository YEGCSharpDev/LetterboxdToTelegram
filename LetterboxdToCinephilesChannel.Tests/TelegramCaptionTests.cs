using LetterboxdToCinephilesChannel.Configuration;
using LetterboxdToCinephilesChannel.Infrastructure.Http;
using LetterboxdToCinephilesChannel.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LetterboxdToCinephilesChannel.Tests;

public class TelegramCaptionTests
{
    private readonly TelegramService _sut;

    public TelegramCaptionTests()
    {
        var options = Options.Create(new TelegramOptions
        {
            BotToken = "test-token",
            ChannelId = "test-channel"
        });
        _sut = new TelegramService(null!, options, NullLogger<TelegramService>.Instance);
    }

    [Fact]
    public void BuildHtmlCaption_WithoutReview_DoesNotIncludeReviewSection()
    {
        var item = new RssItem(
            "Fight Club, 1999", "https://letterboxd.com/film/fight-club/", "user",
            "date", "desc", "watched", "rewatch", "Fight Club", "1999",
            "4.5", "tt0137523", "550", "Yes", "poster", "guid-1", ""
        );

        var tmdb = new TmdbMovieDetails
        {
            Id = 550,
            Overview = "An insomniac office worker...",
            ReleaseDate = "1999-10-15",
            Runtime = 139,
            VoteAverage = 8.4,
            OriginalLanguage = "en",
            Genres = new List<TmdbGenre> { new TmdbGenre { Id = 18, Name = "Drama" } }
        };

        var caption = _sut.BuildHtmlCaption(item, tmdb);

        Assert.DoesNotContain("💬 <b>Review:</b>", caption);
        Assert.Contains("<i>An insomniac office worker...</i>", caption);
    }

    [Fact]
    public void BuildHtmlCaption_WithReview_IncludesReviewSection()
    {
        var item = new RssItem(
            "Modha Rathri, 2026", "https://letterboxd.com/film/modha-rathri/", "user",
            "date", "desc", "watched", "rewatch", "Modha Rathri", "2026",
            "2.5", "", "1685882", "No", "poster", "guid-2", "Movie should have stopped at the interval"
        );

        var tmdb = new TmdbMovieDetails
        {
            Id = 1685882,
            Overview = "A thrilling Malayalam drama.",
            ReleaseDate = "2026-10-10",
            Runtime = 120,
            VoteAverage = 7.0,
            OriginalLanguage = "ml",
            Genres = new List<TmdbGenre> { new TmdbGenre { Id = 18, Name = "Drama" } }
        };

        var caption = _sut.BuildHtmlCaption(item, tmdb);

        Assert.Contains("💬 <b>Review:</b>", caption);
        Assert.Contains("<i>Movie should have stopped at the interval</i>", caption);
        Assert.Contains("<i>A thrilling Malayalam drama.</i>", caption);
    }

    [Fact]
    public void TruncateHtmlEncoded_SafelyTruncatesAndEncodes()
    {
        var text = "Review with <tag> & text that gets cut";
        var truncated = TelegramService.TruncateHtmlEncoded(text, 30);

        Assert.True(truncated.Length <= 30);
        Assert.EndsWith("...", truncated);
        Assert.DoesNotContain("<tag>", truncated);
        Assert.Contains("&lt;tag&gt;", truncated);
    }
}
