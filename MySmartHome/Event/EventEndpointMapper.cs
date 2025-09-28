using Microsoft.Extensions.Options;
using MySmartHome.Authentication;
using MySmartHome.Telegram;

namespace MySmartHome.Event;

public class EventOptions
{
    public required Dictionary<string, string> Events { get; init; } = new();
}

public static class EventEndpointMapper
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        app.MapGet("event/{eventName}", async (string eventName, IOptions<EventOptions> eventOptions, HomeTelegramService homeTelegramService) =>
        {
            if (!eventOptions.Value.Events.TryGetValue(eventName, out var message))
            {
                throw new ArgumentException($"Event '{eventName}' is not configured.");
            }
        
            await homeTelegramService.SendMessageAsync(message);
            return Results.Ok();
        })
        .AddEndpointFilter<ApiKeyEndpointFilter>();
    }
}
