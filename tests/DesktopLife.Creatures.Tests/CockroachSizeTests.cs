using System.Numerics;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Creatures.Tests;

public class CockroachSizeTests
{
    [Fact]
    public void SizeSettingsApplyImmediatelyAndSurviveDisplayChanges()
    {
        var simulation = new DisplaySimulation(42);
        simulation.Synchronize([new("main", new(0, 0, 1920, 1080), true)]);
        var before = simulation.World.Manager.Creatures.Select(c => (c.Id, c.Position)).ToArray();
        var settings = new PopulationSettings(RoachMin: 180, RoachMax: 180, AntMin: 60, AntMax: 60, CaterpillarMin: 140, CaterpillarMax: 140);
        simulation.SetPopulation(settings);
        Assert.Equal(before, simulation.World.Manager.Creatures.Select(c => (c.Id, c.Position)).ToArray());
        simulation.Synchronize([new("main", new(0, 0, 1920, 1080), true)]);
        Assert.Equal(settings, simulation.Settings);
        foreach (var c in simulation.World.Manager.Creatures)
            Assert.Equal(c.Kind switch { CreatureKind.Cockroach => 1.8f, CreatureKind.Ant => 0.6f, CreatureKind.Caterpillar => 1.4f, _ => 1f }, c.Scale);
        Assert.Throws<ArgumentOutOfRangeException>(() => simulation.SetPopulation(settings with { AntMin = 150, AntMax = 100 }));
        Assert.Equal(settings, simulation.Settings);
        var legacy = System.Text.Json.JsonSerializer.Deserialize<PopulationSettings>("{\"Cockroaches\":12}")!;
        Assert.Equal(60, legacy.RoachMin); Assert.Equal(180, legacy.RoachMax);
        Assert.Equal(120, legacy.AntMax); Assert.Equal(140, legacy.CaterpillarMax);
    }

    [Theory]
    [InlineData(CreatureKind.Cockroach)]
    [InlineData(CreatureKind.Ant)]
    [InlineData(CreatureKind.Caterpillar)]
    public void PopulationHasStableSmallAndLargeCrawlers(CreatureKind kind)
    {
        var simulation = new DisplaySimulation(42);
        simulation.Synchronize([new("main", new(0, 0, 1920, 1080), true)]);
        simulation.SetPopulation(new(50, 50, 50, 50, 200, 50, 200, 50, 200));
        var roaches = simulation.World.Manager.Creatures.Where(c => c.Kind == kind).ToArray();
        Assert.Contains(roaches, c => c.Scale < 0.75f);
        Assert.Contains(roaches, c => c.Scale > 1.75f);
        var sizes = roaches.ToDictionary(c => c.Id, c => c.Scale);
        for (var i = 0; i < 120; i++) simulation.Update(0.02f, new Vector2(900, 500));
        simulation.Synchronize([new("main", new(0, 0, 1280, 720), true)]);
        simulation.SetPopulation(new(55, 55, 55, 50, 200, 50, 200, 50, 200));
        foreach (var creature in simulation.World.Manager.Creatures.Where(c => sizes.ContainsKey(c.Id)))
            Assert.Equal(sizes[creature.Id], creature.Scale);
        Assert.All(simulation.World.Manager.Creatures.Where(c => c.Kind == kind), c => Assert.InRange(c.Scale, 0.5f, 2f));
    }
}
