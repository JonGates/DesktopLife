using System.Numerics;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
namespace DesktopLife.Creatures.Tests;
public class SmallInsectTests
{
    [Theory]
    [InlineData(CreatureKind.Ant)]
    [InlineData(CreatureKind.Caterpillar)]
    public void WalkersCrossTouchingScreensAndNeverJumpGaps(CreatureKind kind)
    {
        foreach (var gap in new[] { 0, 2, 40 })
        {
            var layout = new DesktopLayout([new("left", new(-400, 0, 400, 400), true), new("right", new(gap, 0, 400, 400))]);
            var creature = new CrawlingInsect(new(-1, 200), kind);
            var id = creature.Id;
            for (var i = 0; i < 50; i++)
            {
                var before = creature.Position;
                creature.Update(0.02f, new(new(new(-200, 100), Vector2.Zero, 0, false, TimeSpan.Zero), layout.Bounds, i * 0.02f, new MidpointRandom(), Layout: layout));
                Assert.True(layout.Contains(creature.Position));
                Assert.InRange(Vector2.Distance(before, creature.Position), 0, 2);
                if (gap > 0) Assert.True(creature.Position.X < 0);
            }
            if (gap == 0) Assert.True(creature.Position.X > 0);
            Assert.Equal(id, creature.Id);
        }
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void InvalidCaterpillarCountsAreRejectedAtomically(int count)
    {
        var sim = new DisplaySimulation(12);
        sim.Synchronize([new("main", new(0, 0, 400, 400), true)]);
        var ids = sim.World.Manager.Creatures.Select(c => c.Id).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => sim.SetPopulation(new(20, 20, count)));
        Assert.Equal(ids, sim.World.Manager.Creatures.Select(c => c.Id));
    }
    private sealed class MidpointRandom : IRandomSource
    {
        public float NextFloat(float min, float max) => (min + max) / 2;
    }
}
