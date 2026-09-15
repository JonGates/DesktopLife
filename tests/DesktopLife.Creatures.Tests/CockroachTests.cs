using System.Numerics;
using DesktopLife.Creatures.Cockroach;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class CockroachTests
{
    private static readonly WorldBounds Bounds = new(0, 0, 1920, 1080);
    private static readonly Vector2 SafeMouse = new(100, 100);
    private readonly IRandomSource _random = new RandomSource(42);
    private float _time;

    private void Step(CockroachCreature roach, Vector2? mouse = null, float speed = 0, float idle = 2, float dt = 1f / 60, IReadOnlyList<ICreature>? neighbors = null)
    {
        _time += dt;
        var input = new MouseState(mouse ?? SafeMouse, Vector2.Zero, speed, speed > 15, TimeSpan.FromSeconds(idle));
        roach.Update(dt, new(input, Bounds, _time, _random, neighbors));
    }

    [Fact]
    public void UnthreatenedRoachCrawlsAtWalkingSpeed()
    {
        var roach = new CockroachCreature(new(1000, 600));
        for (var i = 0; i < 60; i++) Step(roach);
        Assert.Equal(CockroachState.Crawl, roach.State);
        Assert.InRange(roach.Velocity.Length(), 35, 96);
        Assert.True(Vector2.Distance(roach.Position, new(1000, 600)) > 20);
    }

    [Fact]
    public void NearbyMouseImmediatelyScaresRoachAndItFleesAway()
    {
        var roach = new CockroachCreature(new(960, 540));
        var mouse = new Vector2(900, 540);
        Step(roach, mouse);
        Assert.Equal(CockroachState.Panic, roach.State);
        Assert.True(roach.Position.X > 960);
        for (var i = 0; i < 12; i++) Step(roach, mouse);
        Assert.Equal(CockroachState.Flee, roach.State);
        Assert.True(Vector2.Dot(roach.Velocity, roach.Position - mouse) > 0);
        Assert.True(Vector2.Distance(roach.Position, mouse) > 100);
    }

    [Fact]
    public void FastMouseExpandsFearRadius()
    {
        var calm = new CockroachCreature(new(960, 540));
        var scared = new CockroachCreature(new(960, 540));
        Step(calm, new(760, 540));
        Step(scared, new(760, 540), speed: 1800);
        Assert.Equal(CockroachState.Crawl, calm.State);
        Assert.Equal(CockroachState.Panic, scared.State);
    }

    [Fact]
    public void MouseOnOtherMonitorDoesNotScareRoach()
    {
        var roach = new CockroachCreature(new(1900, 600));
        Step(roach, new(1950, 600), speed: 2000);
        Assert.Equal(CockroachState.Crawl, roach.State);
    }

    [Fact]
    public void FleeingRoachHidesBeyondEdgeThenReturnsWhenSafe()
    {
        var roach = new CockroachCreature(new(8, 500));
        for (var i = 0; i < 60; i++) Step(roach, new(80, 500));
        Assert.Equal(CockroachState.Hidden, roach.State);
        Assert.False(roach.IsVisible);
        Assert.False(Bounds.Contains(roach.Position));
        for (var i = 0; i < 600; i++) Step(roach, new(1600, 900), idle: 10);
        Assert.True(roach.IsVisible);
        Assert.Equal(CockroachState.Crawl, roach.State);
        Assert.True(Bounds.Contains(roach.Position));
    }

    [Fact]
    public void HiddenRoachEmergesGraduallyWithoutTeleportingVisibleBody()
    {
        var roach = new CockroachCreature(new(-12, 500), initiallyHidden: true);
        var sawEmerge = false;
        for (var i = 0; i < 600; i++)
        {
            var before = roach.Position;
            Step(roach, new(1600, 900));
            if (roach.State == CockroachState.Emerge)
            {
                sawEmerge = true;
                Assert.InRange(Vector2.Distance(before, roach.Position), 0, 5);
            }
        }
        Assert.True(sawEmerge);
        Assert.Equal(CockroachState.Crawl, roach.State);
    }

    [Fact]
    public void HiddenRoachWaitsWhileMouseGuardsItsExit()
    {
        var roach = new CockroachCreature(new(-12, 500), initiallyHidden: true);
        for (var i = 0; i < 600; i++) Step(roach, new(50, 500), idle: 10);
        Assert.False(roach.IsVisible);
        Assert.Equal(CockroachState.Hidden, roach.State);
    }

    [Fact]
    public void RoachEventuallyCalmsDownAfterThreatLeaves()
    {
        var roach = new CockroachCreature(new(960, 540));
        Step(roach, new(900, 540));
        for (var i = 0; i < 180; i++) Step(roach);
        Assert.Equal(CockroachState.Crawl, roach.State);
        Assert.InRange(roach.Velocity.Length(), 0, 96);
    }

    [Fact]
    public void NearbyNeighborPushesMotionAwayFromCrowding()
    {
        var crowded = new CockroachCreature(new(1000, 600));
        var alone = new CockroachCreature(new(1000, 600));
        var neighbor = new CockroachCreature(new(1003, 600));
        var mouse = new MouseState(SafeMouse, Vector2.Zero, 0, false, TimeSpan.FromSeconds(2));
        var randomA = new RandomSource(7);
        var randomB = new RandomSource(7);
        crowded.Update(0.05f, new(mouse, Bounds, 0.05f, randomA, [crowded, neighbor]));
        alone.Update(0.05f, new(mouse, Bounds, 0.05f, randomB, [alone]));
        Assert.True(crowded.Velocity.X < alone.Velocity.X - 5);
    }

    [Fact]
    public void FullyOverlappingRoachesSeparateWithoutNan()
    {
        var a = new CockroachCreature(new(1000, 600));
        var b = new CockroachCreature(new(1000, 600));
        var world = new SimulationWorld(Bounds, new RandomSource(7), [a, b]);
        for (var i = 0; i < 120; i++) world.Update(1f / 60, SafeMouse);
        Assert.True(float.IsFinite(a.Position.X) && float.IsFinite(b.Position.X));
        Assert.True(Vector2.Distance(a.Position, b.Position) > 18);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void InvalidDtDoesNotMoveOrChangeState(float dt)
    {
        var roach = new CockroachCreature(new(960, 540));
        Step(roach, new(900, 540), dt: dt);
        Assert.Equal(new Vector2(960, 540), roach.Position);
        Assert.Equal(CockroachState.Crawl, roach.State);
    }

    [Fact]
    public void ShrinkingMonitorDoesNotLeaveEmergingRoachStrandedOutside()
    {
        var roach = new CockroachCreature(new(1932, 500), initiallyHidden: true);
        var world = new SimulationWorld(Bounds, new RandomSource(42), [roach]);
        for (var i = 0; i < 600 && roach.State != CockroachState.Emerge; i++) world.Update(1f / 60, SafeMouse);
        Assert.Equal(CockroachState.Emerge, roach.State);
        world.Bounds = new(0, 0, 1280, 720);
        for (var i = 0; i < 600; i++) world.Update(1f / 60, SafeMouse);
        Assert.True(world.Bounds.Contains(roach.Position, 30));
        Assert.True(roach.IsVisible);
    }
}
