using System.Numerics;
using DesktopLife.Engine.Input;
namespace DesktopLife.Engine.Tests;
public class MouseClickTests
{
    [Fact]
    public void LatestClickKeepsOriginalPhysicalPositionAndIsConsumedOnce()
    {
        var buffer = new MouseClickBuffer();
        buffer.Record(new(100, 200));
        var first = buffer.TakeLatest()!;
        Assert.NotNull(first);
        buffer.Record(new(-1200, 300));
        buffer.Record(new(-1190, 310));
        var last = buffer.TakeLatest()!;
        Assert.Equal(new Vector2(-1190, 310), last.Position);
        Assert.True(last.Sequence > first.Sequence);
        Assert.Null(buffer.TakeLatest());
    }
    [Fact]
    public void PausingDiscardsPendingClicksAndIgnoresNewClicksUntilResumed()
    {
        var buffer = new MouseClickBuffer();
        buffer.Record(new(100, 200));
        var before = buffer.TakeLatest()!;
        buffer.Record(new(110, 200));
        buffer.Enabled = false;
        buffer.Record(new(120, 200));
        buffer.Enabled = true;
        Assert.Null(buffer.TakeLatest());
        buffer.Record(new(130, 200));
        Assert.True(buffer.TakeLatest()!.Sequence > before.Sequence);
    }
    [Fact]
    public void MouseTrackerForwardsClickOnlyInCurrentSample()
    {
        var tracker = new MouseTracker();
        var click = new MouseClick(1, new(-500, 300));
        tracker.Update(new(900, 500), 0.02f, click);
        Assert.Same(click, tracker.State.Click);
        tracker.Update(new(910, 500), 0.02f);
        Assert.Null(tracker.State.Click);
    }
}
