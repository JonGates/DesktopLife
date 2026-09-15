using System.Numerics;
namespace DesktopLife.Engine.ScreenSaving;

/// <summary>Allow launch input to settle, then leave on any new session input.</summary>
public sealed class ScreenSaverInputGuard
{
    private bool _armed;
    private uint _lastInput;
    private Vector2 _cursor;
    public bool ShouldExit(double elapsedSeconds, Vector2 cursor, uint lastInput)
    {
        if (!_armed || elapsedSeconds < .75)
        {
            _armed = elapsedSeconds >= .75;
            _lastInput = lastInput;
            _cursor = cursor;
            return false;
        }
        return lastInput != _lastInput || Vector2.DistanceSquared(cursor, _cursor) > 9;
    }
}
