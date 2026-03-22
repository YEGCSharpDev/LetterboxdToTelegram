using System.ComponentModel.DataAnnotations;

namespace LetterboxdToCinephilesChannel.Configuration;

public class OmdbOptions
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "http://www.omdbapi.com/";
}
