# TMDB API v3 Research: Movie Search & Detail Fetching

**Project:** LetterboxdToTelegram
**Researched:** 2025-03-22
**Overall Confidence:** HIGH

## 1. Best Endpoint for "Exact Title + Year" Lookup

To find a specific movie by title and its primary release year, the `/search/movie` endpoint is the standard choice.

### **Endpoint**
`GET https://api.themoviedb.org/3/search/movie`

### **Recommended Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `query` | string | Yes | The movie title (URL encoded). |
| `primary_release_year` | integer | No | Filters results to match the primary release date (usually the first theatrical release). |
| `year` | integer | No | Matches *any* release date in that year. `primary_release_year` is stricter. |
| `language` | string | No | Language for titles/overviews (e.g., `en-US`). |

### **Example Request**
```bash
curl "https://api.themoviedb.org/3/search/movie?query=The%20Batman&primary_release_year=2022" \
     -H "Authorization: Bearer YOUR_TOKEN"
```

## 2. Data Structures

### Genre names
*   **Search Response (`/search/movie`):** Returns `genre_ids` (an array of integers). To get names, you must either:
    1.  Fetch the full movie details via `/movie/{id}`.
    2.  Cache the global genre list from `/genre/movie/list` and map the IDs manually.
*   **Detail Response (`/movie/{id}`):** Returns a `genres` array of objects:
    ```json
    "genres": [
      { "id": 28, "name": "Action" },
      { "id": 12, "name": "Adventure" }
    ]
    ```

### Original Language
*   Returned as a string representing the **ISO 639-1** code (e.g., `"en"`, `"fr"`, `"ja"`).
*   Field: `original_language`

### High-Resolution Poster Path
*   The API returns a partial path: `"poster_path": "/8Ul9S9S9S9S9S9S9S9S9S9S9S9S.jpg"`
*   To get a high-resolution image, prepend the base URL with the `original` size:
    *   **Full URL:** `https://image.tmdb.org/t/p/original/8Ul9S9S9S9S9S9S9S9S9S9S9S9S.jpg`
*   **Recommended Sizes:**
    *   `original` (highest quality)
    *   `w780` (high quality, better for mobile/slow networks)
    *   `w500` (standard quality)

## 3. Handling TMDB Bearer Token in .NET 10

TMDB uses the **API Read Access Token** for Bearer authentication. This is preferred over the `api_key` query parameter for security.

### **Using Typed HttpClient (.NET 10)**
The most modern approach is using **Typed Clients** with `IHttpClientFactory`.

**Registration (`Program.cs`):**
```csharp
builder.Services.AddHttpClient<ITmdbClient, TmdbClient>(client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", tmdbOptions.ReadAccessToken);
});
```

**Implementation:**
```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class TmdbClient(HttpClient httpClient) : ITmdbClient
{
    public async Task<TmdbMovie?> GetMovieByTitleAndYear(string title, int year)
    {
        var encodedTitle = Uri.EscapeDataString(title);
        var url = $"search/movie?query={encodedTitle}&primary_release_year={year}";
        
        var response = await httpClient.GetFromJsonAsync<TmdbSearchResponse>(url);
        return response?.Results.FirstOrDefault();
    }

    public async Task<TmdbMovieDetails?> GetMovieDetails(int movieId)
    {
        return await httpClient.GetFromJsonAsync<TmdbMovieDetails>($"movie/{movieId}");
    }
}
```

### **Manual Configuration (Simple/CLI)**
If not using dependency injection:
```csharp
using HttpClient client = new();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", "YOUR_READ_ACCESS_TOKEN");

var response = await client.GetAsync("https://api.themoviedb.org/3/movie/11");
```

## 4. Key Considerations & Pitfalls

*   **Rate Limiting:** TMDB no longer has a strict "40 requests per 10 seconds" limit but suggests reasonable usage. They return `429 Too Many Requests` if exceeded.
*   **Missing Poster:** Always check if `poster_path` is null before constructing the image URL.
*   **Multiple Results:** `/search/movie` returns a list. If "Exact Title + Year" yields multiple results (rare but possible), prioritize the first one or check popularity.
*   **Language:** Ensure the `language` parameter is consistent if you want non-English data.

## Sources
- [TMDB API Documentation - Search](https://developer.themoviedb.org/reference/search-movie)
- [TMDB API Documentation - Movie Details](https://developer.themoviedb.org/reference/movie-details)
- [TMDB API Documentation - Images](https://developer.themoviedb.org/docs/image-basics)
- [Microsoft Docs - HttpClient authentication](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient)
