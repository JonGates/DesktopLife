using System.Numerics;
using DesktopLife.Engine.ScreenSaving;

namespace DesktopLife.Engine.Tests;

public sealed class ScreenSaverTests
{
    [Theory]
    [InlineData("--settings", false)]
    [InlineData("/S", true)]
    [InlineData("/p:123", true)]
    [InlineData("-c", true)]
    public void MainApplicationRecognizesWindowsInvocation(string argument, bool expected) =>
        Assert.Equal(expected, ScreenSaverArguments.IsInvocation([argument]));
    [Theory]
    [InlineData("/s", ScreenSaverMode.Run, 0)]
    [InlineData("/S", ScreenSaverMode.Run, 0)]
    [InlineData("-s", ScreenSaverMode.Run, 0)]
    [InlineData("/c", ScreenSaverMode.Configure, 0)]
    [InlineData("/c:1234", ScreenSaverMode.Configure, 1234)]
    [InlineData("/p:1234", ScreenSaverMode.Preview, 1234)]
    public void WindowsArguments(string argument, ScreenSaverMode mode, long parent)
    {
        var actual = ScreenSaverArguments.Parse([argument]);
        Assert.Equal(mode, actual.Mode);
        Assert.Equal(parent, actual.Parent);
    }

    [Fact]
    public void NoArgumentsOpensConfiguration() => Assert.Equal(ScreenSaverMode.Configure, ScreenSaverArguments.Parse([]).Mode);

    [Fact]
    public void SeparatePreviewParentSupports64BitHandles() => Assert.Equal(4294967296L, ScreenSaverArguments.Parse(["/p", "4294967296"]).Parent);

    [Theory]
    [InlineData("/p")]
    [InlineData("/p:0")]
    [InlineData("/p:-1")]
    [InlineData("/p:abc")]
    [InlineData("/p:999999999999999999999999")]
    [InlineData("/s:12")]
    [InlineData("/unexpected")]
    public void InvalidArgumentsNeverStartFullScreen(string argument) => Assert.Equal(ScreenSaverMode.Invalid, ScreenSaverArguments.Parse([argument]).Mode);

    [Fact]
    public void ExtraArgumentsAreRejected() => Assert.Equal(ScreenSaverMode.Invalid, ScreenSaverArguments.Parse(["/s", "extra"]).Mode);

    [Fact]
    public void LaunchInputIsIgnoredButLaterInputExits()
    {
        var guard = new ScreenSaverInputGuard();
        Assert.False(guard.ShouldExit(.1, new(10, 10), 10));
        Assert.False(guard.ShouldExit(.6, new(40, 40), 20));
        Assert.False(guard.ShouldExit(.8, new(40, 40), 20));
        Assert.True(guard.ShouldExit(.9, new(45, 40), 21));
    }

    [Fact]
    public void KeyboardInputExitsWithoutCursorMovement()
    {
        var guard = new ScreenSaverInputGuard();
        guard.ShouldExit(.8, Vector2.Zero, 100);
        Assert.True(guard.ShouldExit(.9, Vector2.Zero, 101));
    }

    [Fact]
    public void IdleCursorDoesNotExit()
    {
        var guard = new ScreenSaverInputGuard();
        guard.ShouldExit(.8, new(-100, 10), 10);
        Assert.False(guard.ShouldExit(500, new(-100, 10), 10));
    }
}
