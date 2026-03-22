using System.ComponentModel.DataAnnotations;

namespace LetterboxdToCinephilesChannel.Configuration;

public class TelegramOptions
{
    [Required]
    public string BotToken { get; set; } = string.Empty;

    [Required]
    public string ChannelId { get; set; } = string.Empty;

    [Required]
    public string ErrorChatId { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int ApiId { get; set; }

    [Required]
    public string ApiHash { get; set; } = string.Empty;
}
