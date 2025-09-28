using TickerQ.Utilities.Enums;
using TickerQ.Utilities.Interfaces;

namespace MySmartHome;

public sealed class TickerExceptionHandler : ITickerExceptionHandler
{
    public Task HandleExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
    {
        Console.Write(exception.ToString());
        return Task.CompletedTask;
    }

    public Task HandleCanceledExceptionAsync(Exception exception, Guid tickerId, TickerType tickerType)
    {
        Console.Write(exception.ToString());
        return Task.CompletedTask;
    }
}