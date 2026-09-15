using System.Numerics;
namespace DesktopLife.Engine.Math;
public static class VectorMath
{
    public static Vector2 NormalizeOrZero(Vector2 value)
    {
        var length = value.Length();
        return length > 0.0001f && float.IsFinite(length) ? value / length : Vector2.Zero;
    }
}
