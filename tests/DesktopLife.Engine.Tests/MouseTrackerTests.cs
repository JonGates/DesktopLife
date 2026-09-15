using System.Numerics;
using DesktopLife.Engine.Input;

namespace DesktopLife.Engine.Tests;

public class MouseTrackerTests
{
    [Fact]
    public void FirstSampleDoesNotInventMovement()
    {
        var tracker = new MouseTracker();
        tracker.Update(new(800, 600), 0.05f);
        Assert.Equal(Vector2.Zero, tracker.State.Delta);
        Assert.Equal(0, tracker.State.Speed);
        Assert.False(tracker.State.IsMoving);
    }

    [Fact]
    public void SpeedUsesActualSampleInterval()
    {
        var tracker = new MouseTracker();
        tracker.Update(Vector2.Zero, 0.05f);
        tracker.Update(new(3, 4), 0.05f);
        Assert.Equal(100, tracker.State.Speed, 3);
        Assert.Equal(new Vector2(3, 4), tracker.State.Delta);
        Assert.True(tracker.State.IsMoving);
    }

    [Fact]
    public void IdleAccumulatesRealTimeAndResetsOnMovement()
    {
        var tracker = new MouseTracker();
        tracker.Update(new(100, 100), 0.01f);
        tracker.Update(new(100, 100), 1);
        tracker.Update(new(100, 100), 0.7f);
        Assert.InRange(tracker.State.IdleTime.TotalSeconds, 1.69, 1.71);
        tracker.Update(new(120, 100), 0.05f);
        Assert.Equal(TimeSpan.Zero, tracker.State.IdleTime);
    }

    [Fact]
    public void SubpixelJitterDoesNotResetIdle()
    {
        var tracker = new MouseTracker();
        tracker.Update(new(100, 100), 0.01f);
        tracker.Update(new(100.5f, 100), 0.01f);
        Assert.False(tracker.State.IsMoving);
        Assert.True(tracker.State.IdleTime > TimeSpan.Zero);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void InvalidIntervalsDoNotCorruptTracking(float dt)
    {
        var tracker = new MouseTracker();
        tracker.Update(Vector2.Zero, 0.01f);
        tracker.Update(new(100, 100), dt);
        Assert.True(float.IsFinite(tracker.State.Speed));
        Assert.Equal(Vector2.Zero, tracker.State.Position);
    }
}
