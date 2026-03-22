using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using LetterboxdToCinephilesChannel.Configuration;

namespace LetterboxdToCinephilesChannel.Infrastructure.Services;

public class ErrorReportingService(ITelegramBotClient botClient, IOptions<TelegramOptions> options, ILogger<ErrorReportingService> logger)
{
    private readonly TelegramOptions _options = options.Value;

    public async Task ReportErrorAsync(string message, Exception? ex = null, CancellationToken ct = default)
    {
        try
        {
            var fullMessage = $"❌ <b>Critical Error</b>\n\n{message}";
            if (ex != null)
            {
                fullMessage += $"\n\n<b>Exception:</b> {ex.Message}";
                if (ex.InnerException != null)
                {
                    fullMessage += $"\n<b>Inner Exception:</b> {ex.InnerException.Message}";
                }
            }

            await botClient.SendTextMessageAsync(
                chatId: _options.ErrorChatId,
                text: fullMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: ct);
            
            logger.LogInformation("Error reported to Telegram chat {ChatId}", _options.ErrorChatId);
        }
        catch (Exception errorEx)
        {
            logger.LogError(errorEx, "Failed to report error to Telegram chat {ChatId}", _options.ErrorChatId);
        }
    }
}
