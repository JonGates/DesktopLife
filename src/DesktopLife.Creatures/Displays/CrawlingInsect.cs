using System.Numerics;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Creatures.Displays;

/// <summary>Small walkers share desktop boundaries; touching screen edges are traversable.</summary>
public sealed class CrawlingInsect : Creature
{
    public override CreatureKind Kind { get; }
    private float _turnIn;
    public CrawlingInsect(Vector2 position, CreatureKind kind)
    {
        if (kind is not (CreatureKind.Ant or CreatureKind.Caterpillar)) throw new ArgumentOutOfRangeException(nameof(kind));
        Position = position;
        Kind = kind;
    }
    public override void Update(float deltaTime, in CreatureContext context)
    {
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        _turnIn -= deltaTime;
        if (_turnIn <= 0)
        {
            Rotation += context.Random.NextFloat(-0.9f, 0.9f);
            _turnIn = context.Random.NextFloat(0.6f, 2.5f);
        }
        var speed = Kind == CreatureKind.Ant ? 48f : 13f;
        var away = Position - context.Mouse.Position;
        if (Kind == CreatureKind.Ant && away.LengthSquared() is > 1 and < 6400)
        {
            Rotation = MathF.Atan2(away.Y, away.X);
            speed *= 1.8f;
        }
        var direction = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
        var next = Position + direction * speed * deltaTime;
        var allowed = context.Layout?.ConstrainMove(Position, next) ?? context.Bounds.Clamp(next);
        Velocity = (allowed - Position) / deltaTime;
        Position = allowed;
        if (Vector2.DistanceSquared(next, allowed) > 0.0001f)
        {
            var inward = context.Layout?.NearestEdge(Position).Inward;
            Rotation = inward.HasValue ? MathF.Atan2(inward.Value.Y, inward.Value.X) : Rotation + MathF.PI;
            _turnIn = 0.5f;
        }
    }
}

