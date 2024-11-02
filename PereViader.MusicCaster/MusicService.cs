using System.Net;
using Microsoft.Extensions.Options;
using Sharpcaster;
using Sharpcaster.Interfaces;
using Sharpcaster.Models.Media;

namespace PereViader.MusicCaster;

public class MusicService : BackgroundService
{
    private readonly Configuration _configuration;
    private readonly ILogger<MusicService> _logger;
    private readonly UrlService _urlService;
    
    public MusicService(IOptions<Configuration> configuration, ILogger<MusicService> logger, UrlService urlService)
    {
        _configuration = configuration.Value;
        _logger = logger;
        _urlService = urlService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                await PlaySong();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while playing song");
            }

            var timeToWait = CalculateTimeToWait();
            _logger.LogInformation("Waiting for {0:hh\\:mm\\:ss}", timeToWait);
            await Task.Delay(timeToWait, stoppingToken);
        }
    }

    private async Task PlaySong()
    {
        var random = new Random();
        var files = Directory.GetFiles(_configuration.AssetsPath, "*.mp3", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            throw new FileNotFoundException("No mp3 file found");
        }
        var index = random.Next(0, files.Length);
        var file = files[index];
        
        IChromecastLocator locator = new MdnsChromecastLocator();
        using var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(3000));
        var chromecasts = (await locator.FindReceiversAsync(source.Token))?.ToArray();
        if (chromecasts is null)
        {
            throw new FileNotFoundException("No chromecasts found");
        }
        
        foreach (var i in chromecasts)
        {
            Console.WriteLine(i.Name);
        }
        var chromecast = chromecasts.FirstOrDefault(x => x.Name.Equals(_configuration.GroupName));
        if (chromecast is null)
        {
            throw new FileNotFoundException($"Chromecast with name: {_configuration.GroupName} not found");
        }
        
        var client = new ChromecastClient();
        await client.ConnectChromecast(chromecast);
        _ = await client.LaunchApplicationAsync("0B2D9915");

        var media = new Media
        {
            ContentUrl = $"{_urlService.Url}/media/{Uri.EscapeDataString(Path.GetRelativePath(_configuration.AssetsPath, file))}"
        };
        _logger.LogInformation($"Playing: {media.ContentUrl}");
        _ = await client.MediaChannel.LoadAsync(media);
    }

    private string GetIp()
    {
        string ipAddress = "";
        if (Dns.GetHostAddresses(Dns.GetHostName()).Length > 0)
        {
            ipAddress = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString();
        }
        return ipAddress;
    }
    
    public TimeSpan CalculateTimeToWait()
    {
        var current = TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay);

        var maxDelay = _configuration.MaximumTimeBetweenPlaybacks - _configuration.MinimumTimeBetweenPlaybacks;
        var randomWeight = new Random().NextDouble();
        var delay = maxDelay * randomWeight;
        
        if (current < _configuration.StartTime)
        {
            return _configuration.StartTime - current + delay;
        }
        
        if (current > _configuration.EndTime)
        {
            return _configuration.StartTime.ToTimeSpan().Add(TimeSpan.FromDays(1)) - current.ToTimeSpan() + delay;
        }
        
        var nextTime = current.ToTimeSpan() + delay;
        if (nextTime > _configuration.EndTime.ToTimeSpan())
        {
            return nextTime - _configuration.EndTime.ToTimeSpan() + _configuration.StartTime.ToTimeSpan().Add(TimeSpan.FromDays(1));
        }

        return delay;
    }
}