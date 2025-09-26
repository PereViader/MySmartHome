using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MySmartHome.Authentication;
using MySmartHome.Event;
using MySmartHome.Music;
using MySmartHome.Telegram;
using MySmartHome.WaterReminders;
using TickerQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Configuration.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true);

builder.Services
    .AddOpenApi()
    .AddTickerQ()
    .AddHostedService<MusicService>()
    .AddSingleton<ITelegramService, TelegramService>()
    .AddSingleton<HomeTelegramService>()
    .AddSingleton<WaterTelegramService>()
    .AddSingleton<IEventService, EventService>()
    .AddScoped<ApiKeyEndpointFilter>()
    .Configure<EventOptions>(builder.Configuration.GetSection(nameof(EventOptions)))
    .Configure<MusicServiceOptions>(builder.Configuration.GetSection(nameof(MusicServiceOptions)))
    .Configure<ApiKeyEndpointFilterOptions>(builder.Configuration.GetSection(nameof(ApiKeyEndpointFilterOptions)))
    .Configure<HomeTelegramServiceOptions>(builder.Configuration.GetSection(nameof(HomeTelegramServiceOptions)))
    .Configure<WaterTelegramServiceOptions>(builder.Configuration.GetSection(nameof(WaterTelegramServiceOptions)))
    .Configure<WaterReminderJobOptions>(builder.Configuration.GetSection(nameof(WaterReminderJobOptions)));

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
var variables = app.Services.GetRequiredService<IOptions<MusicServiceOptions>>();
app.UseStaticFiles(new StaticFileOptions()
{
    RequestPath = "/media",
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(variables.Value.MusicPath)),
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.UseTickerQ();

app.MapEventEndpoints();

app.Run();