using System.Numerics;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Creatures.Displays;

/// <summary>Small walkers share desktop boundaries; touching screen edges are traversable.</summary>
public sealed class CrawlingInsect : Creature
{
    public override CreatureKind Kind { get; }
    private float _turnIn;
    private float _heading;
    public CrawlingInsect(Vector2 position, CreatureKind kind)
    {
        if (kind is not (CreatureKind.Ant or CreatureKind.Caterpillar)) throw new ArgumentOutOfRangeException(nameof(kind));
        Position = position;
        Kind = kind;
    }
    public override void Update(float deltaTime, in CreatureContext context)
    {
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        deltaTime = MathF.Min(deltaTime, 0.05f);
        var previous = Position;
        _turnIn -= deltaTime;
        if (_turnIn <= 0)
        {
            _heading = Rotation + context.Random.NextFloat(-0.9f, 0.9f);
            _turnIn = context.Random.NextFloat(0.6f, 2.5f);
        }
        var speed = Kind == CreatureKind.Ant ? 48f : 13f;
        var away = Position - context.Mouse.Position;
        if (Kind == CreatureKind.Ant && away.LengthSquared() is > 1 and < 6400)
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
        AdvanceGait(previous, Kind == CreatureKind.Ant ? 12 : 9);
        if (Vector2.DistanceSquared(next, allowed) > 0.0001f)
        {
            var inward = context.Layout?.NearestEdge(Position).Inward;
            _heading = inward.HasValue ? MathF.Atan2(inward.Value.Y, inward.Value.X) : Rotation + MathF.PI;
            _turnIn = 0.5f;
        }
    }
}

