using System.Numerics;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Creatures.Displays;

/// <summary>Small walkers share desktop boundaries; touching screen edges are traversable.</summary>
public sealed class CrawlingInsect : Creature
{
    public override CreatureKind Kind { get; }
    private float _turnIn;
    private float _heading;
    private float _pauseRemaining;
    private bool _hasWandered;
    public CrawlingInsect(Vector2 position, CreatureKind kind, float scale = 1)
    {
        if (kind is not (CreatureKind.Ant or CreatureKind.Caterpillar)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (!float.IsFinite(scale) || scale < 0.1f || scale > 3f) throw new ArgumentOutOfRangeException(nameof(scale));
        Scale = scale;
        Position = position;
        Kind = kind;
    }
    public override void Update(float deltaTime, in CreatureContext context)
    {
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        deltaTime = MathF.Min(deltaTime, 0.05f);
        var previous = Position;
        var away = Position - context.Mouse.Position;
        var threatened = Kind == CreatureKind.Ant && away.LengthSquared() is > 1 and < 6400;
        if (threatened) _pauseRemaining = 0;
        if (_pauseRemaining > 0)
        {
            _pauseRemaining -= deltaTime;
            Velocity = Vector2.Zero;
            return;
        }
        _turnIn -= deltaTime;
        if (_turnIn <= 0)
        {
            if (_hasWandered && Kind == CreatureKind.Ant && !threatened && context.Random.NextFloat(0, 1) < 0.22f)
                _pauseRemaining = context.Random.NextFloat(0.12f, 0.35f);
            _hasWandered = true;
            _heading = Rotation + context.Random.NextFloat(-0.9f, 0.9f);
            _turnIn = context.Random.NextFloat(0.6f, 2.5f);
        }
        var speed = Kind == CreatureKind.Ant ? 48f : 13f;
        if (threatened)
        {
            _heading = MathF.Atan2(away.Y, away.X);
            speed *= 1.8f;
        }
        var turn = MathF.IEEERemainder(_heading - Rotation, MathF.Tau);
        var maxTurn = (Kind == CreatureKind.Ant ? 8 : 2) * deltaTime;
        Rotation += Math.Clamp(turn, -maxTurn, maxTurn);
        speed *= MathF.Max(0.2f, 1 - MathF.Abs(turn) / MathF.PI);
        var direction = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
        var next = Position + direction * speed * deltaTime;
        var allowed = context.Layout?.ConstrainMove(Position, next) ?? context.Bounds.Clamp(next);
        Velocity = (allowed - Position) / deltaTime;
        Position = allowed;
        AdvanceGait(previous, (Kind == CreatureKind.Ant ? 12 : 9) * Scale);
        if (Vector2.DistanceSquared(next, allowed) > 0.0001f)
        {
            var inward = context.Layout?.NearestEdge(Position).Inward;
            _heading = inward.HasValue ? MathF.Atan2(inward.Value.Y, inward.Value.X) : Rotation + MathF.PI;
            _turnIn = 0.5f;
        }
    }
}

