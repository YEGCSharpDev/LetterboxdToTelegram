using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;
using LetterboxdToCinephilesChannel.Configuration;
using Microsoft.Extensions.Options;

namespace LetterboxdToCinephilesChannel.Infrastructure.Http;

public class RssClient
{
    private static readonly XNamespace LbNamespace = "https://letterboxd.com";
    private static readonly XNamespace TmdbNamespace = "https://themoviedb.org";
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
                var review = ExtractReview(description);

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
                    x.Element(TmdbNamespace + "movieId")?.Value ?? string.Empty,
                    x.Element(LbNamespace + "memberLike")?.Value ?? "No",
                    highResPosterUrl,
                    x.Element("guid")?.Value ?? string.Empty,
                    review
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
        return url.Replace("0-230-0-345", "0-1000-0-1500")
                  .Replace("0-600-0-900", "0-1000-0-1500")
                  .Replace("0-150-0-225", "0-1000-0-1500");
    }

    public static string ExtractReview(string? description)
    {
        if (string.IsNullOrWhiteSpace(description)) return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(description);

        var pNodes = doc.DocumentNode.SelectNodes("//p");
        if (pNodes == null || pNodes.Count == 0) return string.Empty;

        var reviewParagraphs = new List<string>();

        foreach (var p in pNodes)
        {
            // Skip poster paragraph (contains <img>)
            if (p.SelectSingleNode(".//img") != null) continue;

            // Replace <br> tags with newlines
            var brNodes = p.SelectNodes(".//br");
            if (brNodes != null)
            {
                foreach (var br in brNodes)
                {
                    br.ParentNode.ReplaceChild(doc.CreateTextNode("\n"), br);
                }
            }

            var text = WebUtility.HtmlDecode(p.InnerText).Trim();
            if (string.IsNullOrEmpty(text)) continue;

            // Skip "Watched on ..." watch date paragraphs
            if (text.StartsWith("Watched on ", StringComparison.OrdinalIgnoreCase)) continue;

            if (text.Equals("This review may contain spoilers.", StringComparison.OrdinalIgnoreCase))
            {
                reviewParagraphs.Add("⚠️ " + text);
                continue;
            }

            reviewParagraphs.Add(text);
        }

        return string.Join("\n\n", reviewParagraphs).Trim();
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
    string Guid,
    string Review = ""
);
