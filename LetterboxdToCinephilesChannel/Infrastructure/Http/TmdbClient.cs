using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LetterboxdToCinephilesChannel.Infrastructure.Http;

public class TmdbClient(HttpClient httpClient)
{
    public async Task<TmdbMovie?> GetMovieByTitleAndYearAsync(string title, int year, CancellationToken ct = default)
    {
        var encodedTitle = Uri.EscapeDataString(title);
        var url = $"search/movie?query={encodedTitle}&primary_release_year={year}";
        
        var response = await httpClient.GetFromJsonAsync<TmdbSearchResponse>(url, ct);
        return response?.Results.FirstOrDefault();
    }

    public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int movieId, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<TmdbMovieDetails>($"movie/{movieId}", ct);
    }
}

public class TmdbSearchResponse
{
    [JsonPropertyName("results")]
    public List<TmdbMovie> Results { get; set; } = [];
}

public class TmdbMovie
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;
    
    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; } = string.Empty;
    
    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }
}

public class TmdbMovieDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("genres")]
    public List<TmdbGenre> Genres { get; set; } = [];
    
    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; } = string.Empty;
    
    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;
    
    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }
}

public class TmdbGenre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
