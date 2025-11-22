using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace MySmartHome.Music;

public static class MusicApplicationBuilderExtensions
{
    public static void UseMusic(this WebApplication app)
    {
        var variables = app.Services.GetRequiredService<IOptions<MusicServiceOptions>>();
        app.UseStaticFiles(new StaticFileOptions()
        {
            RequestPath = "/media",
            FileProvider = new PhysicalFileProvider(Path.GetFullPath(variables.Value.MusicPath)),
            ServeUnknownFileTypes = true,
            DefaultContentType = "application/octet-stream"
        });
    }
}