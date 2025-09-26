using Microsoft.Extensions.Options;

namespace PereViader.MusicCaster.Telegram;

public class WaterTelegramServiceOptions
{
    public required string BotToken { get; init; }
    public required string ChatId { get; init; }
    public required string TopicId { get; init; }
}

public class WaterTelegramService(ITelegramService telegramService, IOptions<WaterTelegramServiceOptions> options)
{
    public Task SendMessageAsync(string message)
    {
        var serviceOptions = options.Value;
        return telegramService.SendMessageAsync(serviceOptions.BotToken, serviceOptions.ChatId, replyToMessageId: serviceOptions.TopicId, message);
    }
}