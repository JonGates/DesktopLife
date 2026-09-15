using System.Numerics;
using DesktopLife.Creatures.Displays;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
namespace DesktopLife.Creatures.Tests;

public class NaturalMotionTests
{
    [Fact]
    public void AntPausesToExploreButAThreatInterruptsThePause()
    {
        var ant = new CrawlingInsect(new(500, 500), CreatureKind.Ant);
        var context = new CreatureContext(new(new(900, 900), Vector2.Zero, 0, false, TimeSpan.Zero), new(0, 0, 1000, 1000), 0, new MinimumRandom());
        var paused = false;
        for (var i = 0; i < 150; i++)
        {
            var phase = ant.AnimationPhase;
            ant.Update(0.02f, context);
            if (i > 0 && ant.Velocity == Vector2.Zero)
            {
                Assert.Equal(phase, ant.AnimationPhase);
                paused = true; break;
            }
        }
        Assert.True(paused);
        var threat = new CreatureContext(new(ant.Position + new Vector2(20, 0), Vector2.Zero, 0, false, TimeSpan.Zero), context.Bounds, 0, new MinimumRandom());
        ant.Update(0.02f, threat);
        Assert.NotEqual(Vector2.Zero, ant.Velocity);
    }
    private sealed class MinimumRandom : IRandomSource
    {
        public float NextFloat(float min, float max) => min;
    }

    [Fact]
    public void FlyWingPhaseAdvancesAt53FramesPerSecond()
    {
        var fly = new FlyCreature(new(300, 300));
        var context = new CreatureContext(new(new(400, 300), Vector2.Zero, 0, false, TimeSpan.Zero), new(0, 0, 1000, 1000), 0, new RandomSource(1));
        fly.Update(0.02f, context);
        Assert.InRange(fly.AnimationPhase, 0.1324f, 0.1326f);
    }

    [Fact]
    public void AntTurnsTowardEscapeWithoutInstantlyReversing()
    {
        var ant = new CrawlingInsect(new(300, 300), CreatureKind.Ant);
        var context = new CreatureContext(new(new(320, 300), Vector2.Zero, 0, false, TimeSpan.Zero),
            new WorldBounds(0, 0, 1000, 1000), 0, new RandomSource(1));
        ant.Update(0.02f, context);
        Assert.InRange(MathF.Abs(ant.Rotation), 0.001f, 0.2f);
        for (var i = 0; i < 30; i++) ant.Update(0.02f, context);
        Assert.True(ant.Position.X < 300);
    }

    [Fact]
    public void FlyBrakesBeforeTouchdownAndStillReachesExactTarget()
    {
        var fly = new FlyCreature(new(200, 300));
        var target = new Vector2(400, 300);
        var random = new RandomSource(2);
        float approachSpeed = 0, nearSpeed = float.MaxValue;
        for (var i = 0; i < 500 && !fly.IsResting; i++)
        {
            var context = new CreatureContext(new(target, Vector2.Zero, 0, false, TimeSpan.Zero,
                i == 0 ? new(1, target) : null), new(0, 0, 1000, 1000), i * 0.02f, random);
            fly.Update(0.02f, context);
            if (Vector2.Distance(fly.Position, target) > 100) approachSpeed = Math.Max(approachSpeed, fly.Velocity.Length());
            if (!fly.IsResting && Vector2.Distance(fly.Position, target) < 15) nearSpeed = Math.Min(nearSpeed, fly.Velocity.Length());
        }
        Assert.True(fly.IsResting);
        Assert.Equal(target, fly.Position);
        Assert.True(nearSpeed < approachSpeed * 0.65f, $"Near: {nearSpeed}, approach: {approachSpeed}");
    }
}
