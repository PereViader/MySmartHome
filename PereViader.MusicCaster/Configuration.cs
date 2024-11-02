public class Configuration
{
    public const string Position = "Music";

    public required string AssetsPath { get; init; }
    public required TimeOnly StartTime { get; init; }
    public required TimeOnly EndTime { get; init; }
    public required TimeSpan MinimumTimeBetweenPlaybacks { get; init; }
    public required TimeSpan MaximumTimeBetweenPlaybacks { get; init; }
}