# Research: OMDB API & .NET Integration

**Date:** 2024-10-18
**Confidence:** HIGH

## OMDB API Overview

The [OMDb API](http://www.omdbapi.com/) is a RESTful web service to obtain movie information.

### Usage Limits
| Tier | Daily Limit | Cost |
|------|-------------|------|
| Free | 1,000 requests | $0 |
| Patron | 100,000 requests | $1/mo |
| Patron | 250,000 requests | $5/mo |
| Patron | Unlimited | $10/mo |

**Notes:**
- Limits are per API key.
- Excessive requests may result in temporary or permanent IP bans.

### Data Structures

#### 1. Search Result (`s=`)
Used for finding multiple matches.
```json
{
  "Search": [
    {
      "Title": "Batman Begins",
      "Year": "2005",
      "imdbID": "tt0372784",
      "Type": "movie",
      "Poster": "https://..."
    }
  ],
  "totalResults": "1",
  "Response": "True"
}
```

#### 2. Detailed Metadata (`t=` or `i=`)
Used for retrieving full movie details.
```json
{
  "Title": "Batman Begins",
  "Year": "2005",
  "Rated": "PG-13",
  "Released": "15 Jun 2005",
  "Runtime": "140 min",
  "Genre": "Action, Crime, Drama",
  "Director": "Christopher Nolan",
  "Writer": "Bob Kane, David S. Goyer",
  "Actors": "Christian Bale, Michael Caine, Liam Neeson",
  "Plot": "After training with his mentor...",
  "Language": "English, Urdu, Mandarin",
  "Country": "UK, USA",
  "Awards": "Nominated for 1 Oscar. 13 wins & 79 nominations total",
  "Poster": "https://...",
  "Ratings": [
    { "Source": "Internet Movie Database", "Value": "8.2/10" },
    { "Source": "Rotten Tomatoes", "Value": "85%" },
    { "Source": "Metacritic", "Value": "70/100" }
  ],
  "Metascore": "70",
  "imdbRating": "8.2",
  "imdbVotes": "1,556,197",
  "imdbID": "tt0372784",
  "Type": "movie",
  "DVD": "18 Oct 2005",
  "BoxOffice": "$206,852,432",
  "Production": "N/A",
  "Website": "N/A",
  "Response": "True"
}
```

## .NET Integration Best Practices

### 1. IHttpClientFactory
**Problem:** `new HttpClient()` causes socket exhaustion. `static HttpClient` causes stale DNS.
**Solution:** Use `IHttpClientFactory` via Dependency Injection.

### 2. Typed Clients
Encapsulate API logic in a dedicated service.
```csharp
public class OmdbClient
{
    private readonly HttpClient _httpClient;
    public OmdbClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<MovieMetadata> GetMovieAsync(string title, string year)
    {
        return await _httpClient.GetFromJsonAsync<MovieMetadata>($"?t={title}&y={year}&plot=full");
    }
}
```

### 3. Resilience with Polly
Handle transient errors (429, 503, network blips) gracefully.
```csharp
services.AddHttpClient<OmdbClient>(client => {
    client.BaseAddress = new Uri("http://www.omdbapi.com/");
})
.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));
```

### 4. System.Text.Json
Prefer `System.Text.Json` over `Newtonsoft.Json` for better performance and lower memory footprint in .NET 8.

## Implementation Recommendations

1. **Move API Key to Configuration:** Use `IOptions<OmdbOptions>` or `IConfiguration` instead of direct `Environment.GetEnvironmentVariable`.
2. **Use Query Helpers:** Use `QueryHelpers.AddQueryString` or similar to build URLs safely.
3. **Map Nulls Early:** Use C# 12 features like primary constructors and default values in DTOs to handle "N/A" results from OMDB.

## Sources
- [OMDB API Documentation](http://www.omdbapi.com/)
- [Microsoft: IHttpClientFactory in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [Polly Documentation](https://github.com/App-vNext/Polly)
