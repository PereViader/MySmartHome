using Microsoft.Extensions.Options;
using MySmartHome.Authentication;
using MySmartHome.Telegram;

namespace MySmartHome.Event;

public static class EventEndpointMapper
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        app.MapGet("event/{eventName}", async (string eventName, IEventService eventService) =>
        {
            await eventService.HandleEvent(eventName);
            return Results.Ok();
        })
        .AddEndpointFilter<ApiKeyEndpointFilter>();
    }
}

public class EventOptions
{
    public required Dictionary<string, string> Events { get; init; } = new();
}

public interface IEventService
{
    /// <summary>
    /// Handles the event identified with an ID.
    /// </summary>
    /// <param name="eventName">The name of the event to handle.</param>
    Task HandleEvent(string eventName);
}

public class EventService(IOptions<EventOptions> eventOptions, HomeTelegramService homeTelegramService) : IEventService
{
    public async Task HandleEvent(string eventName)
    {
        if (!eventOptions.Value.Events.TryGetValue(eventName, out var message))
        {
            throw new ArgumentException($"Event '{eventName}' is not configured.");
        }
        
        await homeTelegramService.SendMessageAsync(message);
    }
}