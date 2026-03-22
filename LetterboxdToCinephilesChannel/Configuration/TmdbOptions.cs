using System.ComponentModel.DataAnnotations;

namespace LetterboxdToCinephilesChannel.Configuration;

public class TmdbOptions
{
    [Required]
    public string ReadAccessToken { get; set; } = string.Empty;
}
