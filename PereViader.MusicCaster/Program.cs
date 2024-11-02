using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PereViader.MusicCaster;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddOpenApi()
    .AddHostedService<MusicService>()
    .AddSingleton<UrlService>();

builder.Services.Configure<Configuration>(
    builder.Configuration.GetSection(Configuration.Position));

var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Services.GetRequiredService<UrlService>().Url = app.Urls.First();
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
var variables = app.Services.GetRequiredService<IOptions<Configuration>>();

//list files in folder
foreach (var file in Directory.GetFiles(variables.Value.AssetsPath))
{
    Console.WriteLine(file);
}
Console.WriteLine("Assets Path: " + variables.Value.AssetsPath);
app.UseStaticFiles(new StaticFileOptions()
{
    RequestPath = "/media",
    FileProvider = new PhysicalFileProvider(variables.Value.AssetsPath),
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});
app.UseHttpsRedirection();
app.Run();