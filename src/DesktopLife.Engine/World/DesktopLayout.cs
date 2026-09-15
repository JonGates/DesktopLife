using System.Numerics;
namespace DesktopLife.Engine.World;

public readonly record struct DesktopEdge(Vector2 Start, Vector2 End, Vector2 Inward);
public readonly record struct BoundaryPoint(Vector2 Point, Vector2 Inward, float Distance);

/// <summary>The union of physical screen rectangles. Only exposed edges are walls.</summary>
public sealed class DesktopLayout
{
    public IReadOnlyList<DisplayArea> Displays { get; }
    public IReadOnlyList<DesktopEdge> Edges { get; }
    public WorldBounds Bounds { get; }

    public DesktopLayout(IReadOnlyList<DisplayArea> displays)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var d in displays)
        {
            var b = d.Bounds;
            if (string.IsNullOrWhiteSpace(d.Id) || !ids.Add(d.Id) || !float.IsFinite(b.Left) ||
                !float.IsFinite(b.Top) || !float.IsFinite(b.Right) || !float.IsFinite(b.Bottom) ||
                b.Width < 32 || b.Height < 32)
                throw new ArgumentException("Displays need unique IDs and finite rectangles of at least 32 pixels.", nameof(displays));
        }
        Displays = Array.AsReadOnly(displays.ToArray());
        if (displays.Count == 0) { Bounds = new(0, 0, 1920, 1080); Edges = []; return; }
        var left = displays.Min(d => d.Bounds.Left);
        var top = displays.Min(d => d.Bounds.Top);
        Bounds = new(left, top, displays.Max(d => d.Bounds.Right) - left, displays.Max(d => d.Bounds.Bottom) - top);
        var edges = new List<DesktopEdge>();
        foreach (var d in displays)
        {
            var b = d.Bounds;
            AddEdge(d, true, b.Left, b.Top, b.Bottom, Vector2.UnitX, edges);
            AddEdge(d, true, b.Right, b.Top, b.Bottom, -Vector2.UnitX, edges);
            AddEdge(d, false, b.Top, b.Left, b.Right, Vector2.UnitY, edges);
            AddEdge(d, false, b.Bottom, b.Left, b.Right, -Vector2.UnitY, edges);
        }
        Edges = edges.AsReadOnly();
    }

    private void AddEdge(DisplayArea owner, bool vertical, float axis, float from, float to, Vector2 inward, List<DesktopEdge> output)
    {
        var intervals = new List<(float From, float To)> { (from, to) };
        foreach (var other in Displays)
        {
            if (other.Id == owner.Id) continue;
            var b = other.Bounds;
            // The neighboring rectangle must cover the outward side of this edge.
            var outward = axis - (vertical ? inward.X : inward.Y) * 0.01f;
            if (vertical ? outward < b.Left || outward >= b.Right : outward < b.Top || outward >= b.Bottom) continue;
            var cutFrom = vertical ? b.Top : b.Left;
            var cutTo = vertical ? b.Bottom : b.Right;
            var remaining = new List<(float, float)>();
            foreach (var span in intervals)
            {
                if (cutTo <= span.From || cutFrom >= span.To) { remaining.Add(span); continue; }
                if (cutFrom > span.From) remaining.Add((span.From, cutFrom));
                if (cutTo < span.To) remaining.Add((cutTo, span.To));
            }
            intervals = remaining;
        }
        foreach (var span in intervals)
            output.Add(vertical ? new(new(axis, span.From), new(axis, span.To), inward) :
                new(new(span.From, axis), new(span.To, axis), inward));
    }

    public bool Contains(Vector2 point, float margin = 0)
    {
        foreach (var d in Displays)
            if (margin == 0 ? d.Bounds.ContainsScreenPoint(point) : d.Bounds.Contains(point, margin)) return true;
        return false;
    }

    public Vector2 Clamp(Vector2 point)
    {
        if (Contains(point)) return point;
        var best = point;
        var distance = float.PositiveInfinity;
        foreach (var d in Displays)
        {
            var b = d.Bounds;
            var candidate = new Vector2(System.Math.Clamp(point.X, b.Left + 1, b.Right - 1),
                System.Math.Clamp(point.Y, b.Top + 1, b.Bottom - 1));
            var squared = Vector2.DistanceSquared(point, candidate);
            if (squared < distance) { distance = squared; best = candidate; }
        }
        return best;
    }

    public BoundaryPoint NearestEdge(Vector2 point)
    {
        var best = new BoundaryPoint(point, Vector2.UnitX, float.PositiveInfinity);
        foreach (var edge in Edges)
        {
            var delta = edge.End - edge.Start;
            var t = System.Math.Clamp(Vector2.Dot(point - edge.Start, delta) / delta.LengthSquared(), 0, 1);
            var nearest = edge.Start + delta * t;
            var distance = Vector2.Distance(point, nearest);
            if (distance < best.Distance) best = new(nearest, edge.Inward, distance);
        }
        return best;
    }

    public Vector2 ConstrainMove(Vector2 start, Vector2 end)
    {
        start = Clamp(start);
        var delta = end - start;
        var length = delta.Length();
        if (length < 0.0001f) return start;
        var limit = 1f;
        foreach (var edge in Edges)
        {
            var outwardSpeed = Vector2.Dot(delta, edge.Inward);
            if (outwardSpeed >= -0.0001f) continue;
            var t = Vector2.Dot(edge.Start - start, edge.Inward) / outwardSpeed;
            if (t < 0 || t > limit) continue;
            var at = start + delta * t;
            if (at.X < MathF.Min(edge.Start.X, edge.End.X) - 0.001f || at.X > MathF.Max(edge.Start.X, edge.End.X) + 0.001f ||
                at.Y < MathF.Min(edge.Start.Y, edge.End.Y) - 0.001f || at.Y > MathF.Max(edge.Start.Y, edge.End.Y) + 0.001f) continue;
            limit = MathF.Max(0, t - 0.05f / length);
        }
        return Clamp(start + delta * limit);
    }
}
