using System.ComponentModel.DataAnnotations;

namespace LetterboxdToCinephilesChannel.Configuration;

public class RssOptions
{
    [Required]
    [Url]
    public string FeedUrl { get; set; } = string.Empty;
}
