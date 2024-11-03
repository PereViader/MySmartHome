public class Configuration
{
    public const string Position = "Music";

    public required string MusicPath { get; init; }
    public required string CastDeviceName { get; init; }
    public required TimeOnly StartTime { get; init; }
    public required TimeOnly EndTime { get; init; }
    public required TimeSpan MinimumTimeBetweenPlaybacks { get; init; }
    public required TimeSpan MaximumTimeBetweenPlaybacks { get; init; }
}