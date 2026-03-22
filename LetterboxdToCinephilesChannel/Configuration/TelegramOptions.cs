using System.ComponentModel.DataAnnotations;

namespace LetterboxdToCinephilesChannel.Configuration;

public class TelegramOptions
{
    [Required]
    public string BotToken { get; set; } = string.Empty;

    [Required]
    public string ChannelId { get; set; } = string.Empty;

    public string? ErrorChatId { get; set; }
}
