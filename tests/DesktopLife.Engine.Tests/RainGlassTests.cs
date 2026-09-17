using System.Numerics;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.World;

namespace DesktopLife.Engine.Tests;

public class RainGlassTests
{
    [Fact] public void ReleasedDropBrieflyPinsThenResumes()
    {
        var rain = new RainGlass { Enabled = true };
        var layout = new DesktopLayout([new("tall", new(0, 0, 800, 10000), true)]);
        var drop = rain.AddDrop(new(200, 100), 6);
        rain.Update(.05f, layout, drop.Position, null, false);
        var stopped = false; var resumed = false;
        for (var i = 0; i < 200; i++)
        {
            rain.Update(.05f, layout, new(-999, -999), null, false);
            if (drop.Speed == 0) stopped = true;
            if (stopped && drop.Speed > 0) resumed = true;
        }
        Assert.True(stopped); Assert.True(resumed); Assert.True(drop.Position.Y > 100);
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
