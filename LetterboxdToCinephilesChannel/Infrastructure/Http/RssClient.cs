using System.Text.RegularExpressions;
using System.Xml.Linq;
using LetterboxdToCinephilesChannel.Configuration;
using Microsoft.Extensions.Options;

namespace LetterboxdToCinephilesChannel.Infrastructure.Http;

public class RssClient
{
    private static readonly XNamespace LbNamespace = "https://letterboxd.com/";
    private readonly HttpClient _httpClient;
    private readonly RssOptions _options;

    public RssClient(HttpClient httpClient, IOptions<RssOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<List<RssItem>> GetFeedAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(_options.FeedUrl, ct);
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync(ct);
        var trimmedContent = rawContent.Trim();
        var doc = XDocument.Parse(trimmedContent);

        return doc.Descendants("item")
            .Select(x =>
            {
                var description = x.Element("description")?.Value ?? string.Empty;
                var posterUrl = ExtractPosterUrl(description);
                var highResPosterUrl = UpgradePosterResolution(posterUrl);

                return new RssItem(
                    x.Element("title")?.Value ?? string.Empty,
                    x.Element("link")?.Value ?? string.Empty,
                    x.Element("{http://purl.org/dc/elements/1.1/}creator")?.Value ?? string.Empty,
                    x.Element("pubDate")?.Value ?? string.Empty,
                    description,
                    x.Element(LbNamespace + "watchedDate")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "rewatch")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "filmTitle")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "filmYear")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "memberRating")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "imdbId")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "tmdbId")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "liked")?.Value ?? "No",
                    highResPosterUrl,
                    x.Element("guid")?.Value ?? string.Empty
                );
            })
            .ToList();
    }

    private static string ExtractPosterUrl(string description)
    {
        var match = Regex.Match(description, @"<img\s+src=""([^""]+)""");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    public static string UpgradePosterResolution(string? url)
    {
        if (string.IsNullOrEmpty(url)) return string.Empty;
        // Replace small dimensions with high resolution (1000x1500)
        return url.Replace("0-230-0-345", "0-1000-0-1500");
    }
}

public record RssItem(
    string Title,
    string Link,
    string Creator,
    string PubDate,
    string Description,
    string WatchedDate,
    string Rewatch,
    string FilmTitle,
    string FilmYear,
    string MemberRating,
    string ImdbId,
    string TmdbId,
    string Liked,
    string PosterUrl,
    string Guid
);
