using Microsoft.Extensions.Options;

namespace MySmartHome.Telegram;

public class HomeTelegramServiceOptions
{
    public required string BotToken { get; init; }
    public required string ChatId { get; init; }
}

public class HomeTelegramService(ITelegramService telegramService, IOptions<HomeTelegramServiceOptions> options)
{
    public Task SendMessageAsync(string message)
    {
        var serviceOptions = options.Value;
        return telegramService.SendMessageAsync(serviceOptions.BotToken, serviceOptions.ChatId, replyToMessageId: null, message);
    }
}