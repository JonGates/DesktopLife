using System.Numerics;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
namespace DesktopLife.Creatures.Tests;

public class FlyLandingTests
{
    private static readonly WorldBounds Bounds = new(0, 0, 1920, 1080);
    private static readonly Vector2 Cursor = new(900, 500);
    private readonly RandomSource _random = new(12);
    private float _time;
    private void Step(FlyCreature fly, Vector2 mouse, MouseClick? click = null, float elapsed = 0.02f, DesktopLayout? layout = null)
    {
        _time += MathF.Min(elapsed, 0.05f);
        fly.Update(MathF.Min(elapsed, 0.05f), new(new(mouse, Vector2.Zero, 0, false, TimeSpan.FromSeconds(30), click),
            layout?.Bounds ?? Bounds, _time, _random, Layout: layout, ElapsedSeconds: elapsed));
    }
    private void Land(FlyCreature fly, Vector2 point, long sequence = 1)
    {
        Step(fly, point, new(sequence, point));
        for (var i = 0; i < 1000 && fly.State != FlyState.Landed; i++) Step(fly, Cursor);
        Assert.Equal(FlyState.Landed, fly.State);
        Assert.Equal(point, fly.Position);
    }
    [Fact]
    public void StationaryCursorSummonsFlyAndKeepsItOrbiting()
    {
        var fly = new FlyCreature(new(-1, 500));
        for (var i = 0; i < 1500; i++) Step(fly, Cursor);
        Assert.Equal(FlyState.Orbit, fly.State);
        Assert.InRange(Vector2.Distance(Cursor, fly.Position), 25, 150);
        var before = fly.Position;
        for (var i = 0; i < 20; i++) Step(fly, Cursor);
        Assert.True(Vector2.Distance(before, fly.Position) > 10);
    }
    [Fact]
    public void ClickFliesToFixedPointThenRestsForThreeSecondsFromArrival()
    {
        var point = new Vector2(300, 300);
        var fly = new FlyCreature(new(1800, 900));
        Step(fly, point, new(1, point));
        Assert.Equal(FlyState.Landing, fly.State);
        Assert.InRange(Vector2.Distance(fly.Position, new(1800, 900)), 0.01, 12);
        for (var i = 0; i < 150; i++) Step(fly, new(1100, 600));
        Assert.Equal(FlyState.Landing, fly.State); // Travel takes more than three seconds.
        for (var i = 0; i < 1000 && fly.State != FlyState.Landed; i++) Step(fly, new(1100, 600));
        Assert.Equal(FlyState.Landed, fly.State);
        Assert.Equal(point, fly.Position);
        for (var i = 0; i < 149; i++)
        {
            Step(fly, new(1500, 700));
            Assert.Equal(FlyState.Landed, fly.State);
            Assert.Equal(point, fly.Position);
            Assert.Equal(Vector2.Zero, fly.Velocity);
        }
        Step(fly, new(1500, 700));
        Assert.NotEqual(FlyState.Landed, fly.State);
        for (var i = 0; i < 600; i++) Step(fly, new(1500, 700));
        Assert.Equal(FlyState.Orbit, fly.State);
    }
    [Fact]
    public void NewClickRetargetsWhileFlyingAndWhileResting()
    {
        var fly = new FlyCreature(new(1700, 700));
        Step(fly, Cursor, new(1, new(100, 100)));
        var final = new Vector2(1200, 500);
        Land(fly, final, 2);
        var newer = new Vector2(400, 600);
        Step(fly, Cursor, new(3, newer));
        Assert.Equal(FlyState.Landing, fly.State);
        for (var i = 0; i < 1000 && fly.State != FlyState.Landed; i++) Step(fly, Cursor);
        Assert.Equal(newer, fly.Position);
    }
    [Fact]
    public void RepeatedDeliveryOfSameEventDoesNotRestartRestTimer()
    {
        var fly = new FlyCreature(Cursor);
        Land(fly, Cursor);
        for (var i = 0; i < 149; i++) Step(fly, Cursor, new(1, Cursor));
        Assert.Equal(FlyState.Landed, fly.State);
        Step(fly, Cursor, new(1, Cursor));
        Assert.NotEqual(FlyState.Landed, fly.State);
    }
    [Fact]
    public void NewClickOnSamePointRestartsThreeSecondStay()
    {
        var fly = new FlyCreature(Cursor);
        Land(fly, Cursor);
        Step(fly, Cursor, elapsed: 2);
        Step(fly, Cursor, new(2, Cursor));
        Step(fly, Cursor, elapsed: 2.9f);
        Assert.Equal(FlyState.Landed, fly.State);
        Step(fly, Cursor, elapsed: 0.11f);
        Assert.NotEqual(FlyState.Landed, fly.State);
    }
    [Fact]
    public void RealElapsedTimeExpiresLandingAfterLongFrame()
    {
        var fly = new FlyCreature(Cursor);
        Land(fly, Cursor);
        Step(fly, Cursor, elapsed: 2.99f);
        Assert.Equal(FlyState.Landed, fly.State);
        Step(fly, Cursor, elapsed: 0.02f);
        Assert.NotEqual(FlyState.Landed, fly.State);
    }
    [Fact]
    public void RemovedLandingDisplayCancelsTheTarget()
    {
        var right = new DesktopLayout([new("a", Bounds), new("b", new(1920, 0, 1920, 1080))]);
        var fly = new FlyCreature(Cursor);
        Step(fly, Cursor, new(1, new(2400, 500)), layout: right);
        Assert.Equal(FlyState.Landing, fly.State);
        Step(fly, Cursor, layout: new([new("a", Bounds)]));
        Assert.NotEqual(FlyState.Landing, fly.State);
        Assert.NotEqual(FlyState.Landed, fly.State);
    }
    [Fact]
    public void ClickInGapIsIgnored()
    {
        var layout = new DesktopLayout([new("a", new(0, 0, 1000, 800)), new("b", new(1100, 0, 1000, 800))]);
        var fly = new FlyCreature(new(500, 400));
        Step(fly, new(500, 400), new(1, new(1050, 400)), layout: layout);
        Assert.NotEqual(FlyState.Landing, fly.State);
    }
}
