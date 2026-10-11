using System.Xml.Linq;
using LetterboxdToCinephilesChannel.Infrastructure.Http;
using Xunit;

namespace LetterboxdToCinephilesChannel.Tests;

public class RssParserTests
{
    private const string SampleRss = @"<?xml version=""1.0"" encoding=""UTF-8""?>
        <rss xmlns:letterboxd=""https://letterboxd.com/"" version=""2.0"">
          <channel>
            <item>
              <title>Fight Club, 1999 - ★★★★½</title>
              <link>https://letterboxd.com/shnkr/film/fight-club/</link>
              <description>&lt;p&gt;&lt;img src=""https://a.ltrbxd.com/resized/film-poster/5/1/5/3/9/51539-fight-club-0-230-0-345-crop.jpg""/&gt;&lt;/p&gt; &lt;p&gt;Watched on Saturday March 21, 2026.&lt;/p&gt;</description>
              <letterboxd:filmTitle>Fight Club</letterboxd:filmTitle>
              <letterboxd:filmYear>1999</letterboxd:filmYear>
              <letterboxd:memberRating>4.5</letterboxd:memberRating>
              <letterboxd:liked>Yes</letterboxd:liked>
            </item>
          </channel>
        </rss>";

    [Fact]
    public void ParseFeed_ShouldTrimWhitespace()
    {
        var rawXml = "   " + SampleRss + "   ";
        var doc = XDocument.Parse(rawXml.Trim());
        Assert.NotNull(doc);
    }

    [Fact]
    public void ParseFeed_ShouldHandleLetterboxdNamespace()
    {
        var doc = XDocument.Parse(SampleRss);
        XNamespace lb = "https://letterboxd.com/";
        var item = doc.Descendants("item").First();
        
        Assert.Equal("Fight Club", item.Element(lb + "filmTitle")?.Value);
    }

    [Fact]
    public void ParseFeed_ShouldExtractLikedStatus()
    {
        var doc = XDocument.Parse(SampleRss);
        XNamespace lb = "https://letterboxd.com/";
        var item = doc.Descendants("item").First();
        
        Assert.Equal("Yes", item.Element(lb + "liked")?.Value);
    }

    [Fact]
    public void UpgradePosterResolution_ShouldReturnHighResUrl()
    {
        var lowResUrl = "https://a.ltrbxd.com/resized/film-poster/5/1/5/3/9/51539-fight-club-0-230-0-345-crop.jpg";
        var highResUrl = RssClient.UpgradePosterResolution(lowResUrl);
        
        Assert.Equal("https://a.ltrbxd.com/resized/film-poster/5/1/5/3/9/51539-fight-club-0-1000-0-1500-crop.jpg", highResUrl);
    }

    [Fact]
    public void ExtractReview_WhenWatchedDateOnly_ShouldReturnEmpty()
    {
        var description = @"<p><img src=""https://a.ltrbxd.com/poster.jpg""/></p> <p>Watched on Saturday March 21, 2026.</p>";
        var review = RssClient.ExtractReview(description);
        
        Assert.Empty(review);
    }

    [Fact]
    public void ExtractReview_WhenReviewPresent_ShouldExtractText()
    {
        var description = @"<p><img src=""https://a.ltrbxd.com/poster.jpg""/></p> <p>Movie should have stopped at the interval</p>";
        var review = RssClient.ExtractReview(description);
        
        Assert.Equal("Movie should have stopped at the interval", review);
    }

    [Fact]
    public void ExtractReview_WhenSpoilerWarningPresent_ShouldIncludeWarning()
    {
        var description = @"<p><img src=""https://a.ltrbxd.com/poster.jpg""/></p> <p><em>This review may contain spoilers.</em></p> <p>Great plot twist!</p>";
        var review = RssClient.ExtractReview(description);
        
        Assert.Equal("⚠️ This review may contain spoilers.\n\nGreat plot twist!", review);
    }

    [Fact]
    public void ExtractReview_WithLineBreaks_ShouldPreserveNewlines()
    {
        var description = @"<p><img src=""https://a.ltrbxd.com/poster.jpg""/></p> <p>🐕 PP<br />🪆Karnan<br />🐷 Maamannan<br />🐐Bison</p>";
        var review = RssClient.ExtractReview(description);
        
        Assert.Equal("🐕 PP\n🪆Karnan\n🐷 Maamannan\n🐐Bison", review);
    }

    [Fact]
    public void ExtractReview_WithMultiParagraphs_ShouldJoinWithDoubleNewlines()
    {
        var description = @"<p><img src=""https://a.ltrbxd.com/poster.jpg""/></p> <p>First paragraph.</p> <p>Second paragraph.</p>";
        var review = RssClient.ExtractReview(description);
        
        Assert.Equal("First paragraph.\n\nSecond paragraph.", review);
    }

    [Fact]
    public void ExtractReview_WhenNullOrWhitespace_ShouldReturnEmpty()
    {
        Assert.Empty(RssClient.ExtractReview(null));
        Assert.Empty(RssClient.ExtractReview("   "));
    }

    [Fact]
    public void ParseFeed_WithRealWorldReviewItem_ExtractsReviewAndUpgradesPoster()
    {
        var itemXml = @"
            <item xmlns:letterboxd=""https://letterboxd.com"" xmlns:tmdb=""https://themoviedb.org"">
              <title>Modha Rathri, 2026 - ★★½</title>
              <link>https://letterboxd.com/waffle1234/film/modha-rathri/</link>
              <guid isPermaLink=""false"">letterboxd-review-1532562935</guid>
              <letterboxd:filmTitle>Modha Rathri</letterboxd:filmTitle>
              <letterboxd:filmYear>2026</letterboxd:filmYear>
              <letterboxd:memberRating>2.5</letterboxd:memberRating>
              <letterboxd:memberLike>No</letterboxd:memberLike>
              <tmdb:movieId>1685882</tmdb:movieId>
              <description><![CDATA[ <p><img src=""https://a.ltrbxd.com/resized/film-poster/1/5/4/9/1/6/1/1549161-modha-rathri-0-600-0-900-crop.jpg?v=960b109662""/></p> <p>Movie should have stopped at the interval</p> ]]></description>
            </item>";

        var element = XElement.Parse(itemXml);
        var description = element.Element("description")?.Value ?? string.Empty;
        var review = RssClient.ExtractReview(description);

        Assert.Equal("Movie should have stopped at the interval", review);
    }
}
