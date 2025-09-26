using Microsoft.Extensions.Options;
using PereViader.MusicCaster.Telegram;
using TickerQ.Utilities.Base;

namespace PereViader.MusicCaster.WaterReminders;

public class WaterReminderJobOptions
{
    public required string[] Messages { get; init; }
}

public class WaterReminderJob(WaterTelegramService waterTelegramService, IOptions<WaterReminderJobOptions> options)
{
    [TickerFunction("WaterReminderJob", "0 9-21 * * *")] //Every hour between 9 am and 21 pm
    public Task Execute()
    {
        var message = options.Value.Messages[Random.Shared.Next(0, options.Value.Messages.Length)];
        
        return waterTelegramService.SendMessageAsync(message);
    }
}