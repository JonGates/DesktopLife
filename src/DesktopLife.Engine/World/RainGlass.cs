using System.Numerics;
using DesktopLife.Engine.Input;

namespace DesktopLife.Engine.World;

public sealed class GlassDrop(Vector2 position, float radius)
{
    public Vector2 Position { get; internal set; } = position;
    public float Radius { get; internal set; } = radius;
    public float Speed { get; internal set; }
    public bool Sliding { get; internal set; }
    public float Born { get; internal set; }
}
public sealed record GlassTrail(Vector2 Start, Vector2 End, float Width, float Born);
public sealed record GlassFracture(Vector2 Center, int Style, int Seed, float Born);

/// <summary>Physical pixel coordinates shared by every monitor. Radius cubed represents water volume.</summary>
public sealed class RainGlass(int seed = 73)
{
    private readonly Random _random = new(seed);
    private readonly List<GlassDrop> _drops = [];
    private readonly List<GlassTrail> _trails = [];
    private readonly List<GlassFracture> _fractures = [];
    private float _spawn;
    private long _lastClick;
    private Vector2? _previousCursor;
    public bool Enabled { get; set; }
    public float Time { get; private set; }
    public IReadOnlyList<GlassDrop> Drops => _drops;
    public IReadOnlyList<GlassTrail> Trails => _trails;
    public IReadOnlyList<GlassFracture> Fractures => _fractures;
    public GlassDrop AddDrop(Vector2 position, float radius)
    {
        if (!float.IsFinite(radius) || radius <= 0 || radius > 50 || !float.IsFinite(position.X) || !float.IsFinite(position.Y)) throw new ArgumentOutOfRangeException(nameof(radius));
        var drop = new GlassDrop(position, radius) { Born = Time }; if (_drops.Count < 600) _drops.Add(drop); return drop;
    }
    public void ResetInput() => _previousCursor = null;
    public void Clear() { _drops.Clear(); _trails.Clear(); _fractures.Clear(); _spawn = 0; ResetInput(); }
    public void Update(float elapsed, DesktopLayout layout, Vector2 cursor, MouseClick? click, bool spawn = true)
    {
        if (!Enabled || !float.IsFinite(elapsed) || elapsed <= 0) return;
        var dt = MathF.Min(elapsed, .05f); Time += dt;
        if (click != null && click.Sequence != _lastClick && layout.Contains(click.Position))
        {
            _lastClick = click.Sequence;
            if (_fractures.Count >= 5) _fractures.RemoveAt(0);
            _fractures.Add(new(click.Position, _random.Next(3), _random.Next(), Time));
            foreach (var drop in _drops) if (Vector2.Distance(drop.Position, click.Position) < 160) drop.Sliding = true;
        }
        if (spawn)
        {
            var area = layout.Displays.Sum(d => d.Bounds.Width * d.Bounds.Height);
            _spawn += area / 1_000_000f * 18 * dt;
            while (_spawn >= 1 && _drops.Count < 600 && layout.Displays.Count > 0)
            {
                _spawn--;
                var pick = (float)_random.NextDouble() * area;
                var display = layout.Displays[^1];
                foreach (var candidate in layout.Displays)
                {
                    pick -= candidate.Bounds.Width * candidate.Bounds.Height;
                    if (pick <= 0) { display = candidate; break; }
                }
                AddDrop(new(display.Bounds.Left + (float)_random.NextDouble() * display.Bounds.Width, display.Bounds.Top + (float)_random.NextDouble() * display.Bounds.Height), 2 + (float)_random.NextDouble() * 5);
            }
            _spawn = System.Math.Min(_spawn, 1);
        }
        for (var i = _drops.Count - 1; i >= 0; i--)
        {
            if (i >= _drops.Count) continue;
            var drop = _drops[i]; var start = drop.Position;
            if (DistanceToSegment(start, _previousCursor ?? cursor, cursor) < drop.Radius + 7) drop.Sliding = true;
            drop.Radius = MathF.Cbrt(drop.Radius * drop.Radius * drop.Radius + dt * 5);
            if (drop.Radius >= 7.8f) drop.Sliding = true;
            if (!drop.Sliding) continue;
            drop.Speed = System.Math.Min(700, drop.Speed + dt * (80 + drop.Radius * 25));
            drop.Position += new Vector2(MathF.Sin(Time * 2.2f + start.X * .015f) * 7 * dt, drop.Speed * dt);
            // Sweep the whole travelled segment: fast droplets must not skip smaller drops.
            for (var j = _drops.Count - 1; j >= 0; j--)
            {
                var other = _drops[j]; if (ReferenceEquals(drop, other)) continue;
                if (DistanceToSegment(other.Position, start, drop.Position) > drop.Radius + other.Radius) continue;
                drop.Radius = MathF.Cbrt(MathF.Pow(drop.Radius, 3) + MathF.Pow(other.Radius, 3));
                _drops.RemoveAt(j); if (j < i) i--;
            }
            _trails.Add(new(start, drop.Position, System.Math.Min(drop.Radius * .65f, 10), Time));
            if (!layout.Contains(drop.Position)) _drops.Remove(drop);
        }
        _drops.RemoveAll(d => !layout.Contains(d.Position));
        _trails.RemoveAll(t => Time - t.Born > 2.5f);
        if (_trails.Count > 3500) _trails.RemoveRange(0, _trails.Count - 3500);
        _fractures.RemoveAll(f => Time - f.Born > 4);
        _previousCursor = cursor;
    }
    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var delta = b - a; var length = delta.LengthSquared();
        return Vector2.Distance(p, a + delta * (length < .001f ? 0 : System.Math.Clamp(Vector2.Dot(p - a, delta) / length, 0, 1)));
    }
}
