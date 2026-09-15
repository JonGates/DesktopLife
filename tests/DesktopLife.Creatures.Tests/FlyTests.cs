using System.Numerics;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class FlyTests
{
    private static readonly WorldBounds Bounds = new(0, 0, 1920, 1080);
    private static readonly Vector2 Cursor = new(960, 540);
    private readonly IRandomSource _random = new RandomSource(42);
    private float _time;

    private void Step(FlyCreature fly, bool moving = true, float speed = 100, float idle = 0, Vector2? cursor = null, float dt = 1f / 60)
    {
        _time += dt;
        var mouse = new MouseState(cursor ?? Cursor, moving ? new(3, 0) : Vector2.Zero, speed, moving, TimeSpan.FromSeconds(idle));
        var context = new CreatureContext(mouse, Bounds, _time, _random);
        fly.Update(dt, in context);
    }

    [Fact]
    public void MouseActivityBringsFlyBackWithoutTeleporting()
    {
        var fly = new FlyCreature(new(-90, 540));
        var start = fly.Position;
        Step(fly);
        Assert.Equal(FlyState.Approach, fly.State);
        Assert.True(fly.IsVisible);
        Assert.InRange(Vector2.Distance(start, fly.Position), 0.001f, 12);
        Assert.True(fly.Position.X > start.X);
    }

    [Fact]
    public void FlyApproachesThenOrbitsAwayFromCursorCenter()
    {
        var fly = new FlyCreature(new(-90, 540));
        for (var i = 0; i < 600; i++) Step(fly);
        Assert.Equal(FlyState.Orbit, fly.State);
        Assert.InRange(Vector2.Distance(Cursor, fly.Position), 25, 150);
        var before = fly.Position;
        for (var i = 0; i < 20; i++) Step(fly);
        Assert.True(Vector2.Distance(before, fly.Position) > 10);
    }

    [Fact]
    public void CursorOutsideDesktopCausesDepartureAndFlyReallyLeavesBounds()
    {
        var fly = new FlyCreature(Cursor + new Vector2(100, 0));
        Step(fly);
        Step(fly, moving: false, speed: 0, idle: 1.6f, cursor: new(-500, -500));
        Assert.Equal(FlyState.Depart, fly.State);
        for (var i = 0; i < 600; i++) Step(fly, moving: false, speed: 0, idle: 2 + i / 60f, cursor: new(-500, -500));
        Assert.Equal(FlyState.Offscreen, fly.State);
        Assert.False(fly.IsVisible);
        Assert.False(Bounds.Contains(fly.Position));
    }

    [Fact]
    public void ActivityInterruptsDepartureAndReturnsFromOffscreen()
    {
        var fly = new FlyCreature(Cursor + new Vector2(100, 0));
        Step(fly);
        Step(fly, moving: false, idle: 2, cursor: new(-500, -500));
        Step(fly);
        Assert.Equal(FlyState.Approach, fly.State);
        for (var i = 0; i < 600; i++) Step(fly, moving: false, speed: 0, idle: 3, cursor: new(-500, -500));
        var before = fly.Position;
        Step(fly);
        Assert.Equal(FlyState.Approach, fly.State);
        Assert.InRange(Vector2.Distance(before, fly.Position), 0.001f, 12);
    }

    [Fact]
    public void FastNearbyMouseTriggersEscapeThenRecovery()
    {
        var fly = new FlyCreature(Cursor + new Vector2(100, 0));
        Step(fly);
        Step(fly);
        var before = Vector2.Distance(Cursor, fly.Position);
        Step(fly, speed: 1600);
        Assert.Equal(FlyState.Panic, fly.State);
        Assert.True(Vector2.Distance(Cursor, fly.Position) > before);
        for (var i = 0; i < 120; i++) Step(fly);
        Assert.NotEqual(FlyState.Panic, fly.State);
    }

    [Fact]
    public void StationaryMouseAlsoSummonsFly()
    {
        var fly = new FlyCreature(new(-90, 540));
        for (var i = 0; i < 60; i++) Step(fly, moving: false, speed: 0, idle: 4);
        Assert.Equal(FlyState.Approach, fly.State);
        Assert.True(fly.IsVisible);
        Assert.True(fly.Position.X > -90);
    }

    [Fact]
    public void MouseOnAnotherMonitorDoesNotSummonFly()
    {
        var fly = new FlyCreature(new(-90, 540));
        Step(fly, cursor: new(2300, 400));
        Assert.Equal(FlyState.Offscreen, fly.State);
    }

    [Fact]
    public void SeededSimulationIsRepeatableAndStableOverTenMinutes()
    {
        var a = new FlyCreature(new(-90, 540));
        var b = new FlyCreature(new(-90, 540));
        var worldA = new SimulationWorld(Bounds, new RandomSource(19), [a]);
        var worldB = new SimulationWorld(Bounds, new RandomSource(19), [b]);
        for (var i = 0; i < 36000; i++)
        {
            var t = i / 60f;
            var p = i % 600 < 300 ? Cursor + new Vector2(MathF.Sin(t) * 300, MathF.Cos(t) * 200) : Cursor;
            worldA.Update(1f / 60, p);
            worldB.Update(1f / 60, p);
            Assert.True(float.IsFinite(a.Position.X) && float.IsFinite(a.Position.Y));
            Assert.InRange(a.Velocity.Length(), 0, 721);
            Assert.True(Bounds.Contains(a.Position, 250));
            Assert.Equal(a.Position, b.Position);
            Assert.Equal(a.State, b.State);
        }
    }
}
