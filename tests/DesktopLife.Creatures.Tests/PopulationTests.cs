using System.Numerics;
using DesktopLife.Creatures;
using DesktopLife.Creatures.Cockroach;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class PopulationTests
{
    private static readonly WorldBounds Bounds = new(-1920, 0, 1920, 1080);

    [Fact]
    public void DefaultPopulationPreservesFlyAndAddsTwentyHiddenRoachesAtEdges()
    {
        var population = DesktopPopulation.Create(Bounds, new RandomSource(7));
        Assert.Single(population, c => c.Kind == CreatureKind.Fly);
        var roaches = population.OfType<CockroachCreature>().ToArray();
        Assert.Equal(20, roaches.Length);
        Assert.All(roaches, r =>
        {
            Assert.Equal(CockroachState.Hidden, r.State);
            Assert.False(Bounds.Contains(r.Position));
            Assert.True(Bounds.Contains(r.Position, 15));
        });
        Assert.True(roaches.Select(r => r.Position).Distinct().Count() > 15);
    }

    [Fact]
    public void CrowdHasDifferentPanicDurationsSpeedsAndEscapeDirections()
    {
        var bounds = new WorldBounds(0, 0, 1920, 1080);
        var roaches = new CockroachCreature[20];
        for (var i = 0; i < roaches.Length; i++)
        {
            var angle = i * MathF.Tau / roaches.Length;
            roaches[i] = new(bounds.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 80);
        }
        var world = new SimulationWorld(bounds, new RandomSource(42), roaches);
        for (var i = 0; i < 6; i++) world.Update(1f / 60, bounds.Center);
        Assert.Contains(roaches, r => r.State == CockroachState.Panic);
        Assert.Contains(roaches, r => r.State == CockroachState.Flee);
        Assert.True(roaches.Max(r => r.Velocity.Length()) - roaches.Min(r => r.Velocity.Length()) > 40);
        Assert.All(roaches, r => Assert.True(Vector2.Dot(r.Velocity, r.Position - bounds.Center) > 0));
        Assert.True(Vector2.Dot(Vector2.Normalize(roaches[0].Velocity), Vector2.Normalize(roaches[10].Velocity)) < -0.5f);
    }

    [Fact]
    public void InitialEmergenceIsStaggeredAndEventuallyRevealsAllRoaches()
    {
        var population = DesktopPopulation.Create(Bounds, new RandomSource(42));
        var world = new SimulationWorld(Bounds, new RandomSource(42), population);
        var sawPartial = false;
        var maxVisible = 0;
        for (var i = 0; i < 600; i++)
        {
            world.Update(1f / 60, Bounds.Center);
            var visible = population.Count(c => c.Kind == CreatureKind.Cockroach && c.IsVisible);
            if (visible is > 0 and < 20) sawPartial = true;
            maxVisible = Math.Max(maxVisible, visible);
        }
        Assert.True(sawPartial);
        Assert.Equal(20, maxVisible);
    }

    [Theory]
    [InlineData(20, 18000)]
    [InlineData(100, 1800)]
    public void SeededPopulationStaysStableWithMovingMouse(int count, int frames)
    {
        var randomA = new RandomSource(12);
        var randomB = new RandomSource(12);
        var a = DesktopPopulation.Create(Bounds, randomA, count);
        var b = DesktopPopulation.Create(Bounds, randomB, count);
        Assert.Equal(count + 1, a.Count);
        var worldA = new SimulationWorld(Bounds, randomA, a);
        var worldB = new SimulationWorld(Bounds, randomB, b);
        for (var frame = 0; frame < frames; frame++)
        {
            var time = frame / 60f;
            var mouse = Bounds.Center + new Vector2(MathF.Sin(time * 0.7f) * 880, MathF.Cos(time * 1.1f) * 480);
            worldA.Update(1f / 60, mouse);
            worldB.Update(1f / 60, mouse);
            for (var i = 1; i < a.Count; i++)
            {
                Assert.True(float.IsFinite(a[i].Position.X) && float.IsFinite(a[i].Position.Y));
                Assert.True(Bounds.Contains(a[i].Position, 45));
                Assert.InRange(a[i].Velocity.Length(), 0, 421);
                Assert.Equal(a[i].Position, b[i].Position);
            }
        }
    }
}
