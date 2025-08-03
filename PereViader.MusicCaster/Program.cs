using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PereViader.MusicCaster.Authentication;
using PereViader.MusicCaster.Event;
using PereViader.MusicCaster.Music;
using PereViader.MusicCaster.Telegram;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Configuration.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true);

builder.Services
    .AddOpenApi()
    .AddHostedService<MusicService>()
    .AddSingleton<ITelegramService, TelegramService>()
    .AddSingleton<IEventService, EventService>()
    .AddScoped<ApiKeyEndpointFilter>()
    .Configure<EventOptions>(builder.Configuration.GetSection(nameof(EventOptions)))
    .Configure<MusicServiceOptions>(builder.Configuration.GetSection(nameof(MusicServiceOptions)))
    .Configure<ApiKeyEndpointFilterOptions>(builder.Configuration.GetSection(nameof(ApiKeyEndpointFilterOptions)))
    .Configure<TelegramServiceOptions>(builder.Configuration.GetSection(nameof(TelegramServiceOptions)));

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

app.MapEventEndpoints();

app.Run();