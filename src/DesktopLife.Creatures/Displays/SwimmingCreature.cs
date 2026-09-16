using System.Numerics;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Creatures.Displays;

/// <summary>Continuous swimming in the physical display union, with one cursor-following turtle.</summary>
public sealed class SwimmingCreature : Creature
{
    public override CreatureKind Kind { get; }
    public override bool IsResting => _dwellRemaining > 0;
    private readonly InsectDefinition _definition;
    private float _heading;
    private float _swimRotation;
    private float _turnRemaining;
    private bool _initialized;
    private long? _lastClick;
    private float _dwellRemaining;

    public SwimmingCreature(Vector2 position, CreatureKind kind, float scale = 1)
    {
        _definition = OceanCatalog.Get(kind);
        Kind = kind;
        Position = position;
        SetScale(scale);
    }

    public override void Relocate(Vector2 position)
    {
        base.Relocate(position);
        _dwellRemaining = 0;
        RestingSeconds = 0;
        _turnRemaining = 0;
    }

    public override void Update(float deltaTime, in CreatureContext context)
    {
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        var dt = MathF.Min(deltaTime, .05f);
        var elapsed = float.IsFinite(context.ElapsedSeconds) && context.ElapsedSeconds > 0 ? context.ElapsedSeconds : deltaTime;
        var previous = Position;
        Vector2 desired;
        if (Kind == CreatureKind.GreenTurtle)
        {
            var click = context.Mouse.Click;
            if (click is not null && click.Sequence != _lastClick)
            {
                _lastClick = click.Sequence;
                _dwellRemaining = 3;
                RestingSeconds = 0;
                Velocity = Vector2.Zero;
                return;
            }
            if (_dwellRemaining > 0)
            {
                _dwellRemaining = MathF.Max(0, _dwellRemaining - elapsed);
                RestingSeconds += elapsed;
                Velocity = Vector2.Zero;
                return;
            }
            RestingSeconds = 0;
            var target = context.Mouse.Position;
            target = context.Layout?.Clamp(target) ?? context.Bounds.Clamp(target);
            var offset = target - Position;
            desired = offset.LengthSquared() > .01f ? Vector2.Normalize(offset) * MathF.Min(_definition.Speed, offset.Length() * 2) : Vector2.Zero;
        }
        else
        {
            if (!_initialized)
            {
                Rotation = _swimRotation = _heading = context.Random.NextFloat(-MathF.PI, MathF.PI);
                _initialized = true;
            }
            _turnRemaining -= dt;
            if (_turnRemaining <= 0)
            {
                _heading = _swimRotation + context.Random.NextFloat(-.8f, .8f);
                _turnRemaining = context.Random.NextFloat(1.2f, 3.5f);
            }
            var direction = new Vector2(MathF.Cos(_heading), MathF.Sin(_heading));
            var ahead = Position + direction * 65;
            var safeAhead = context.Layout?.ConstrainMove(Position, ahead) ?? context.Bounds.Clamp(ahead);
            if (Vector2.DistanceSquared(ahead, safeAhead) > 1)
            {
                var inward = context.Layout?.NearestEdge(Position).Inward ?? Vector2.Normalize(context.Bounds.Center - Position);
                _heading = MathF.Atan2(inward.Y, inward.X);
                _turnRemaining = .6f;
            }
            var angle = MathF.IEEERemainder(_heading - _swimRotation, MathF.Tau);
            _swimRotation += Math.Clamp(angle, -1.5f * dt, 1.5f * dt);
            desired = new Vector2(MathF.Cos(_swimRotation), MathF.Sin(_swimRotation)) * _definition.Speed;
        }
        Velocity = Vector2.Lerp(Velocity, desired, 1 - MathF.Exp(-3 * dt));
        var next = Position + Velocity * dt;
        Position = context.Layout?.ConstrainMove(Position, next) ?? context.Bounds.Clamp(next);
        Velocity = (Position - previous) / dt;
        if (Kind != CreatureKind.GreenTurtle && Velocity.LengthSquared() > .0001f)
            Rotation = MathF.Atan2(Velocity.Y, Velocity.X);
        if (Kind == CreatureKind.GreenTurtle && Velocity.LengthSquared() > .01f)
        {
            var angle = MathF.IEEERemainder(MathF.Atan2(Velocity.Y, Velocity.X) - Rotation, MathF.Tau);
            Rotation += Math.Clamp(angle, -2.5f * dt, 2.5f * dt);
        }
        AdvanceGait(previous, _definition.Stride);
    }
}
