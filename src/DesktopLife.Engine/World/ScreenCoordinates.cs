using System.Numerics;
namespace DesktopLife.Engine.World;
public static class ScreenCoordinates
{
    public static Vector2 ToLocal(Vector2 screen, Vector2 origin, float scaleX, float scaleY)
    {
        if (scaleX <= 0 || scaleY <= 0 || !float.IsFinite(scaleX) || !float.IsFinite(scaleY)) throw new ArgumentOutOfRangeException(nameof(scaleX));
        return (screen - origin) / new Vector2(scaleX, scaleY);
    }
}
