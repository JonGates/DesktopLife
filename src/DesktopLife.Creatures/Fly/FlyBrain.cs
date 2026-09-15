using System.Numerics;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Fly;

public readonly record struct FlyMotion(Vector2 Position, Vector2 Velocity);

/// <summary>Pure steering/state machine. All coordinates and speeds use physical screen pixels.</summary>
public sealed class FlyBrain(FlyOptions options)
{
    private bool _initialized;
    private float _angle;
    private bool _hasWaypoint;
    private Vector2 _waypoint;
    private Vector2 _cursorAnchor;
    private float _hoverRemaining;
    private float _dashRemaining;
    private float _dashSpeed;
    private float _panicRemaining;
    private float _panicCooldown;
    private Vector2 _escapeDirection;
    private Vector2 _departTarget;
    private Vector2 _landingTarget;
    private long _lastClickSequence;
    private double _landedRemaining;
    public FlyState State { get; private set; } = FlyState.Offscreen;

    public FlyMotion Update(Vector2 position, Vector2 velocity, float dt, in CreatureContext context)
    {
        if (!float.IsFinite(dt) || dt <= 0) return new(position, velocity);
        dt = MathF.Min(dt, 0.05f);
        if (!_initialized)
        {
            _angle = context.Random.NextFloat(0, MathF.Tau);

            _initialized = true;
        }
        var mouseOnScreen = (context.Layout?.Contains(context.Mouse.Position) ?? context.Bounds.ContainsScreenPoint(context.Mouse.Position));
        if (context.Mouse.Click is { } click && click.Sequence > _lastClickSequence)
        {
            _lastClickSequence = click.Sequence;
            if (context.Layout?.Contains(click.Position) ?? context.Bounds.ContainsScreenPoint(click.Position))
            {
                _landingTarget = click.Position;
                State = FlyState.Landing;
            }
        }
        if ((State is FlyState.Landing or FlyState.Landed) &&
            !(context.Layout?.Contains(_landingTarget) ?? context.Bounds.ContainsScreenPoint(_landingTarget)))
            State = mouseOnScreen ? FlyState.Approach : FlyState.Offscreen;

        if (State == FlyState.Landing)
        {
            var toLanding = _landingTarget - position;
            var distanceToLanding = toLanding.Length();
            var step = MathF.Min(options.FollowSpeed * dt, distanceToLanding);
            if (distanceToLanding <= step + 0.001f)
            {
                State = FlyState.Landed;
                _landedRemaining = options.LandedSeconds;
                return new(_landingTarget, Vector2.Zero);
            }
            // A fixed destination is independent of later cursor movement; never snap across the screen.
            velocity = toLanding / distanceToLanding * options.FollowSpeed;
            return new(position + toLanding / distanceToLanding * step, velocity);
        }
        if (State == FlyState.Landed)
        {
            var elapsed = float.IsFinite(context.ElapsedSeconds) && context.ElapsedSeconds > 0 ? context.ElapsedSeconds : dt;
            _landedRemaining -= elapsed;
            if (_landedRemaining > 0.000001) return new(_landingTarget, Vector2.Zero);
            State = FlyState.Approach;
        }
        _panicCooldown = MathF.Max(0, _panicCooldown - dt);
        var active = context.Mouse.IsMoving && mouseOnScreen;
        var awakened = false;
        if ((State is FlyState.Offscreen or FlyState.Depart) && mouseOnScreen)
        {
            State = FlyState.Approach;
            awakened = true;
        }
        if (State == FlyState.Offscreen) return new(position, Vector2.Zero);

        if (!mouseOnScreen && State != FlyState.Depart)
        {
            var outer = context.Layout?.NearestEdge(position);
            _departTarget = outer is { } edge ? edge.Point - edge.Inward * 110 : ChooseExit(position, context.Bounds, context.Random);
            State = FlyState.Depart;
        }

        var distance = Vector2.Distance(position, context.Mouse.Position);
        var onDesktop = context.Layout?.Contains(position) ?? context.Bounds.ContainsScreenPoint(position);
        var visibleRoute = onDesktop && (context.Layout == null ||
            Vector2.DistanceSquared(context.Layout.ConstrainMove(position, context.Mouse.Position), context.Mouse.Position) < 0.01f);
        if (!awakened && (State is FlyState.Approach or FlyState.Orbit) && active && context.Mouse.Speed > options.PanicMouseSpeed && distance < 240 && _panicCooldown <= 0)
        {
            State = FlyState.Panic;
            _panicRemaining = context.Random.NextFloat(0.3f, 0.5f);
            _panicCooldown = 1.2f;
            _escapeDirection = VectorMath.NormalizeOrZero(position - context.Mouse.Position);
            if (_escapeDirection == Vector2.Zero) _escapeDirection = new(MathF.Cos(_angle), MathF.Sin(_angle));
            // A sharp escape impulse avoids initially drifting toward the threat.
            velocity = _escapeDirection * options.PanicSpeed;
        }

        Vector2 desired;
        switch (State)
        {
            case FlyState.Panic:
                _panicRemaining -= dt;
                desired = _escapeDirection * options.PanicSpeed;
                if (_panicRemaining <= 0) State = FlyState.Approach;
                break;
            case FlyState.Depart:
                if (context.Layout is { } departureLayout ? !departureLayout.Contains(position) :
                    Vector2.Distance(position, _departTarget) < 20 || !context.Bounds.Contains(position, 75))
                {
                    State = FlyState.Offscreen;
                    return new(position, Vector2.Zero);
                }
                desired = VectorMath.NormalizeOrZero(_departTarget - position) * options.FollowSpeed;
                break;
            default:
                if (!awakened && State == FlyState.Approach && distance < 150 && visibleRoute) State = FlyState.Orbit;
                if (State == FlyState.Orbit && (distance > 260 || !visibleRoute)) State = FlyState.Approach;
                if (State == FlyState.Approach)
                {
                    _hasWaypoint = false;
                    desired = VectorMath.NormalizeOrZero(context.Mouse.Position - position) *
                        MathF.Min(options.FollowSpeed, MathF.Max(0, distance - (visibleRoute ? 70 : 0)) * 8);
                    break;
                }
                // Each dart has a fixed destination. Cursor movement can interrupt a hover,
                // but a stationary cursor never drives a continuously rotating target.
                if (!_hasWaypoint || Vector2.DistanceSquared(_cursorAnchor, context.Mouse.Position) > 10000)
                {
                    _cursorAnchor = context.Mouse.Position;
                    _angle = context.Random.NextFloat(0, MathF.Tau);
                    var radius = context.Random.NextFloat(options.OrbitMinRadius, options.OrbitMaxRadius);
                    var candidate = _cursorAnchor + new Vector2(MathF.Cos(_angle), MathF.Sin(_angle)) * radius;
                    _waypoint = context.Layout?.Clamp(candidate) ?? ClampInside(candidate, context.Bounds);
                    if (context.Layout is { } waypointLayout) _waypoint = waypointLayout.ConstrainMove(_cursorAnchor, _waypoint);
                    _dashSpeed = options.FollowSpeed * context.Random.NextFloat(0.5f, 1);
                    _dashRemaining = context.Random.NextFloat(0.4f, 0.7f);
                    _hoverRemaining = 0;
                    _hasWaypoint = true;
                }
                if (_hoverRemaining > 0)
                {
                    _hoverRemaining -= dt;
                    desired = Vector2.Zero;
                    if (_hoverRemaining <= 0) _hasWaypoint = false;
                }
                else
                {
                    _dashRemaining -= dt;
                    var toTarget = _waypoint - position;
                    desired = VectorMath.NormalizeOrZero(toTarget) * MathF.Min(_dashSpeed, toTarget.Length() * 18);
                    if (toTarget.LengthSquared() < 36 || _dashRemaining <= 0)
                    {
                        _hoverRemaining = context.Random.NextFloat(0.16f, 0.4f);
                        desired = Vector2.Zero;
                    }
                }                break;
        }
        velocity = Vector2.Lerp(velocity, desired, 1 - MathF.Exp(-(State == FlyState.Orbit ? 24 : 12) * dt));
        var next = position + velocity * dt;
        if (State == FlyState.Depart && context.Layout is { } layout && layout.Contains(position))
        {
            var constrained = layout.ConstrainMove(position, next);
            if (Vector2.DistanceSquared(constrained, next) > 0.0001f)
            {
                // Stop at the first exposed boundary even when a narrow gap could be crossed in one tick.
                var edge = layout.NearestEdge(constrained);
                State = FlyState.Offscreen;
                return new(edge.Point - edge.Inward * 0.01f, Vector2.Zero);
            }
        }
        if (State == FlyState.Orbit)
        {
            var visible = context.Layout?.Clamp(next) ?? ClampInside(next, context.Bounds);
            if (visible != next) { velocity = (visible - position) / dt; next = visible; _hasWaypoint = false; }
        }
        return new(next, velocity);
    }

    private static Vector2 ClampInside(Vector2 point, WorldBounds bounds) => new(Math.Clamp(point.X, bounds.Left + 1, bounds.Right - 1), Math.Clamp(point.Y, bounds.Top + 1, bounds.Bottom - 1));

    private static Vector2 ChooseExit(Vector2 position, WorldBounds bounds, IRandomSource random)
    {
        var p = bounds.Clamp(position);
        var edge = 0;
        var nearest = p.X - bounds.Left;
        if (bounds.Right - p.X < nearest) { nearest = bounds.Right - p.X; edge = 1; }
        if (p.Y - bounds.Top < nearest) { nearest = p.Y - bounds.Top; edge = 2; }
        if (bounds.Bottom - p.Y < nearest) edge = 3;
        var offset = random.NextFloat(-90, 90);
        return edge switch
        {
            0 => new(bounds.Left - 110, p.Y + offset),
            1 => new(bounds.Right + 110, p.Y + offset),
            2 => new(p.X + offset, bounds.Top - 110),
            _ => new(p.X + offset, bounds.Bottom + 110)
        };
    }
}
