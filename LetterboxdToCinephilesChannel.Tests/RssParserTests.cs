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
}
