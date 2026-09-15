using System.Numerics;
using DesktopLife.Creatures.Displays;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class DisplaySimulationTests
{
    private static readonly DisplayArea Primary = new("primary", new(0, 0, 1920, 1080), true);
    private static readonly DisplayArea Left = new("left", new(-2560, -200, 2560, 1440));
    private static readonly DisplayArea Above = new("above", new(0, -1200, 1920, 1200));

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void PopulationMultipliesByLogicalDisplayCount(int count)
    {
        var simulation = new DisplaySimulation(7);
        simulation.Synchronize(new[] { Primary, Left, Above }.Take(count).ToArray());
        Assert.Equal(count, simulation.Worlds.Count);
        Assert.Equal(count, simulation.TotalFlyCount);
        Assert.Equal(20 * count, simulation.TotalCockroachCount);
        foreach (var item in simulation.Worlds)
        {
            Assert.Single(item.World.Manager.Creatures, c => c.Kind == CreatureKind.Fly);
            Assert.Equal(20, item.World.Manager.Creatures.Count(c => c.Kind == CreatureKind.Cockroach));
            Assert.All(item.World.Manager.Creatures, c => Assert.True(item.Display.Bounds.Contains(c.Position, 100)));
        }
    }

    [Fact]
    public void AddRemoveAndRepeatedEventsDoNotDuplicateOrResetUnchangedWorlds()
    {
        var simulation = new DisplaySimulation(7);
        simulation.Synchronize([Primary]);
        var original = Assert.Single(simulation.Worlds).World;
        simulation.Update(0.02f, new(100, 100));
        simulation.Synchronize([Left, Primary]);
        simulation.Synchronize([Primary, Left]);
        Assert.Equal(40, simulation.TotalCockroachCount);
        Assert.Same(original, simulation.Worlds.Single(w => w.Display.Id == "primary").World);
        Assert.Equal(0.02f, original.TotalTime);
        simulation.Synchronize([Primary]);
        Assert.Same(original, Assert.Single(simulation.Worlds).World);
        Assert.Equal(20, simulation.TotalCockroachCount);
        simulation.Synchronize([]);
        Assert.Empty(simulation.Worlds);
        Assert.Equal(0, simulation.TotalFlyCount);
        simulation.Synchronize([Primary]);
        Assert.Equal(20, simulation.TotalCockroachCount);
    }

    [Fact]
    public void PrimaryFlagChangePreservesCreaturesButBoundsChangeRecreatesOnlyAffectedScreen()
    {
        var simulation = new DisplaySimulation(7);
        simulation.Synchronize([Primary, Left]);
        var primaryWorld = simulation.Worlds[0].World;
        var leftWorld = simulation.Worlds[1].World;
        simulation.Synchronize([Primary with { IsPrimary = false }, Left with { IsPrimary = true }]);
        Assert.Same(primaryWorld, simulation.Worlds[0].World);
        Assert.Same(leftWorld, simulation.Worlds[1].World);
        simulation.Synchronize([Primary, Left with { Bounds = new(-1920, 0, 1920, 1080) }]);
        Assert.Same(primaryWorld, simulation.Worlds[0].World);
        Assert.NotSame(leftWorld, simulation.Worlds[1].World);
        Assert.Equal(-1920, simulation.Worlds[1].World.Bounds.Left);
    }

    [Fact]
    public void CursorOnNegativeCoordinateScreenOnlySummonsThatScreensFly()
    {
        var simulation = new DisplaySimulation(42);
        simulation.Synchronize([Primary, Left]);
        simulation.Update(0.02f, new(-1000, 500));
        simulation.Update(0.02f, new(-995, 500));
        Assert.Equal(FlyState.Offscreen, Fly(simulation, "primary").State);
        Assert.Equal(FlyState.Approach, Fly(simulation, "left").State);
        Assert.All(simulation.Worlds, w => Assert.Equal(0.04f, w.World.TotalTime));
    }

    [Fact]
    public void SharedScreenEdgeBelongsToExactlyOneDisplay()
    {
        var simulation = new DisplaySimulation(42);
        simulation.Synchronize([Primary, Left, Above]);
        simulation.Update(0.02f, new(-5, 500));
        simulation.Update(0.02f, new(0, 500));
        Assert.Equal(FlyState.Approach, Fly(simulation, "primary").State);
        Assert.Equal(FlyState.Offscreen, Fly(simulation, "left").State);
        simulation.Update(0.02f, new(100, 0));
        Assert.Equal(FlyState.Offscreen, Fly(simulation, "above").State);
    }

    [Fact]
    public void ResumeRebasesEveryScreensMouseSample()
    {
        var simulation = new DisplaySimulation(42);
        simulation.Synchronize([Primary, Left]);
        simulation.Update(0.02f, new(100, 500));
        simulation.ResetInput();
        simulation.Update(0.02f, new(-995, 500));
        Assert.All(simulation.Worlds, w => Assert.Equal(0, w.World.Mouse.State.Speed));
        Assert.All(simulation.Worlds, w => Assert.Equal(FlyState.Offscreen, ((FlyCreature)w.World.Manager.Creatures[0]).State));
    }

    [Fact]
    public void DuplicateDeviceIdsAreRejectedWithoutChangingExistingWorlds()
    {
        var simulation = new DisplaySimulation(42);
        simulation.Synchronize([Primary]);
        Assert.Throws<ArgumentException>(() => simulation.Synchronize([Primary, Primary]));
        Assert.Single(simulation.Worlds);
    }

    private static FlyCreature Fly(DisplaySimulation simulation, string id) =>
        (FlyCreature)simulation.Worlds.Single(w => w.Display.Id == id).World.Manager.Creatures[0];
}
