namespace DesktopLife.Creatures.Cockroach;

public sealed record CockroachOptions
{
    public float CrawlSpeedMin { get; init; } = 45;
    public float CrawlSpeedMax { get; init; } = 95;
    public float FleeSpeedMin { get; init; } = 220;
    public float FleeSpeedMax { get; init; } = 420;
    public float FearRadius { get; init; } = 160;
    public float SeparationRadius { get; init; } = 24;
}
