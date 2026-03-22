using System.ComponentModel.DataAnnotations;

namespace LetterboxdToCinephilesChannel.Domain.Entities;

public class ProcessedMovie
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string LetterboxdId { get; set; } = string.Empty;

    public string? ImdbId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public int? Year { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public long? TelegramMessageId { get; set; }
}
