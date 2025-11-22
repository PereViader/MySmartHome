using Microsoft.Extensions.Options;
using Sharpcaster;
using Sharpcaster.Interfaces;
using Sharpcaster.Models.Media;

namespace MySmartHome.Music;

public class MusicServiceOptions
{
    public required string MusicPath { get; init; }
    public required string CastDeviceName { get; init; }
    public required TimeOnly StartTime { get; init; }
    public required TimeOnly EndTime { get; init; }
    public required TimeSpan MinimumTimeBetweenPlaybacks { get; init; }
    public required TimeSpan MaximumTimeBetweenPlaybacks { get; init; }
}

public class MusicService(IOptions<MusicServiceOptions> musicServiceOptions, ILogger<MusicService> logger)
    : BackgroundService
{
    private readonly MusicServiceOptions _musicServiceOptions = musicServiceOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (IsInAllowedTime())
            {
                try
                {
                    await PlaySong();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while playing song");
                }
            }

            var timeToWait = CalculateTimeToWait();
            logger.LogInformation("Waiting for {0:hh\\:mm\\:ss}", timeToWait);
            await Task.Delay(timeToWait, stoppingToken);
        }
    }

    private async Task PlaySong()
    {
        var random = new Random();
        var files = Directory.GetFiles(_musicServiceOptions.MusicPath, "*.mp3", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            throw new FileNotFoundException("No mp3 file found");
        }
        var index = random.Next(0, files.Length);
        var file = files[index];
        
        IChromecastLocator locator = new MdnsChromecastLocator();
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var chromecasts = (await locator.FindReceiversAsync(source.Token))?.ToArray();
        if (chromecasts is null)
        {
            throw new InvalidOperationException("No chromecasts found");
        }
        
        var anyDoingAnything = chromecasts.Any(x => !string.IsNullOrEmpty(x.Status) && x.Status != "AmbientCaster");
        if (anyDoingAnything)
        {
            throw new InvalidOperationException("There is some chromecast that is doing something");
        }
        
        var chromecast = chromecasts.FirstOrDefault(x => x.Name.Equals(_musicServiceOptions.CastDeviceName));
        if (chromecast is null)
        {
            throw new InvalidOperationException($"Chromecast with name: {_musicServiceOptions.CastDeviceName} not found");
        }
        
        var client = new ChromecastClient();
        
        await client.ConnectChromecast(chromecast);
        _ = await client.LaunchApplicationAsync("0B2D9915");

        var media = new Media
        {
            ContentUrl = $"{UrlService.GetLocalUrl()}/media/{Uri.EscapeDataString(Path.GetRelativePath(_musicServiceOptions.MusicPath, file))}"
        };
        logger.LogInformation($"Playing: {media.ContentUrl}");
        _ = await client.MediaChannel.LoadAsync(media);
    }

    public bool IsInAllowedTime()
    {
        var current = TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay);
        return current >= _musicServiceOptions.StartTime && current <= _musicServiceOptions.EndTime;
    }
    
    public TimeSpan CalculateTimeToWait()
    {
        var current = TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay);

        var maxDelay = _musicServiceOptions.MaximumTimeBetweenPlaybacks - _musicServiceOptions.MinimumTimeBetweenPlaybacks;
        var randomWeight = new Random().NextDouble();
        var delay = maxDelay * randomWeight;
        
        if (current < _musicServiceOptions.StartTime)
        {
            return _musicServiceOptions.StartTime - current + delay;
        }
        
        if (current > _musicServiceOptions.EndTime)
        {
            return _musicServiceOptions.StartTime.ToTimeSpan().Add(TimeSpan.FromDays(1)) - current.ToTimeSpan() + delay;
        }
        
        var nextTime = current.ToTimeSpan() + delay;
        if (nextTime > _musicServiceOptions.EndTime.ToTimeSpan())
        {
            return nextTime - _musicServiceOptions.EndTime.ToTimeSpan() + _musicServiceOptions.StartTime.ToTimeSpan().Add(TimeSpan.FromDays(1));
        }

        return delay;
    }
}