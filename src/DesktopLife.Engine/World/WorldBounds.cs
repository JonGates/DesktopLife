using System.Numerics;
namespace DesktopLife.Engine.World;
public readonly record struct WorldBounds(float Left, float Top, float Width, float Height)
{
    public float Right => Left + Width;
    public float Bottom => Top + Height;
    public Vector2 Center => new(Left + Width / 2, Top + Height / 2);
    // Desktop rectangles exclude their right/bottom edge, so a seam has one owner.
    public bool ContainsScreenPoint(Vector2 p) => p.X >= Left && p.X < Right && p.Y >= Top && p.Y < Bottom;
    public bool Contains(Vector2 p, float margin = 0) => p.X >= Left - margin && p.X <= Right + margin && p.Y >= Top - margin && p.Y <= Bottom + margin;
    public Vector2 Clamp(Vector2 p) => new(System.Math.Clamp(p.X, Left, Right), System.Math.Clamp(p.Y, Top, Bottom));
}
