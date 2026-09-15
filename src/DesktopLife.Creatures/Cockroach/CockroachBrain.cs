using System.Numerics;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;

namespace DesktopLife.Creatures.Cockroach;

public readonly record struct CockroachMotion(Vector2 Position, Vector2 Velocity);

/// <summary>Screen-space behavior only; no UI, timers, assets or system calls.</summary>
public sealed class CockroachBrain(CockroachOptions options, bool initiallyHidden)
{
    private bool _initialized;
    private float _crawlSpeed;
    private float _fleeSpeed;
    private float _heading;
    private float _wanderRemaining;
    private float _hiddenRemaining;
    private float _panicRemaining;
    private float _fleeRemaining;
    private float _escapeTurn;
    private float _edgeSign;
    private Vector2 _emergeTarget;
    public CockroachState State { get; private set; } = initiallyHidden ? CockroachState.Hidden : CockroachState.Crawl;

    public CockroachMotion Update(ICreature self, float dt, in CreatureContext context)
    {
        var position = self.Position;
        var velocity = self.Velocity;
        if (!float.IsFinite(dt) || dt <= 0) return new(position, velocity);
        dt = MathF.Min(dt, 0.05f);
        if (!_initialized)
        {
            _crawlSpeed = context.Random.NextFloat(options.CrawlSpeedMin, options.CrawlSpeedMax);
            _fleeSpeed = context.Random.NextFloat(options.FleeSpeedMin, options.FleeSpeedMax);
            _heading = context.Random.NextFloat(0, MathF.Tau);
            _edgeSign = context.Random.NextFloat(0, 1) < 0.5f ? -1 : 1;
            _wanderRemaining = context.Random.NextFloat(0.2f, 0.8f);
            _hiddenRemaining = context.Random.NextFloat(0.3f, 4.5f);
            _initialized = true;
        }

        var mouseOnScreen = context.Bounds.ContainsScreenPoint(context.Mouse.Position);
        var fearRadius = options.FearRadius + MathF.Min(context.Mouse.Speed, 1800) * 0.035f;
        var distanceToMouse = Vector2.Distance(position, context.Mouse.Position);
        var threatened = mouseOnScreen && distanceToMouse < fearRadius;
        var edge = CockroachSteering.NearestEdge(position, context.Bounds);

        if (State == CockroachState.Hidden)
        {
            _hiddenRemaining -= dt * (1 + MathF.Min((float)context.Mouse.IdleTime.TotalSeconds, 5) * 0.12f);
            // Stay hidden while the cursor guards the entry point, even if the timer expired.
            if (_hiddenRemaining > 0 || (mouseOnScreen && Vector2.Distance(edge.Point, context.Mouse.Position) < fearRadius + 35))
                return new(position, Vector2.Zero);
            position = edge.Point - edge.Inward * 12;
            _emergeTarget = edge.Point + edge.Inward * 55;
            _heading = MathF.Atan2(edge.Inward.Y, edge.Inward.X);
            State = CockroachState.Emerge;
        }

        if ((State is CockroachState.Crawl or CockroachState.Emerge) && threatened)
        {
            State = CockroachState.Panic;
            _panicRemaining = context.Random.NextFloat(0.06f, 0.16f);
            _fleeRemaining = context.Random.NextFloat(0.6f, 1.2f);
            _escapeTurn = context.Random.NextFloat(-MathF.PI / 9, MathF.PI / 9);
            // Respond on this frame instead of freezing during the brief startle state.
            velocity = EscapeDirection(position, in context) * _fleeSpeed * 0.65f;
        }

        var headingDirection = new Vector2(MathF.Cos(_heading), MathF.Sin(_heading));
        var separation = CockroachSteering.Separate(self, context.Neighbors, options.SeparationRadius, headingDirection);
        Vector2 desired;
        switch (State)
        {
            case CockroachState.Panic:
                _panicRemaining -= dt;
                if (_panicRemaining <= 0) State = CockroachState.Flee;
                desired = EscapeDirection(position, in context) * _fleeSpeed;
                break;
            case CockroachState.Flee:
                _fleeRemaining -= dt;
                if (_fleeRemaining <= 0 && !threatened && context.Bounds.Contains(position))
                {
                    State = CockroachState.Crawl;
                    _heading = MathF.Atan2(velocity.Y, velocity.X);
                    velocity = VectorMath.NormalizeOrZero(velocity) * _crawlSpeed;
                    desired = velocity;
                }
                else desired = EscapeDirection(position, in context) * _fleeSpeed;
                break;
            case CockroachState.Emerge:
                desired = VectorMath.NormalizeOrZero(_emergeTarget - position + separation * 18) * _crawlSpeed;
                if (Vector2.Distance(position, _emergeTarget) < 12) State = CockroachState.Crawl;
                break;
            default:
                _wanderRemaining -= dt;
                if (_wanderRemaining <= 0)
                {
                    _heading += context.Random.NextFloat(-0.65f, 0.65f);
                    _wanderRemaining = context.Random.NextFloat(0.25f, 0.9f);
                    headingDirection = new(MathF.Cos(_heading), MathF.Sin(_heading));
                }
                var steering = headingDirection + separation * 2.2f;
                if (edge.Distance < 100)
                {
                    var tangent = new Vector2(-edge.Inward.Y, edge.Inward.X) * _edgeSign;
                    steering += tangent * 0.6f + edge.Inward * (MathF.Max(0, 60 - edge.Distance) / 60 * 2);
                }
                desired = VectorMath.NormalizeOrZero(steering) * _crawlSpeed;
                break;
        }

        var escaping = State is CockroachState.Panic or CockroachState.Flee;
        velocity = Vector2.Lerp(velocity, desired, 1 - MathF.Exp(-(escaping ? 18 : 10) * dt));
        position += velocity * dt;
        if (escaping && !context.Bounds.Contains(position, 18))
        {
            State = CockroachState.Hidden;
            _hiddenRemaining = context.Random.NextFloat(1.5f, 5);
            return new(position, Vector2.Zero);
        }
        if (State == CockroachState.Crawl && !context.Bounds.Contains(position, -10))
        {
            position = new(
                Math.Clamp(position.X, context.Bounds.Left + 10, context.Bounds.Right - 10),
                Math.Clamp(position.Y, context.Bounds.Top + 10, context.Bounds.Bottom - 10));
            var inward = CockroachSteering.NearestEdge(position, context.Bounds).Inward;
            velocity = VectorMath.NormalizeOrZero(velocity + inward * _crawlSpeed * 2) * _crawlSpeed;
            _heading = MathF.Atan2(velocity.Y, velocity.X);
        }
        return new(position, velocity);
    }

    private Vector2 EscapeDirection(Vector2 position, in CreatureContext context)
    {
        var away = VectorMath.NormalizeOrZero(position - context.Mouse.Position);
        if (away == Vector2.Zero) away = new(MathF.Cos(_heading), MathF.Sin(_heading));
        var direction = CockroachSteering.Rotate(away, _escapeTurn);
        var edge = CockroachSteering.NearestEdge(position, context.Bounds);
        if (edge.Distance < 100 && Vector2.Dot(away, -edge.Inward) > 0.3f) direction -= edge.Inward * 0.2f;
        return VectorMath.NormalizeOrZero(direction);
    }
}
