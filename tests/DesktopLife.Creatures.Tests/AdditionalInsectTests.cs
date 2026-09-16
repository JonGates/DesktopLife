using System.Text.Json;
using System.Numerics;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Creatures.Tests;

public class AdditionalInsectTests
{
    public static IEnumerable<object[]> Kinds => InsectCatalog.Additional.Select(d => new object[] { d.Kind });

    [Theory]
    [MemberData(nameof(Kinds))]
    public void WalkersMoveContinuouslyAndRespectDisplayGaps(CreatureKind kind)
    {
        var layout = new DesktopLayout([new("left", new(0, 0, 1000, 800)), new("right", new(1020, 0, 1000, 800))]);
        var insect = new CrawlingInsect(new(990, 400), kind);
        var random = new RandomSource(17);
        var distance = 0f;
        for (var i = 0; i < 1000; i++)
        {
            var previous = insect.Position;
            insect.Update(0.02f, new(new(new(-1000, -1000), Vector2.Zero, 0, false, TimeSpan.Zero), layout.Bounds, i * 0.02f, random, Layout: layout));
            var step = Vector2.Distance(previous, insect.Position);
            Assert.InRange(step, 0, kind is CreatureKind.Cricket or CreatureKind.Grasshopper ? 3.61f : kind == CreatureKind.Ladybug ? 2.21f : 2);
            Assert.True(layout.Contains(insect.Position));
            Assert.True(insect.Position.X < 1000);
            distance += step;
        }
        Assert.True(distance > 10);
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void ResizingAndHotplugPreserveAdditionalIndividuals(CreatureKind kind)
    {
        var input = new Dictionary<CreatureKind, SpeciesPopulation> { [kind] = new(3, 100, 100) };
        var simulation = new DisplaySimulation(3);
        simulation.Synchronize([new("screen", new(0, 0, 1000, 800))]);
        simulation.SetPopulation(new(0, 0, 0, Additional: input));
        var originals = simulation.World.Manager.Creatures.Where(c => c.Kind == kind).ToArray();
        input[kind] = new(0);
        simulation.Synchronize([]);
        Assert.Equal(3, simulation.Settings.GetAdditional(kind).Count);
        simulation.SetPopulation(new(0, 0, 0, Additional: new() { [kind] = new(4, 150, 150) }));
        simulation.Synchronize([new("other", new(1000, 0, 1000, 800))]);
        Assert.All(originals, c => Assert.Contains(c, simulation.World.Manager.Creatures));
        Assert.All(simulation.World.Manager.Creatures.Where(c => c.Kind == kind), c => Assert.Equal(1.5f, c.Scale));
    }

    [Theory]
    [InlineData(5, -1, 80, 120)]
    [InlineData(5, 101, 80, 120)]
    [InlineData(5, 1, 9, 120)]
    [InlineData(5, 1, 80, 301)]
    [InlineData(5, 1, 120, 80)]
    [InlineData(3, 1, 80, 120)]
    [InlineData(999, 1, 80, 120)]
    public void InvalidAdditionalSettingsDoNotMutateLivePopulation(int kind, int count, int min, int max)
    {
        var simulation = new DisplaySimulation(4);
        simulation.SetPopulation(new());
        var ids = simulation.World.Manager.Creatures.Select(c => c.Id).ToArray();
        Assert.ThrowsAny<ArgumentException>(() => simulation.SetPopulation(new(Additional: new() { [(CreatureKind)kind] = new(count, min, max) })));
        Assert.Equal(ids, simulation.World.Manager.Creatures.Select(c => c.Id));
    }

    [Fact]
    public void NullAdditionalEntryIsRejected() => Assert.Throws<ArgumentException>(() =>
        new PopulationSettings(Additional: new() { [CreatureKind.Ladybug] = null! }).Validate());

    [Fact]
    public void AdditionalSpeciesAreAppendedWithoutChangingExistingEnumValues()
    {
        Assert.Equal(new[] { "Debug", "Fly", "Cockroach", "Ant", "Caterpillar", "Ladybug", "GroundBeetle", "Earwig", "Silverfish", "Cricket", "Grasshopper", "Mantis", "StickInsect" }, Enum.GetNames<CreatureKind>());
    }

    [Fact]
    public void SavedAdditionalPopulationIsInstantiated()
    {
        var settings = JsonSerializer.Deserialize<PopulationSettings>("""{"Cockroaches":0,"Ants":0,"Caterpillars":0,"Additional":{"5":{"Count":2,"MinPercent":100,"MaxPercent":100}}}""")!;
        var simulation = new DisplaySimulation(7);
        simulation.SetPopulation(settings);
        Assert.Equal(2, simulation.World.Manager.Creatures.Count(c => (int)c.Kind == 5));
    }
}
