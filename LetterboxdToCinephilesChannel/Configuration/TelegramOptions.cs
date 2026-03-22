using System.ComponentModel.DataAnnotations;

namespace LetterboxdToCinephilesChannel.Configuration;

public class TelegramOptions
{
    [Required]
    public string BotToken { get; set; } = string.Empty;

    [Required]
    public string ChannelId { get; set; } = string.Empty;

    public string? ErrorChatId { get; set; }

    [Range(0, int.MaxValue)]
    public int ApiId { get; set; }

    public string? ApiHash { get; set; }
}
