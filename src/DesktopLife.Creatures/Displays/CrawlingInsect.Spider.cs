using System.Numerics;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Creatures.Displays;

public sealed partial class CrawlingInsect
{
    private float _silkCooldown;
    private Vector2 _silkOrigin;
    private float _silkPullDuration;

    private bool TryBeginSilkEscape(Vector2 away, in CreatureContext context)
    {
        var heading = MathF.Atan2(away.Y, away.X);
        var range = context.Random.NextFloat(180, 260) * Scale;
        var best = Position;
        var bestScore = 0f;
        // Prefer straight away from the cursor; near a wall, try sideways escape routes.
        foreach (var angle in new[] { 0f, 0.55f, -0.55f, 1.1f, -1.1f, MathF.PI / 2, -MathF.PI / 2 })
        {
            var direction = new Vector2(MathF.Cos(heading + angle), MathF.Sin(heading + angle));
            var target = Position + direction * range;
            target = context.Layout?.ConstrainMove(Position, target) ?? context.Bounds.Clamp(target);
            var distance = Vector2.Distance(Position, target);
            var score = distance * (1 - MathF.Abs(angle) * 0.2f);
            if (distance >= 24 && score > bestScore) { best = target; bestScore = score; }
        }
        _silkCooldown = 1; // Retry later if the available display area is too small.
        if (bestScore == 0) return false;
        SilkAnchor = best;
        _silkOrigin = Position;
        _motionHeading = MathF.Atan2(best.Y - Position.Y, best.X - Position.X);
        _silkPullDuration = Math.Clamp(Vector2.Distance(Position, best) / (650 * MathF.Sqrt(Scale)), 0.28f, 0.8f);
        Velocity = Vector2.Zero;
        _pauseRemaining = RestingSeconds = 0;
        EnterMotion(LocomotionState.SilkCasting, 0.3f);
        return true;
    }

    private void UpdateSilkEscape(float dt, in CreatureContext context)
    {
        if (SilkAnchor is not { } anchor) return;
        // A changed screen layout can invalidate an anchor even if the spider remains on a screen.
        var reachable = context.Layout?.ConstrainMove(Position, anchor) ?? context.Bounds.Clamp(anchor);
        if (Vector2.DistanceSquared(reachable, anchor) > 0.01f) { EndSilkEscape(); return; }
        _motionElapsed = MathF.Min(_motionElapsed + dt, _motionDuration);
        MotionProgress = _motionElapsed / _motionDuration;
        Velocity = Vector2.Zero;
        if (MotionState == LocomotionState.SilkCasting) TurnToward(_motionHeading, 14 * dt);
        if (MotionState == LocomotionState.SilkPulling)
        {
            var next = Vector2.Lerp(_silkOrigin, anchor, SmoothStep(MotionProgress));
            var allowed = context.Layout?.ConstrainMove(Position, next) ?? context.Bounds.Clamp(next);
            Velocity = (allowed - Position) / dt;
            Position = allowed;
            if (Vector2.DistanceSquared(allowed, next) > 0.01f) { EndSilkEscape(); return; }
        }
        if (MotionProgress < 1) return;
        if (MotionState == LocomotionState.SilkCasting)
            EnterMotion(LocomotionState.SilkPulling, _silkPullDuration);
        else if (MotionState == LocomotionState.SilkPulling)
            EnterMotion(LocomotionState.SilkSettling, 0.25f);
        else EndSilkEscape();
    }

    private void EndSilkEscape()
    {
        SilkAnchor = null;
        MotionState = LocomotionState.Walking;
        MotionProgress = Elevation = WingSpread = RestingSeconds = 0;
        Velocity = Vector2.Zero;
        _pauseRemaining = _motionElapsed = 0;
        _heading = Rotation;
        _turnIn = 0.5f;
        _silkCooldown = 3;
    }
}
