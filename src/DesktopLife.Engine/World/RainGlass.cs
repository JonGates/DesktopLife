using System.Numerics;
using DesktopLife.Engine.Input;

namespace DesktopLife.Engine.World;

public sealed class GlassDrop(Vector2 position, float radius)
{
    public Vector2 Position { get; internal set; } = position;
    public float Radius { get; internal set; } = radius;
    public float Speed { get; internal set; }
    public float MergePulse { get; internal set; }
    public bool Sliding { get; internal set; }
    public float Born { get; internal set; }
    public int ShapeIndex { get; internal set; }
    public int RestingShapeIndex { get; internal set; }
    internal float ReleaseRadius { get; set; } = 7.8f;
    internal Vector2 TrailStart { get; set; } = position;
    internal float TrailTime { get; set; }
}
public sealed record GlassTrail(Vector2 Start, Vector2 End, float Width, float Born);
public sealed record GlassFracture(Vector2 Center, int Style, int Seed, float Born);

/// <summary>Physical pixel coordinates shared by every monitor. Radius cubed represents water volume.</summary>
public sealed class RainGlass(int seed = 73)
{
    public const float MaximumDropRadius = 10;
    private readonly Random _random = new(seed);
    private readonly List<GlassDrop> _drops = [];
    private readonly List<GlassTrail> _trails = [];
    private readonly List<GlassFracture> _fractures = [];
    private float _spawn;
    private long _lastClick;
    private Vector2? _previousCursor;
    public bool Enabled { get; set; }
    public int Level { get; private set; } = 3;
    public int DropLimit => Level switch { 1 => 100, 2 => 220, 3 => 600, 4 => 900, _ => 1200 };
    public void SetLevel(int level)
    {
        if (level is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(level));
        Level = level;
        if (_drops.Count > DropLimit) _drops.RemoveRange(DropLimit, _drops.Count - DropLimit);
    }
    public float Time { get; private set; }
    public IReadOnlyList<GlassDrop> Drops => _drops;
    public IReadOnlyList<GlassTrail> Trails => _trails;
    public IReadOnlyList<GlassFracture> Fractures => _fractures;
    public GlassDrop AddDrop(Vector2 position, float radius)
    {
        if (!float.IsFinite(radius) || radius <= 0 || radius > 50 || !float.IsFinite(position.X) || !float.IsFinite(position.Y)) throw new ArgumentOutOfRangeException(nameof(radius));
        radius = MathF.Min(radius, MaximumDropRadius);
        var drop = new GlassDrop(position, radius) { Born = Time, ShapeIndex = _random.Next(12), ReleaseRadius = 7 + (float)_random.NextDouble() * 1.6f };
        var family = radius < 2.8f ? 0 : radius < 6 ? 1 + drop.ShapeIndex % 3 : 3;
        drop.RestingShapeIndex = family * 12 + drop.ShapeIndex;
        if (_drops.Count < DropLimit) _drops.Add(drop); return drop;
    }
    public void ResetInput() => _previousCursor = null;
    public void Clear() { _drops.Clear(); _trails.Clear(); _fractures.Clear(); _spawn = 0; ResetInput(); }
    public void Update(float elapsed, DesktopLayout layout, Vector2 cursor, MouseClick? click, bool spawn = true)
    {
        if (!Enabled || !float.IsFinite(elapsed) || elapsed <= 0) return;
        var dt = MathF.Min(elapsed, .05f); Time += elapsed;
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
            _spawn += area / 1_000_000f * (Level switch { 1 => 3, 2 => 8, 3 => 22, 4 => 36, _ => 55 }) * dt;
            while (_spawn >= 1 && _drops.Count < DropLimit && layout.Displays.Count > 0)
            {
                _spawn--;
                var pick = (float)_random.NextDouble() * area;
                var display = layout.Displays[^1];
                foreach (var candidate in layout.Displays)
                {
                    pick -= candidate.Bounds.Width * candidate.Bounds.Height;
                    if (pick <= 0) { display = candidate; break; }
                }
                AddDrop(new(display.Bounds.Left + (float)_random.NextDouble() * display.Bounds.Width, display.Bounds.Top + (float)_random.NextDouble() * display.Bounds.Height), 1.3f + MathF.Pow((float)_random.NextDouble(), 2.1f) * 8.5f);
            }
            _spawn = System.Math.Min(_spawn, 1);
        }
        for (var i = _drops.Count - 1; i >= 0; i--)
        {
            if (i >= _drops.Count) continue;
            var drop = _drops[i]; var start = drop.Position;
            drop.MergePulse *= MathF.Exp(-8 * dt);
            if (DistanceToSegment(start, _previousCursor ?? cursor, cursor) < drop.Radius + 7) drop.Sliding = true;
            drop.Radius = MathF.Min(MaximumDropRadius, MathF.Cbrt(drop.Radius * drop.Radius * drop.Radius + dt * 5));
            if (drop.Radius >= drop.ReleaseRadius) drop.Sliding = true;
            if (!drop.Sliding) continue;
            var targetSpeed = System.Math.Clamp(1.8f * drop.Radius * drop.Radius, 4, 180);
            drop.Speed += (targetSpeed - drop.Speed) * (1 - MathF.Exp(-7 * dt));
            drop.Position += new Vector2(MathF.Sin(Time * .6f + drop.ShapeIndex) * 3 * dt, drop.Speed * dt);
            // Sweep the whole travelled segment: fast droplets must not skip smaller drops.
            for (var j = _drops.Count - 1; j >= 0; j--)
            {
                var other = _drops[j]; if (ReferenceEquals(drop, other)) continue;
                if (DistanceToSegment(other.Position, start, drop.Position) > drop.Radius + other.Radius) continue;
                drop.MergePulse = MathF.Min(1, drop.MergePulse + other.Radius / drop.Radius);
                // Visual size cap: excess merged volume is not retained by this simplified simulation.
                drop.Radius = MathF.Min(MaximumDropRadius, MathF.Cbrt(MathF.Pow(drop.Radius, 3) + MathF.Pow(other.Radius, 3)));
                _drops.RemoveAt(j); if (j < i) i--;
            }
            if (Vector2.DistanceSquared(drop.TrailStart, drop.Position) >= 100 || Time - drop.TrailTime >= .1f)
            {
                _trails.Add(new(drop.TrailStart, drop.Position, System.Math.Min(drop.Radius * .65f, 10), Time));
                drop.TrailStart = drop.Position; drop.TrailTime = Time;
            }
            if (!layout.Contains(drop.Position)) _drops.Remove(drop);
        }
        _drops.RemoveAll(d => !layout.Contains(d.Position));
        _trails.RemoveAll(t => Time - t.Born > 5f);
        if (_trails.Count > 1000) _trails.RemoveRange(0, _trails.Count - 1000);
        _fractures.RemoveAll(f => Time - f.Born >= 3);
        _previousCursor = cursor;
    }
    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var delta = b - a; var length = delta.LengthSquared();
        return Vector2.Distance(p, a + delta * (length < .001f ? 0 : System.Math.Clamp(Vector2.Dot(p - a, delta) / length, 0, 1)));
    }
}
