using System.Numerics;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
namespace DesktopLife.Creatures.Tests;
public class FlyFlightRhythmTests
{
    [Fact]
    public void NearbyFlightContainsDartsAndHoversInsteadOfConstantOrbit()
    {
        var fly = new FlyCreature(new(1020, 540));
        var world = new SimulationWorld(new(0, 0, 1920, 1080), new RandomSource(42), [fly]);
        var fast = 0; var slow = 0; var clockwise = 0; var counterclockwise = 0;
        var previous = fly.Position - new Vector2(960, 540);
        for (var i = 0; i < 1800; i++)
        {
            world.Update(1f / 60, new(960, 540));
            var offset = fly.Position - new Vector2(960, 540);
            if (i > 120)
            {
                if (fly.Velocity.Length() > 150) fast++;
                if (fly.Velocity.Length() < 20) slow++;
                var cross = previous.X * offset.Y - previous.Y * offset.X;
                if (cross > 3) clockwise++;
                if (cross < -3) counterclockwise++;
                Assert.InRange(offset.Length(), 0, 180);
            }
            previous = offset;
        }
        Assert.True(fast > 100 && slow > 100, $"Expected both darts and hovers: fast={fast}, slow={slow}");
        Assert.True(clockwise > 100 && counterclockwise > 100, "Flight should change direction, not circle one way");
    }
    [Fact]
    public void NearbyFlightStaysOnVisibleDesktopAtCorners()
    {
        var fly = new FlyCreature(new(15, 15));
        var world = new SimulationWorld(new(0, 0, 1920, 1080), new RandomSource(31), [fly]);
        for (var i = 0; i < 1800; i++)
        {
            world.Update(1f / 60, new(3, 3));
            Assert.True(world.Bounds.ContainsScreenPoint(fly.Position));
        }
    }
    [Fact]
    public void PanicReturnsFromOutsideWithoutTeleportingOrStalling()
    {
        var fly = new FlyCreature(new(15, 200));
        var bounds = new WorldBounds(0, 0, 800, 600);
        var random = new RandomSource(7);
        var outside = false;
        for (var i = 0; i < 400; i++)
        {
            var before = fly.Position;
            fly.Update(1f / 60, new(new(new(40, 200), Vector2.Zero, i == 1 ? 1600 : 0, i == 1, TimeSpan.Zero), bounds, i / 60f, random));
            if (!bounds.ContainsScreenPoint(fly.Position)) outside = true;
            Assert.InRange(Vector2.Distance(before, fly.Position), 0, 12.1f);
            Assert.InRange(fly.Velocity.Length(), 0, 721);
        }
        Assert.True(outside);
        Assert.True(bounds.ContainsScreenPoint(fly.Position));
        Assert.Equal(FlyState.Orbit, fly.State);
    }
    [Fact]
    public void FollowsAcrossDisplayGapWithoutPinningOrTeleporting()
    {
        var simulation = new DesktopLife.Creatures.Displays.DisplaySimulation(7);
        simulation.Synchronize([new("left", new(0, 0, 400, 400), true), new("right", new(420, 0, 400, 400))]);
        simulation.SetPopulation(new(0, 0, 0));
        var fly = (FlyCreature)simulation.World.Manager.Creatures.Single();
        fly.Relocate(new(390, 200));
        for (var i = 0; i < 300; i++)
        {
            var before = fly.Position;
            simulation.Update(1f / 60, new(440, 200));
            Assert.InRange(Vector2.Distance(before, fly.Position), 0, 12.1f);
        }
        Assert.True(fly.Position.X >= 420);
    }}
