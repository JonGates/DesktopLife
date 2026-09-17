using System.Numerics;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.World;

namespace DesktopLife.Engine.Tests;

public class RainGlassTests
{
    [Fact] public void RestingProfilesDependOnInitialSizeAndRemainStable()
    {
        var rain = new RainGlass { Enabled = true };
        var small = rain.AddDrop(new(100, 100), 2);
        var medium = Enumerable.Range(0, 30).Select(i => rain.AddDrop(new(200 + i * 20, 150), 4)).ToArray();
        var large = rain.AddDrop(new(100, 300), 7);
        Assert.InRange(small.RestingShapeIndex, 0, 11);
        Assert.InRange(large.RestingShapeIndex, 36, 47);
        Assert.Equal(3, medium.Select(d => d.RestingShapeIndex / 12).Distinct().Count());
        var profiles = rain.Drops.Select(d => d.RestingShapeIndex).ToArray();
        rain.Update(.05f, Layout, new(-999, -999), null, false);
        Assert.Equal(profiles, rain.Drops.Select(d => d.RestingShapeIndex).ToArray());
    }
    [Fact] public void ReleasedDropContinuesSlidingWithoutPauses()
    {
        var rain = new RainGlass { Enabled = true };
        var layout = new DesktopLayout([new("tall", new(0, 0, 800, 10000), true)]);
        var drop = rain.AddDrop(new(200, 100), 6);
        rain.Update(.05f, layout, drop.Position, null, false);
        var previousY = drop.Position.Y;
        for (var i = 0; i < 200; i++)
        {
            rain.Update(.05f, layout, new(-999, -999), null, false);
            Assert.True(drop.Speed > 0);
            Assert.True(drop.Position.Y > previousY); previousY = drop.Position.Y;
        }
        Assert.True(drop.Position.Y > 100);
    }
    [Theory]
    [InlineData(1, 100)] [InlineData(2, 220)] [InlineData(3, 360)] [InlineData(4, 480)] [InlineData(5, 600)]
    public void RainLevelControlsCapacity(int level, int limit)
    {
        var rain = new RainGlass(); rain.SetLevel(level);
        for (var i = 0; i < 700; i++) rain.AddDrop(new(i, 100), 2);
        Assert.Equal(limit, rain.Drops.Count);
        rain.SetLevel(1); Assert.Equal(100, rain.Drops.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => rain.SetLevel(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => rain.SetLevel(6));
    }
    [Fact] public void FracturesExpireAfterThreeRealSecondsEvenAtLowFrameRate()
    {
        var rain = new RainGlass { Enabled = true };
        rain.Update(.016f, Layout, new(-999, -999), new(1, new(200, 200)), false);
        rain.Update(2.9f, Layout, new(-999, -999), null, false);
        Assert.Single(rain.Fractures);
        rain.Update(.11f, Layout, new(-999, -999), null, false);
        Assert.Empty(rain.Fractures);
    }
    private static readonly DesktopLayout Layout = new([new("screen", new(0, 0, 800, 600), true)]);
    [Fact] public void LargeDropSlidesAndSweepsSmallerDrops()
    {
        var rain = new RainGlass { Enabled = true };
        var large = rain.AddDrop(new(200, 100), 9);
        rain.AddDrop(new(200, 125), 3);
        for (var i = 0; i < 20; i++) rain.Update(.05f, Layout, new(-999, -999), null, false);
        Assert.Single(rain.Drops); Assert.True(large.Position.Y > 125); Assert.True(large.Radius > 9); Assert.NotEmpty(rain.Trails);
    }
    [Fact] public void PointerSweepReleasesSmallPinnedDrop()
    {
        var rain = new RainGlass { Enabled = true }; var drop = rain.AddDrop(new(200, 100), 3);
        rain.Update(.05f, Layout, new(100, 100), null, false);
        Assert.False(drop.Sliding);
        rain.Update(.05f, Layout, new(300, 100), null, false);
        Assert.True(drop.Sliding); Assert.True(drop.Position.Y > 100);
    }
    [Fact] public void ClickIsDeduplicatedAndEffectsExpire()
    {
        var rain = new RainGlass { Enabled = true }; var click = new MouseClick(1, new(200, 200));
        for (var i = 0; i < 10; i++) rain.Update(.05f, Layout, new(-999, -999), click, false);
        Assert.Single(rain.Fractures);
        for (var i = 0; i < 100; i++) rain.Update(.05f, Layout, new(-999, -999), null, false);
        Assert.Empty(rain.Fractures);
    }
    [Fact] public void DisabledSceneIsFrozenAndDropsStayOnDisplays()
    {
        var rain = new RainGlass(); rain.AddDrop(new(200, 100), 9);
        rain.Update(1, Layout, Vector2.Zero, null);
        Assert.Equal(0, rain.Time); Assert.Equal(100, rain.Drops[0].Position.Y);
        rain.Enabled = true;
        for (var i = 0; i < 500; i++) rain.Update(.05f, Layout, new(-999, -999), null);
        Assert.InRange(rain.Drops.Count, 1, 600); Assert.All(rain.Drops, d => Assert.True(Layout.Contains(d.Position)));
    }
    [Fact] public void NegativeMonitorsReceiveRainAndRemovedDisplaysLoseDrops()
    {
        var layout = new DesktopLayout([new("left", new(-800, 0, 800, 600), true), new("right", new(100, 0, 800, 600), false)]);
        var rain = new RainGlass { Enabled = true };
        for (var i = 0; i < 300; i++) rain.Update(.05f, layout, new(-9999, -9999), null);
        Assert.Contains(rain.Drops, d => d.Position.X < 0);
        Assert.Contains(rain.Drops, d => d.Position.X > 100);
        Assert.All(rain.Drops, d => Assert.True(layout.Contains(d.Position)));
        rain.Update(.05f, Layout, new(-9999, -9999), null, false);
        Assert.All(rain.Drops, d => Assert.True(Layout.Contains(d.Position)));
    }
}
