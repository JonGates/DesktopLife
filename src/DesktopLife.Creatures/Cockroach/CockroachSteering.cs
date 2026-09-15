using System.Numerics;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Cockroach;

internal static class CockroachSteering
{
    public static Vector2 Rotate(Vector2 direction, float angle)
    {
        var c = MathF.Cos(angle);
        var s = MathF.Sin(angle);
        return new(direction.X * c - direction.Y * s, direction.X * s + direction.Y * c);
    }

    public static Vector2 Separate(ICreature self, IReadOnlyList<ICreature>? neighbors, float radius, Vector2 fallback)
    {
        if (neighbors is null) return Vector2.Zero;
        var force = Vector2.Zero;
        var radiusSquared = radius * radius;
        for (var i = 0; i < neighbors.Count; i++)
        {
            var other = neighbors[i];
            if (ReferenceEquals(self, other) || !other.IsVisible || other.Kind != CreatureKind.Cockroach) continue;
            var away = self.Position - other.Position;
            var squared = away.LengthSquared();
            if (squared >= radiusSquared) continue;
            if (squared < 0.0001f) force += fallback;
            else
            {
                var distance = MathF.Sqrt(squared);
                force += away / distance * (1 - distance / radius);
            }
        }
        return force.LengthSquared() > 1 ? VectorMath.NormalizeOrZero(force) : force;
    }

    public static (Vector2 Point, Vector2 Inward, float Distance) NearestEdge(Vector2 position, WorldBounds bounds)
    {
        var p = bounds.Clamp(position);
        var point = new Vector2(bounds.Left, p.Y);
        var inward = Vector2.UnitX;
        var distance = p.X - bounds.Left;
        if (bounds.Right - p.X < distance) { distance = bounds.Right - p.X; point = new(bounds.Right, p.Y); inward = -Vector2.UnitX; }
        if (p.Y - bounds.Top < distance) { distance = p.Y - bounds.Top; point = new(p.X, bounds.Top); inward = Vector2.UnitY; }
        if (bounds.Bottom - p.Y < distance) { distance = bounds.Bottom - p.Y; point = new(p.X, bounds.Bottom); inward = -Vector2.UnitY; }
        return (point, inward, distance);
    }
}
