using System.Numerics;
using System.Text.Json;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class OceanTests
{
    [Fact]
    public void LegacySettingsRemainForestAndOceanDefaultsSurviveRoundtrip()
    {
        var legacy = JsonSerializer.Deserialize<PopulationSettings>("{\"Cockroaches\":7,\"Ants\":8,\"Caterpillars\":2}")!;
        Assert.Equal(Habitat.Forest, legacy.Habitat);
        Assert.Equal(new SpeciesPopulation(2, 80, 120), legacy.GetOcean(CreatureKind.Clownfish));
        var restored = JsonSerializer.Deserialize<PopulationSettings>(JsonSerializer.Serialize(legacy with { Habitat = Habitat.Ocean }))!;
        Assert.Equal(7, restored.Cockroaches);
        Assert.Equal(Habitat.Ocean, restored.Habitat);
    }

    [Fact]
    public void SceneSwitchKeepsIndependentConfigurationsAndSameSceneKeepsIndividuals()
    {
        var ocean = OceanCatalog.Fish.ToDictionary(d => d.Kind, _ => new SpeciesPopulation(0));
        ocean[CreatureKind.Clownfish] = new(3, 100, 100);
        var additional = new Dictionary<CreatureKind, SpeciesPopulation> { [CreatureKind.Spider] = new(4) };
        var sim = new DisplaySimulation(42);
        sim.Synchronize([new("a", new(0, 0, 1000, 800))]);
        sim.SetPopulation(new(7, 8, 2, Additional: additional, Habitat: Habitat.Ocean, Ocean: ocean));
        ocean[CreatureKind.Clownfish] = new(0);
        additional.Clear();
        Assert.Equal(4, sim.World.Manager.Creatures.Count);
        Assert.Single(sim.World.Manager.Creatures, c => c.Kind == CreatureKind.GreenTurtle);
        Assert.Equal(0, sim.TotalFlyCount);
        Assert.Equal(0, sim.TotalCockroachCount);
        var originals = sim.World.Manager.Creatures.ToArray();
        sim.SetPopulation(sim.Settings with { Cockroaches = 9 });
        sim.Synchronize([new("b", new(1000, 0, 1000, 800))]);
        Assert.All(originals, c => Assert.Contains(c, sim.World.Manager.Creatures));
        Assert.All(originals, c => Assert.True(sim.Layout.Contains(c.Position)));
        sim.SetPopulation(sim.Settings with { Habitat = Habitat.Forest });
        Assert.Equal(1, sim.TotalFlyCount);
        Assert.Equal(9, sim.TotalCockroachCount);
        Assert.Equal(4, sim.Settings.GetAdditional(CreatureKind.Spider).Count);
        Assert.DoesNotContain(sim.World.Manager.Creatures, c => OceanCatalog.IsOcean(c.Kind));
        sim.SetPopulation(sim.Settings with { Habitat = Habitat.Ocean });
        Assert.Equal(3, sim.World.Manager.Creatures.Count(c => c.Kind == CreatureKind.Clownfish));
    }

    [Theory]
    [InlineData(15, -1, 80, 120)]
    [InlineData(15, 101, 80, 120)]
    [InlineData(15, 2, 9, 120)]
    [InlineData(15, 2, 80, 301)]
    [InlineData(15, 2, 120, 80)]
    [InlineData(14, 2, 80, 120)]
    [InlineData(1, 2, 80, 120)]
    public void InvalidOceanSettingsAreAtomic(int kind, int count, int min, int max)
    {
        var sim = new DisplaySimulation(1);
        sim.SetPopulation(new());
        var ids = sim.World.Manager.Creatures.Select(c => c.Id).ToArray();
        Assert.ThrowsAny<ArgumentException>(() => sim.SetPopulation(sim.Settings with { Ocean = new() { [(CreatureKind)kind] = new(count, min, max) } }));
        Assert.Equal(ids, sim.World.Manager.Creatures.Select(c => c.Id));
        Assert.ThrowsAny<ArgumentException>(() => sim.SetPopulation(sim.Settings with { Habitat = (Habitat)99 }));
        Assert.ThrowsAny<ArgumentException>(() => sim.SetPopulation(sim.Settings with { Ocean = new() { [CreatureKind.Clownfish] = null! } }));
    }

    [Theory]
    [InlineData(1000, true)]
    [InlineData(1020, false)]
    public void TurtleCrossesTouchingDisplaysButCannotCrossGaps(float right, bool crosses)
    {
        var layout = new DesktopLayout([new("a", new(0, 0, 1000, 800)), new("b", new(right, 0, 1000, 800))]);
        var turtle = new SwimmingCreature(new(990, 400), CreatureKind.GreenTurtle);
        var random = new RandomSource(1);
        for (var i = 0; i < 300; i++)
        {
            var previous = turtle.Position;
            turtle.Update(.02f, new(new(new(1200, 400), Vector2.Zero, 0, false, TimeSpan.Zero), layout.Bounds, i * .02f, random, Layout: layout));
            Assert.InRange(Vector2.Distance(previous, turtle.Position), 0, 2);
            Assert.True(layout.Contains(turtle.Position));
        }
        Assert.Equal(crosses, turtle.Position.X > 1000);
    }

    [Fact]
    public void TurtleRetractsImmediatelyForThreeSecondsThenResumesFollowing()
    {
        var turtle = new SwimmingCreature(new(100, 400), CreatureKind.GreenTurtle);
        var random = new RandomSource(1);
        var click = new MouseClick(1, new(200, 400));
        CreatureContext Context() => new(new(new(800, 400), Vector2.Zero, 0, false, TimeSpan.Zero, click), new(0, 0, 1000, 800), 0, random);
        turtle.Update(.02f, Context());
        Assert.InRange(turtle.Position.X, 100, 102);
        for (var i = 0; i < 1000 && !turtle.IsResting; i++) turtle.Update(.02f, Context());
        Assert.True(turtle.IsResting);
        Assert.Equal(new Vector2(100, 400), turtle.Position);
        var arrived = turtle.Position;
        for (var i = 0; i < 145; i++) turtle.Update(.02f, Context());
        Assert.Equal(arrived, turtle.Position);
        for (var i = 0; i < 100; i++) turtle.Update(.02f, Context());
        Assert.False(turtle.IsResting);
        Assert.True(turtle.Position.X > arrived.X + 10);
    }

    [Fact]
    public void ClickOnDisconnectedDisplayRestsInPlaceThenResumesWithoutCrossingGap()
    {
        var layout = new DesktopLayout([new("a", new(0, 0, 1000, 800)), new("b", new(1020, 0, 1000, 800))]);
        var turtle = new SwimmingCreature(new(990, 400), CreatureKind.GreenTurtle);
        var random = new RandomSource(1);
        var click = new MouseClick(1, new(1200, 400));
        var context = new CreatureContext(new(new(700, 400), Vector2.Zero, 0, false, TimeSpan.Zero, click), layout.Bounds, 0, random, Layout: layout);
        for (var i = 0; i < 300; i++)
        {
            var previous = turtle.Position;
            turtle.Update(.02f, context);
            Assert.InRange(Vector2.Distance(previous, turtle.Position), 0, 1);
            Assert.True(turtle.Position.X < 1000);
            Assert.True(layout.Contains(turtle.Position));
        }
        Assert.True(turtle.Position.X < 940, "After resting the turtle should follow the cursor again.");
    }

    [Theory]
    [InlineData(1f, .02f)]
    [InlineData(0f, 1f)]
    [InlineData(float.NaN, 1f)]
    public void TurtleDwellUsesRealElapsedTimeEvenAtLowFrameRate(float elapsed, float delta)
    {
        var turtle = new SwimmingCreature(new(200, 400), CreatureKind.GreenTurtle);
        var context = new CreatureContext(new(new(800, 400), Vector2.Zero, 0, false, TimeSpan.Zero,
            new MouseClick(1, new(200, 400))), new(0, 0, 1000, 800), 0, new RandomSource(1), ElapsedSeconds: elapsed);
        turtle.Update(.02f, context);
        Assert.True(turtle.IsResting);
        turtle.Update(delta, context);
        turtle.Update(delta, context);
        Assert.True(turtle.IsResting);
        Assert.Equal(new Vector2(200, 400), turtle.Position);
        turtle.Update(delta, context);
        Assert.False(turtle.IsResting);
        turtle.Update(delta, context);
        Assert.InRange(turtle.Position.X, 200.01f, 202.5f);
    }

    [Fact]
    public void FishCrossSharedSeamsAndRemainOnTheirSideOfGaps()
    {
        foreach (var gap in new[] { 0, 20 })
        foreach (var definition in OceanCatalog.Fish)
        {
            var layout = new DesktopLayout([new("a", new(0, 0, 1000, 800)), new("b", new(1000 + gap, 0, 1000, 800))]);
            var fish = new SwimmingCreature(new(995, 400), definition.Kind);
            var context = new CreatureContext(default, layout.Bounds, 0, new MidpointRandom(), Layout: layout);
            for (var i = 0; i < 100; i++)
            {
                var previous = fish.Position;
                fish.Update(.02f, context);
                Assert.True(layout.Contains(fish.Position));
                Assert.InRange(Vector2.Distance(previous, fish.Position), 0, definition.Speed * .0201f);
            }
            Assert.Equal(gap == 0, fish.Position.X > 1000);
            var before = fish.Position;
            fish.Update(1000, context);
            Assert.InRange(Vector2.Distance(before, fish.Position), 0, definition.Speed * .0501f);
            fish.Relocate(new(100, 200));
            Assert.Equal(Vector2.Zero, fish.Velocity);
            fish.Update(.02f, context);
            Assert.InRange(Vector2.Distance(new(100, 200), fish.Position), 0, definition.Speed * .0201f);
        }
    }

    private sealed class MidpointRandom : IRandomSource
    {
        public float NextFloat(float min, float max) => (min + max) / 2;
    }

    [Theory]
    [InlineData(.01f)]
    [InlineData(.02f)]
    public void FishCompletesTurnWhileHeadTracksVelocity(float dt)
    {
        var fish = new SwimmingCreature(new(500, 400), CreatureKind.Clownfish);
        var context = new CreatureContext(default, new(0, 0, 1000, 800), 0, new MaximumRandom());
        for (var t = 0f; t < 3; t += dt) fish.Update(dt, context);
        var expected = new Vector2(MathF.Cos(MathF.PI + .8f), MathF.Sin(MathF.PI + .8f));
        Assert.True(Vector2.Dot(Vector2.Normalize(fish.Velocity), expected) > .98f);
    }
    private sealed class MaximumRandom : IRandomSource { public float NextFloat(float min, float max) => max; }

    [Fact]
    public void FishMotionIsContinuousAndInvalidTimeDoesNothing()
    {
        foreach (var definition in OceanCatalog.Fish)
        {
            var fish = new SwimmingCreature(new(500, 400), definition.Kind);
            var context = new CreatureContext(default, new(0, 0, 1000, 800), 0, new RandomSource(8));
            fish.Update(float.NaN, context);
            fish.Update(-1, context);
            Assert.Equal(new Vector2(500, 400), fish.Position);
            var distance = 0f;
            for (var i = 0; i < 100; i++)
            {
                var previous = fish.Position;
                var phase = fish.AnimationPhase;
                fish.Update(.02f, context);
                var step = Vector2.Distance(previous, fish.Position);
                if (step > .001f)
                    Assert.True(Vector2.Dot(Vector2.Normalize(fish.Position - previous), new(MathF.Cos(fish.Rotation), MathF.Sin(fish.Rotation))) > .9999f, "Fish head must follow actual displacement.");
                Assert.InRange(step, 0, definition.Speed * .0201f);
                Assert.InRange((fish.AnimationPhase - phase + 1) % 1, 0, .2f);
                distance += step;
            }
            Assert.True(distance > 5);
        }
    }
}
