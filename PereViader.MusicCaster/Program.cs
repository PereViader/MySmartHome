using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PereViader.MusicCaster;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddOpenApi()
    .AddHostedService<MusicService>();

builder.Configuration
    .AddJsonFile("appsettings.Configuration.json")
    .AddEnvironmentVariables();

builder.Services.Configure<Configuration>(
    builder.Configuration.GetSection(Configuration.Position));

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
var variables = app.Services.GetRequiredService<IOptions<Configuration>>();
app.UseStaticFiles(new StaticFileOptions()
{
    RequestPath = "/media",
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(variables.Value.MusicPath)),
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});
app.Run();