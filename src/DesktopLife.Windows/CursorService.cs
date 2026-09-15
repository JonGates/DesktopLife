using System.Numerics;
namespace DesktopLife.Windows;
public static class CursorService
{
    public static bool TryGetPosition(out Vector2 position)
    {
        var success = NativeMethods.GetCursorPos(out var point);
        position = new(point.X, point.Y);
        return success;
    }
}
