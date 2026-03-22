using System.Text.Json;
using LetterboxdToCinephilesChannel.Infrastructure.Http;
using Xunit;

namespace LetterboxdToCinephilesChannel.Tests;

public class TmdbMappingTests
{
    [Fact]
    public void TmdbSearchResponse_ShouldDeserializeCorrectly()
    {
        var json = @"{
            ""page"": 1,
            ""results"": [
                {
                    ""id"": 550,
                    ""title"": ""Fight Club"",
                    ""release_date"": ""1999-10-15"",
                    ""genre_ids"": [18],
                    ""original_language"": ""en"",
                    ""vote_average"": 8.433
                }
            ],
            ""total_pages"": 1,
            ""total_results"": 1
        }";

        var response = JsonSerializer.Deserialize<TmdbSearchResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(response);
        Assert.Single(response.Results);
        Assert.Equal(550, response.Results[0].Id);
        Assert.Equal("Fight Club", response.Results[0].Title);
    }

    [Fact]
    public void TmdbMovieDetails_ShouldDeserializeCorrectly()
    {
        var json = @"{
            ""id"": 550,
            ""genres"": [
                { ""id"": 18, ""name"": ""Drama"" }
            ],
            ""original_language"": ""en"",
            ""overview"": ""A ticking-time-bomb insomniac and a slippery soap salesman channel primal male aggression into a shocking new form of therapy. Their concept catches on, with 'fight clubs' forming in every town, until an eccentric gets in the way and ignites an out-of-control spiral toward oblivion."",
            ""vote_average"": 8.433
        }";

        var response = JsonSerializer.Deserialize<TmdbMovieDetails>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(response);
        Assert.Single(response.Genres);
        Assert.Equal("Drama", response.Genres[0].Name);
        Assert.Equal("en", response.OriginalLanguage);
    }
}
