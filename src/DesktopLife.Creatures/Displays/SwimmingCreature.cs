using System.Numerics;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Creatures.Displays;

/// <summary>Continuous swimming in the physical display union, with one cursor-following turtle.</summary>
public sealed class SwimmingCreature : Creature
{
    public override CreatureKind Kind { get; }
    public override bool IsResting => _cursorMotion?.IsResting ?? false;
    private readonly FlyCreature? _cursorMotion;
    private readonly InsectDefinition _definition;
    private float _heading;
    private float _swimRotation;
    private float _turnRemaining;
    private bool _initialized;

    public SwimmingCreature(Vector2 position, CreatureKind kind, float scale = 1)
    {
        _definition = OceanCatalog.Get(kind);
        Kind = kind;
        Position = position;
        SetScale(scale);
        if (kind == CreatureKind.GreenTurtle)
        {
            _cursorMotion = new FlyCreature(position);
            IsVisible = _cursorMotion.IsVisible;
        }
    }

    public override void Relocate(Vector2 position)
    {
        base.Relocate(position);
        _cursorMotion?.Relocate(position);
        RestingSeconds = 0;
        _turnRemaining = 0;
    }

    public override void Update(float deltaTime, in CreatureContext context)
    {
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        var dt = MathF.Min(deltaTime, .05f);
        var previous = Position;
        if (_cursorMotion is not null)
        {
            // Share the fly's speed and state machine; only appearance and gait differ.
            var wasResting = IsResting;
            _cursorMotion.Update(deltaTime, context);
            Position = _cursorMotion.Position;
            Velocity = _cursorMotion.Velocity;
            Rotation = _cursorMotion.Rotation;
            IsVisible = _cursorMotion.IsVisible;
            RestingSeconds = IsResting ? (wasResting ? RestingSeconds + deltaTime : 0) : 0;
            AdvanceGait(previous, _definition.Stride);
            return;
        }
        Vector2 desired;
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
        AdvanceGait(previous, _definition.Stride);
    }
}
