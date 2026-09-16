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
    private readonly float _speed;
    private readonly float _stride;
    private readonly float _turnRate;
    private readonly float _pauseChance;
    private readonly float _pauseDuration;
    private readonly float _wanderAngle;
    private readonly bool _flees;
    public override bool IsResting => _pauseRemaining > 0;
    public CrawlingInsect(Vector2 position, CreatureKind kind, float scale = 1)
    {
        var definition = kind is CreatureKind.Ant or CreatureKind.Caterpillar ? null : InsectCatalog.Get(kind);
        if (!float.IsFinite(scale) || scale < 0.1f || scale > 3f) throw new ArgumentOutOfRangeException(nameof(scale));
        Scale = scale;
        Position = position;
        Kind = kind;
        _speed = definition?.Speed ?? (kind == CreatureKind.Ant ? 48 : 13);
        _stride = definition?.Stride ?? (kind == CreatureKind.Ant ? 12 : 9);
        (_turnRate, _pauseChance, _pauseDuration, _wanderAngle, _flees) = kind switch
        {
            CreatureKind.Ant => (8f, 0.22f, 0.35f, 0.9f, true),
            CreatureKind.Caterpillar => (2f, 0f, 0f, 0.9f, false),
            CreatureKind.Ladybug => (3f, 0.2f, 0.7f, 0.75f, false),
            CreatureKind.GroundBeetle => (5f, 0.12f, 0.4f, 0.7f, true),
            CreatureKind.Earwig => (4f, 0.25f, 0.8f, 1f, true),
            CreatureKind.Silverfish => (9f, 0.3f, 0.45f, 1.4f, true),
            CreatureKind.Cricket => (4f, 0.4f, 1.3f, 1.1f, true),
            CreatureKind.Grasshopper => (2.5f, 0.45f, 1.7f, 0.8f, true),
            CreatureKind.Mantis => (1.2f, 0.65f, 2.8f, 0.6f, false),
            _ => (0.7f, 0.55f, 2.2f, 0.4f, false)
        };
    }
    public override void Update(float deltaTime, in CreatureContext context)
    {
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        deltaTime = MathF.Min(deltaTime, 0.05f);
        var previous = Position;
        var away = Position - context.Mouse.Position;
        var threatened = _flees && away.LengthSquared() is > 1 and < 6400;
        if (threatened) _pauseRemaining = 0;
        if (_pauseRemaining > 0)
        {
            _pauseRemaining -= deltaTime;
            Velocity = Vector2.Zero;
            RestingSeconds += deltaTime;
            return;
        }
        RestingSeconds = 0;
        _turnIn -= deltaTime;
        if (_turnIn <= 0)
        {
            if (_hasWandered && !threatened && _pauseChance > 0 && context.Random.NextFloat(0, 1) < _pauseChance)
                _pauseRemaining = context.Random.NextFloat(0.12f, _pauseDuration);
            _hasWandered = true;
            _heading = Rotation + context.Random.NextFloat(-_wanderAngle, _wanderAngle);
            _turnIn = context.Random.NextFloat(0.6f, 2.5f);
        }
        var speed = _speed;
        if (threatened)
        {
            _heading = MathF.Atan2(away.Y, away.X);
            speed *= 1.8f;
        }
        var turn = MathF.IEEERemainder(_heading - Rotation, MathF.Tau);
        var maxTurn = _turnRate * deltaTime;
        Rotation += Math.Clamp(turn, -maxTurn, maxTurn);
        speed *= MathF.Max(0.2f, 1 - MathF.Abs(turn) / MathF.PI);
        var direction = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
        var next = Position + direction * speed * deltaTime;
        var allowed = context.Layout?.ConstrainMove(Position, next) ?? context.Bounds.Clamp(next);
        Velocity = (allowed - Position) / deltaTime;
        Position = allowed;
        AdvanceGait(previous, _stride * Scale);
        if (Vector2.DistanceSquared(next, allowed) > 0.0001f)
        {
            var inward = context.Layout?.NearestEdge(Position).Inward;
            _heading = inward.HasValue ? MathF.Atan2(inward.Value.Y, inward.Value.X) : Rotation + MathF.PI;
            _turnIn = 0.5f;
        }
    }
}

