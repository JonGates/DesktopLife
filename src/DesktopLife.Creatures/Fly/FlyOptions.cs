namespace DesktopLife.Creatures.Fly;
public sealed record FlyOptions
{
    public float FollowSpeed { get; init; } = 420;
    public float PanicSpeed { get; init; } = 720;
    public float OrbitMinRadius { get; init; } = 40;
    public float OrbitMaxRadius { get; init; } = 120;
    public float IdleDepartSeconds { get; init; } = 1.5f;
    public float PanicMouseSpeed { get; init; } = 900;
}
