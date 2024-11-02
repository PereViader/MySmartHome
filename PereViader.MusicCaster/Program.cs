using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddOpenApi()
    .AddHostedService<MusicService>();

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
    FileProvider = new PhysicalFileProvider(variables.Value.AssetsPath),
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});
app.UseHttpsRedirection();
app.Run();