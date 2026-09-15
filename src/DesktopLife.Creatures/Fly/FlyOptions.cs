namespace DesktopLife.Creatures.Fly;
public sealed record FlyOptions
{
    public float FollowSpeed { get; init; } = 420;
    public float PanicSpeed { get; init; } = 720;
    public float OrbitMinRadius { get; init; } = 40;
    public float OrbitMaxRadius { get; init; } = 120;
    public float LandedSeconds { get; init; } = 3;
    public float PanicMouseSpeed { get; init; } = 900;
}
