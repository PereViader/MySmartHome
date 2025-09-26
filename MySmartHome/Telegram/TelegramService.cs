using Telegram.Bot;
using Telegram.Bot.Types;

namespace MySmartHome.Telegram;

public interface ITelegramService
{
    Task SendMessageAsync(string botToken, string chatId, string? replyToMessageId, string message);
}

public class TelegramService : ITelegramService
{
    public async Task SendMessageAsync(string botToken, string chatId, string? replyToMessageId, string message)
    {
        var client = new TelegramBotClient(botToken);

        ReplyParameters? replyParams = replyToMessageId is null
            ? null
            : int.Parse(replyToMessageId);
        
        await client.SendMessage(chatId:chatId, text: message, replyParameters: replyParams);
    }
}