using System.Numerics;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class PauseResumeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RebasingMouseAfterPauseDoesNotInventMovement(bool visible)
    {
        var fly = new FlyCreature(new(800, 500));
        var world = new SimulationWorld(new(0, 0, 1920, 1080), new RandomSource(1), [fly]);
        world.Update(1f / 60, new(100, 100));
        if (visible) world.Update(1f / 60, new(105, 100));
        var before = fly.Position;
        world.Mouse.Reset();
        // The cursor moved during pause and is now stationary near the fly.
        world.Update(1f / 60, new(700, 500));
        Assert.Equal(0, world.Mouse.State.Speed);
        Assert.False(world.Mouse.State.IsMoving);
        Assert.NotEqual(FlyState.Panic, fly.State);
        Assert.InRange(Vector2.Distance(before, fly.Position), 0, 8);
    }
}
