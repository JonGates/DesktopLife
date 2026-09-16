using System.Numerics;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Tests;

public class SpiderSilkTests
{
    private static CreatureContext Context(Vector2 mouse, DesktopLayout? layout = null) =>
        new(new(mouse, Vector2.Zero, 0, false, TimeSpan.Zero), layout?.Bounds ?? new(0, 0, 4000, 2000), 0, new MidpointRandom(), Layout: layout);

    [Theory]
    [InlineData(0.6f)]
    [InlineData(1f)]
    [InlineData(3f)]
    public void ThreatCastsThenPullsToFixedAnchorAndClearsSilk(float scale)
    {
        var spider = new CrawlingInsect(new(1000, 1000), CreatureKind.Spider, scale);
        var start = spider.Position;
        spider.Update(0.02f, Context(start - new Vector2(20, 0)));
        Assert.Equal("SilkCasting", spider.MotionState.ToString());
        Assert.Equal(start, spider.Position);
        var anchor = Assert.IsType<Vector2>(spider.SilkAnchor);
        Assert.True(anchor.X > start.X + 100 * scale);
        var states = new HashSet<string>();
        for (var i = 0; i < 150 && spider.SilkAnchor != null; i++)
        {
            states.Add(spider.MotionState.ToString());
            var distance = Vector2.Distance(spider.Position, anchor);
            spider.Update(0.02f, Context(spider.Position - new Vector2(20, 0)));
            Assert.True(Vector2.Distance(spider.Position, anchor) <= distance + 0.001f);
            Assert.Equal(0, spider.Elevation);
            if (spider.SilkAnchor != null) Assert.Equal(anchor, spider.SilkAnchor);
        }
        Assert.Contains("SilkPulling", states);
        Assert.Contains("SilkSettling", states);
        Assert.Null(spider.SilkAnchor);
        Assert.Equal(LocomotionState.Walking, spider.MotionState);
        Assert.InRange(Vector2.Distance(spider.Position, anchor), 0, 0.01f);
        for (var i = 0; i < 100; i++)
        {
            spider.Update(0.02f, Context(spider.Position - new Vector2(20, 0)));
            Assert.Null(spider.SilkAnchor);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(40)]
    public void SilkCrossesTouchingScreensButNeverSpansGaps(int gap)
    {
        var layout = new DesktopLayout([new("left", new(-1000, 0, 1000, 1000)), new("right", new(gap, 0, 1000, 1000))]);
        var spider = new CrawlingInsect(new(-80, 500), CreatureKind.Spider, 3);
        spider.Update(0.02f, Context(new(-100, 500), layout));
        var anchor = Assert.IsType<Vector2>(spider.SilkAnchor);
        Assert.True(layout.Contains(anchor));
        Assert.Equal(gap == 0, anchor.X > 0);
        for (var i = 0; i < 100 && spider.SilkAnchor != null; i++)
        {
            spider.Update(0.02f, Context(new(-10000, -10000), layout));
            Assert.True(layout.Contains(spider.Position));
            if (gap > 0) Assert.True(spider.Position.X < 0);
        }
    }

    [Fact]
    public void InvalidTimeFreezesSilkAndRelocationClearsIt()
    {
        var spider = new CrawlingInsect(new(1000, 1000), CreatureKind.Spider);
        var context = Context(new(980, 1000));
        spider.Update(0.02f, context);
        Assert.NotNull(spider.SilkAnchor);
        var before = (spider.Position, spider.SilkAnchor, spider.MotionState, spider.MotionProgress);
        foreach (var dt in new[] { 0, -1, float.NaN, float.PositiveInfinity })
        {
            spider.Update(dt, context);
            Assert.Equal(before, (spider.Position, spider.SilkAnchor, spider.MotionState, spider.MotionProgress));
        }
        spider.Relocate(new(20, 20));
        Assert.Null(spider.SilkAnchor);
        Assert.Equal(LocomotionState.Walking, spider.MotionState);
        Assert.Equal(Vector2.Zero, spider.Velocity);
    }

    [Fact]
    public void RemovedAnchorScreenCancelsSilkWithoutTeleporting()
    {
        var spider = new CrawlingInsect(new(500, 500), CreatureKind.Spider);
        spider.Update(0.02f, Context(new(480, 500)));
        Assert.NotNull(spider.SilkAnchor);
        var layout = new DesktopLayout([new("remaining", new(0, 0, 600, 1000))]);
        spider.Update(0.02f, Context(new(-10000, -10000), layout));
        Assert.Null(spider.SilkAnchor);
        Assert.Equal(new Vector2(500, 500), spider.Position);
    }

    private sealed class MidpointRandom : IRandomSource
    {
        public float NextFloat(float min, float max) => (min + max) / 2;
    }
}
