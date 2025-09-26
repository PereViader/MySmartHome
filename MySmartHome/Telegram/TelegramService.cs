namespace MySmartHome.Telegram;

public interface ITelegramService
{
    Task SendMessageAsync(string botToken, string chatId, string? replyToMessageId, string message);
}

public class TelegramService: ITelegramService
{
    public async Task SendMessageAsync(string botToken, string chatId, string? replyToMessageId, string message)
    {
        using var httpClient = new HttpClient();
        var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("chat_id", chatId),
            new KeyValuePair<string, string>("text", message)
        ]);

        await httpClient.PostAsync(url, content);
    }
}