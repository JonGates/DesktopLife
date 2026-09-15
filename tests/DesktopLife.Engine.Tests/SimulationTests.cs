using System.Numerics;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.Time;
using DesktopLife.Engine.World;

namespace DesktopLife.Engine.Tests;

public class SimulationTests
{
    [Fact]
    public void ZeroVectorNormalizesWithoutNan()
    {
        Assert.Equal(Vector2.Zero, VectorMath.NormalizeOrZero(Vector2.Zero));
        Assert.Equal(new Vector2(0.6f, 0.8f), VectorMath.NormalizeOrZero(new(3, 4)));
    }

    [Fact]
    public void BoundsSupportNegativeScreenOrigin()
    {
        var bounds = new WorldBounds(-1920, 100, 1920, 1080);
        Assert.True(bounds.Contains(new(-500, 500)));
        Assert.False(bounds.Contains(new(5, 500)));
        Assert.Equal(new Vector2(-960, 640), bounds.Center);
        Assert.Equal(new Vector2(-1920, 1180), bounds.Clamp(new(-2000, 2000)));
    }

    [Theory]
    [InlineData(96, 1f)]
    [InlineData(120, 1.25f)]
    [InlineData(144, 1.5f)]
    public void PhysicalToLocalCoordinatesUseOriginAndDpi(float dpi, float scale)
    {
        var local = ScreenCoordinates.ToLocal(new(-1620, 400), new(-1920, 100), dpi / 96, dpi / 96);
        Assert.Equal(300 / scale, local.X, 3);
        Assert.Equal(300 / scale, local.Y, 3);
    }

    [Fact]
    public void SeededRandomIsReproducibleAndBounded()
    {
        var a = new RandomSource(42);
        var b = new RandomSource(42);
        for (var i = 0; i < 100; i++)
        {
            var value = a.NextFloat(-2, 7);
            Assert.Equal(value, b.NextFloat(-2, 7));
            Assert.InRange(value, -2, 7);
        }
    }

    [Fact]
    public void LoopCapsSimulationStepAndResetAvoidsPauseJump()
    {
        var loop = new GameLoop();
        Assert.Equal(0, loop.Tick(10).DeltaTime);
        var time = loop.Tick(11);
        Assert.Equal(0.05f, time.DeltaTime);
        Assert.Equal(1, time.ElapsedSeconds);
        loop.Reset();
        Assert.Equal(0, loop.Tick(90).DeltaTime);
    }

    [Fact]
    public void WorldUpdatesEachCreatureOnceAndKeepsRealIdleTime()
    {
        var creature = new MovingCreature();
        var world = new SimulationWorld(new(0, 0, 1920, 1080), new RandomSource(1), [creature]);
        world.Update(0.01f, new(100, 100));
        world.Update(2, new(100, 100));
        Assert.Equal(2, creature.Updates);
        Assert.Equal(6, creature.Position.X, 3);
        Assert.Equal(2, world.Mouse.State.IdleTime.TotalSeconds, 3);
    }

    [Fact]
    public void ManagerCollectionCannotBeMutatedByRenderers()
    {
        var manager = new CreatureManager([new MovingCreature()]);
        var collection = Assert.IsAssignableFrom<IList<ICreature>>(manager.Creatures);
        Assert.Throws<NotSupportedException>(() => collection.Clear());
    }

    private sealed class MovingCreature : Creature
    {
        public int Updates { get; private set; }
        public override void Update(float deltaTime, in CreatureContext context)
        {
            Updates++;
            Position += new Vector2(100 * deltaTime, 0);
        }
    }
}
