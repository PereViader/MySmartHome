using Microsoft.Extensions.Options;

namespace PereViader.MusicCaster.Telegram;

public interface ITelegramService
{
    /// <summary>
    /// Sends a message to the configured Telegram chat.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendMessageAsync(string message);
}

public class TelegramServiceOptions
{
    public required string BotToken { get; init; }
    public required string ChatId { get; init; }
}

public class TelegramService(IOptions<TelegramServiceOptions> telegramServiceOptions) : ITelegramService
{
    public async Task SendMessageAsync(string message)
    {
        var options = telegramServiceOptions.Value;
        using var httpClient = new HttpClient();
        var url = $"https://api.telegram.org/bot{options.BotToken}/sendMessage";
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("chat_id", options.ChatId),
            new KeyValuePair<string, string>("text", message)
        ]);

        await httpClient.PostAsync(url, content);
    }
}