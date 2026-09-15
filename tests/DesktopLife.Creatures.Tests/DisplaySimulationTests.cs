using System.Numerics;
using DesktopLife.Creatures.Displays;
using DesktopLife.Creatures.Cockroach;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
namespace DesktopLife.Creatures.Tests;

public class DisplaySimulationTests
{
    private static readonly DisplayArea Primary = new("primary", new(0, 0, 1000, 800), true);
    private static readonly DisplayArea Right = new("right", new(1000, 0, 1000, 800));
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void DisplaysShareOneWorldAndDoNotMultiplyPopulation(int count)
    {
        var simulation = new DisplaySimulation(7);
        simulation.Synchronize(new[] { Primary, Right, new DisplayArea("above", new(0, -800, 1000, 800)) }.Take(count).ToArray());
        Assert.Equal(count, simulation.Worlds.Count);
        Assert.All(simulation.Worlds, w => Assert.Same(simulation.World, w.World));
        Assert.Equal(1, simulation.TotalFlyCount);
        Assert.Equal(20, simulation.TotalCockroachCount);
        simulation.Update(0.02f, new(300, 300));
        Assert.Equal(0.02f, simulation.World.TotalTime);
    }
    [Fact]
    public void ChangingCountsPreservesExistingIndividualsAndZeroIsSupported()
    {
        var sim = new DisplaySimulation(7);
        sim.Synchronize([Primary, Right]);
        var original = sim.World.Manager.Creatures.ToArray();
        sim.SetPopulation(3, 25);
        Assert.Equal(28, sim.World.Manager.Creatures.Count);
        Assert.All(original, c => Assert.Contains(c, sim.World.Manager.Creatures));
        sim.SetPopulation(1, 10);
        Assert.Equal(1, sim.World.Manager.Creatures.Count(c => c.Kind == CreatureKind.Fly));
        Assert.Equal(10, sim.World.Manager.Creatures.Count(c => c.Kind == CreatureKind.Cockroach));
        sim.SetPopulation(0, 0);
        Assert.Empty(sim.World.Manager.Creatures);
        sim.Synchronize([Right]);
        Assert.Empty(sim.World.Manager.Creatures);
        sim.SetPopulation(2, 4);
        Assert.Equal(6, sim.World.Manager.Creatures.Count);
    }
    [Fact]
    public void LayoutChangesAndZeroScreensPreserveIdsAndConfiguredTotal()
    {
        var sim = new DisplaySimulation(7);
        sim.Synchronize([Primary, Right]);
        sim.SetPopulation(2, 8);
        var ids = sim.World.Manager.Creatures.Select(c => c.Id).ToArray();
        sim.Synchronize([Right with { IsPrimary = true }]);
        Assert.Equal(ids, sim.World.Manager.Creatures.Select(c => c.Id));
        Assert.All(sim.World.Manager.Creatures, c => Assert.True(Right.Bounds.ContainsScreenPoint(c.Position)));
        sim.Synchronize([]);
        sim.Update(1, new(1100, 300));
        Assert.Equal(0, sim.World.TotalTime);
        Assert.Equal(8, sim.TotalCockroachCount);
        sim.Synchronize([Primary]);
        Assert.Equal(ids, sim.World.Manager.Creatures.Select(c => c.Id));
        sim.Update(0.02f, new(100, 200));
        sim.ResetInput();
        sim.Update(0.02f, new(800, 500));
        Assert.Equal(0, sim.World.Mouse.State.Speed);
    }
    [Theory]
    [InlineData(-1, 20)]
    [InlineData(21, 20)]
    [InlineData(1, -1)]
    [InlineData(1, 501)]
    public void InvalidCountsDoNotChangePopulation(int flies, int roaches)
    {
        var sim = new DisplaySimulation(7);
        sim.Synchronize([Primary]);
        var ids = sim.World.Manager.Creatures.Select(c => c.Id).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => sim.SetPopulation(flies, roaches));
        Assert.Equal(ids, sim.World.Manager.Creatures.Select(c => c.Id));
    }
    [Fact]
    public void InvalidLayoutDoesNotChangeCurrentScreens()
    {
        var sim = new DisplaySimulation(7);
        sim.Synchronize([Primary]);
        Assert.Throws<ArgumentException>(() => sim.Synchronize([Primary, Primary]));
        Assert.Single(sim.Worlds);
    }
    [Theory]
    [InlineData(1000, 0, 995, 400, 0)]
    [InlineData(-1000, 0, 5, 400, 3.1415927f)]
    [InlineData(0, 800, 500, 795, 1.5707964f)]
    [InlineData(0, -800, 500, 5, -1.5707964f)]
    public void SameCockroachWalksAcrossSharedEdgeWithoutHiding(float x, float y, float sx, float sy, float angle)
    {
        var next = new DisplayArea("neighbor", new(x, y, 1000, 800));
        var layout = new DesktopLayout([Primary, next]);
        var roach = new CockroachCreature(new(sx, sy));
        var id = roach.Id;
        var random = new HeadingRandom(angle);
        var previous = roach.Position;
        for (var i = 0; i < 20; i++)
        {
            roach.Update(0.02f, new(new(new(500, 300), Vector2.Zero, 0, false, TimeSpan.Zero),
                layout.Bounds, i * 0.02f, random, Layout: layout));
            Assert.InRange(Vector2.Distance(previous, roach.Position), 0, 2);
            Assert.Equal(CockroachState.Crawl, roach.State);
            previous = roach.Position;
        }
        Assert.Equal(id, roach.Id);
        Assert.True(next.Bounds.ContainsScreenPoint(roach.Position));
    }
    [Fact]
    public void FleeingCockroachDoesNotHideAtAnInternalSeam()
    {
        var layout = new DesktopLayout([Primary, Right]);
        var roach = new CockroachCreature(new(995, 400));
        for (var i = 0; i < 20; i++)
            roach.Update(0.02f, new(new(new(930, 400), Vector2.Zero, 0, false, TimeSpan.Zero),
                layout.Bounds, i * 0.02f, new HeadingRandom(0), Layout: layout));
        Assert.True(roach.IsVisible);
        Assert.True(Right.Bounds.ContainsScreenPoint(roach.Position));
    }
    private sealed class HeadingRandom(float angle) : IRandomSource
    {
        public float NextFloat(float min, float max) => max == MathF.Tau ? angle : (min + max) / 2;
    }

    [Theory]
    [InlineData(2)]
    [InlineData(20)]
    public void FleeingCockroachHidesAtExposedEdgeInsteadOfJumpingAGap(int gap)
    {
        var layout = new DesktopLayout([Primary, Right with { Bounds = new(1000 + gap, 0, 1000, 800) }]);
        var roach = new CockroachCreature(new(995, 400));
        for (var i = 0; i < 5 && roach.IsVisible; i++)
            roach.Update(0.02f, new(new(new(930, 400), Vector2.Zero, 0, false, TimeSpan.Zero),
                layout.Bounds, i * 0.02f, new HeadingRandom(0), Layout: layout));
        Assert.False(roach.IsVisible);
        Assert.True(roach.Position.X < 1000 + gap);
    }

    [Fact]
    public void CrawlingRemainsInActualScreensForAnOffsetLayout()
    {
        var layout = new DesktopLayout([Primary, Right with { Bounds = new(1000, 300, 1000, 800) }]);
        var roach = new CockroachCreature(new(995, 100));
        for (var i = 0; i < 300; i++)
        {
            roach.Update(0.02f, new(new(new(500, 500), Vector2.Zero, 0, false, TimeSpan.Zero),
                layout.Bounds, i * 0.02f, new HeadingRandom(0), Layout: layout));
            Assert.True(layout.Contains(roach.Position));
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(50)]
    public void DepartingFlyCannotDisappearInsideAnotherScreen(int gap)
    {
        var layout = new DesktopLayout([Primary, Right with { Bounds = new(1000 + gap, 0, 1000, 800) }]);
        var fly = new FlyCreature(new(990, 400));
        var random = new HeadingRandom(0);
        fly.Update(0.02f, new(new(new(990, 410), Vector2.One, 20, true, TimeSpan.Zero), layout.Bounds, 0, random, Layout: layout));
        for (var i = 0; i < 300 && fly.State != FlyState.Offscreen; i++)
            fly.Update(0.02f, new(new(new(-500, -500), Vector2.Zero, 0, false, TimeSpan.FromSeconds(3)), layout.Bounds, i * 0.02f, random, Layout: layout));
        Assert.Equal(FlyState.Offscreen, fly.State);
        Assert.False(layout.Contains(fly.Position));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(20)]
    public void HiddenCockroachReemergesOnItsSideOfTheGap(int gap)
    {
        var layout = new DesktopLayout([Primary, Right with { Bounds = new(1000 + gap, 0, 1000, 800) }]);
        var roach = new CockroachCreature(new(995, 400));
        var random = new HeadingRandom(0);
        for (var i = 0; i < 5 && roach.IsVisible; i++)
            roach.Update(0.02f, new(new(new(930, 400), Vector2.Zero, 0, false, TimeSpan.Zero), layout.Bounds, 0, random, Layout: layout));
        Assert.False(roach.IsVisible);
        var emerged = false;
        for (var i = 0; i < 400; i++)
        {
            roach.Update(0.02f, new(new(new(500, 100), Vector2.Zero, 0, false, TimeSpan.FromSeconds(5)), layout.Bounds, i * 0.02f, random, Layout: layout));
            if (roach.IsVisible) { emerged = true; Assert.True(Primary.Bounds.ContainsScreenPoint(roach.Position)); }
            if (roach.State == CockroachState.Crawl) break;
        }
        Assert.True(emerged);
        Assert.Equal(CockroachState.Crawl, roach.State);
    }
}
