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
    private float _noisePhase;
    private float _panicRemaining;
    private float _panicCooldown;
    private Vector2 _escapeDirection;
    private Vector2 _departTarget;
    public FlyState State { get; private set; } = FlyState.Offscreen;

    public FlyMotion Update(Vector2 position, Vector2 velocity, float dt, in CreatureContext context)
    {
        if (!float.IsFinite(dt) || dt <= 0) return new(position, velocity);
        dt = MathF.Min(dt, 0.05f);
        if (!_initialized)
        {
            _angle = context.Random.NextFloat(0, MathF.Tau);
            _noisePhase = context.Random.NextFloat(0, MathF.Tau);
            _initialized = true;
        }
        _panicCooldown = MathF.Max(0, _panicCooldown - dt);
        var mouseOnScreen = context.Bounds.ContainsScreenPoint(context.Mouse.Position);
        var active = context.Mouse.IsMoving && mouseOnScreen;
        var awakened = false;
        if ((State is FlyState.Offscreen or FlyState.Depart) && active)
        {
            State = FlyState.Approach;
            awakened = true;
        }
        if (State == FlyState.Offscreen) return new(position, Vector2.Zero);

        if ((!mouseOnScreen || context.Mouse.IdleTime.TotalSeconds > options.IdleDepartSeconds) && State != FlyState.Depart)
        {
            _departTarget = ChooseExit(position, context.Bounds, context.Random);
            State = FlyState.Depart;
        }

        var distance = Vector2.Distance(position, context.Mouse.Position);
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
                if (Vector2.Distance(position, _departTarget) < 20 || !context.Bounds.Contains(position, 75))
                {
                    State = FlyState.Offscreen;
                    return new(position, Vector2.Zero);
                }
                desired = VectorMath.NormalizeOrZero(_departTarget - position) * options.FollowSpeed;
                break;
            default:
                if (!awakened && State == FlyState.Approach && distance < 150) State = FlyState.Orbit;
                if (State == FlyState.Orbit && distance > 260) State = FlyState.Approach;
                _angle = (_angle + dt * 2.5f) % MathF.Tau;
                var radius = (options.OrbitMinRadius + options.OrbitMaxRadius) / 2
                    + MathF.Sin(context.TotalTime * 1.7f + _noisePhase) * (options.OrbitMaxRadius - options.OrbitMinRadius) * 0.28f;
                var offset = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle)) * radius;
                var noise = new Vector2(MathF.Sin(context.TotalTime * 9 + _noisePhase), MathF.Cos(context.TotalTime * 7 + _noisePhase)) * 9;
                var target = context.Mouse.Position + offset + noise;
                var toTarget = target - position;
                desired = VectorMath.NormalizeOrZero(toTarget) * MathF.Min(options.FollowSpeed, toTarget.Length() * 6);
                break;
        }
        velocity = Vector2.Lerp(velocity, desired, 1 - MathF.Exp(-9 * dt));
        return new(position + velocity * dt, velocity);
    }

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
