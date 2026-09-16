using System.Numerics;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Creatures.Displays;

/// <summary>Insect locomotion shares desktop boundaries; touching screen edges are traversable.</summary>
public sealed partial class CrawlingInsect : Creature
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
    private float _motionIn = -1;
    private float _jumpCooldown;
    private float _motionElapsed;
    private float _motionDuration;
    private float _jumpDuration;
    private float _jumpHeight;
    private float _motionSpeed;
    private float _entrySpeed;
    private float _motionHeading;
    private float _flightTurnIn;
    private bool CanJump => Kind is CreatureKind.Cricket or CreatureKind.Grasshopper;
    public override bool IsResting => MotionState == LocomotionState.Walking && _pauseRemaining > 0;
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
            CreatureKind.Spider => (6f, 0.45f, 1.4f, 1.1f, true),
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
        if (Kind == CreatureKind.Spider)
        {
            if (SilkAnchor != null) { UpdateSilkEscape(deltaTime, context); return; }
            _silkCooldown = MathF.Max(0, _silkCooldown - deltaTime);
            if (threatened && _silkCooldown <= 0 && TryBeginSilkEscape(away, context))
            { UpdateSilkEscape(deltaTime, context); return; }
        }
        if (CanJump || Kind == CreatureKind.Ladybug)
        {
            if (_motionIn < 0) ScheduleMotion(context);
            if (MotionState != LocomotionState.Walking)
            {
                UpdateMotion(deltaTime, context);
                return;
            }
            _motionIn -= deltaTime;
            _jumpCooldown = MathF.Max(0, _jumpCooldown - deltaTime);
            if (_motionIn <= 0 || (CanJump && threatened && _jumpCooldown <= 0))
            {
                BeginMotion(context, threatened, away);
                UpdateMotion(deltaTime, context);
                return;
            }
        }
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

    private void ScheduleMotion(in CreatureContext context) =>
        _motionIn = CanJump ? context.Random.NextFloat(3, 7) : context.Random.NextFloat(5, 12);

    private void BeginMotion(in CreatureContext context, bool threatened, Vector2 away)
    {
        _pauseRemaining = 0;
        RestingSeconds = 0;
        _entrySpeed = Velocity.Length();
        _motionHeading = threatened ? MathF.Atan2(away.Y, away.X) : Rotation;
        if (CanJump)
        {
            var distance = Kind == CreatureKind.Cricket ? context.Random.NextFloat(160, 260) : context.Random.NextFloat(300, 450);
            _jumpDuration = context.Random.NextFloat(0.45f, 0.65f);
            // Choose the range at takeoff; changing size midair must not alter the planned landing.
            _motionSpeed = distance * Scale * (threatened ? 1.3f : 1) / _jumpDuration;
            _jumpHeight = (Kind == CreatureKind.Cricket ? 32 : 48) * (threatened ? 1.2f : 1);
            EnterMotion(LocomotionState.JumpPreparing, context.Random.NextFloat(0.15f, 0.22f));
        }
        else
        {
            _motionSpeed = context.Random.NextFloat(80, 110);
            EnterMotion(LocomotionState.TakingOff, 0.35f);
        }
    }

    private void EnterMotion(LocomotionState state, float duration)
    {
        MotionState = state;
        _motionElapsed = 0;
        _motionDuration = duration;
        MotionProgress = 0;
    }

    private void UpdateMotion(float dt, in CreatureContext context)
    {
        var movementTime = MotionState == LocomotionState.Jumping ? MathF.Min(dt, _motionDuration - _motionElapsed) : dt;
        _motionElapsed = MathF.Min(_motionElapsed + dt, _motionDuration);
        MotionProgress = _motionElapsed / _motionDuration;
        var t = MotionProgress;
        var eased = t * t * (3 - 2 * t);
        float speed;
        switch (MotionState)
        {
            case LocomotionState.JumpPreparing:
                TurnToward(_motionHeading, 12 * dt);
                speed = _entrySpeed * (1 - eased);
                break;
            case LocomotionState.Jumping:
                Elevation = 4 * _jumpHeight * t * (1 - t);
                speed = _motionSpeed;
                break;
            case LocomotionState.JumpLanding:
                Elevation = 0;
                speed = _speed * (0.4f + 0.6f * eased);
                break;
            case LocomotionState.TakingOff:
                WingSpread = SmoothStep(t / 0.4f);
                var lift = SmoothStep((t - 0.4f) / 0.6f);
                Elevation = 12 * lift;
                speed = _entrySpeed * (1 - WingSpread) + _motionSpeed * lift;
                TurnToward(_motionHeading, 1.6f * dt);
                break;
            case LocomotionState.Flying:
                _flightTurnIn -= dt;
                if (_flightTurnIn <= 0)
                {
                    _motionHeading = Rotation + context.Random.NextFloat(-0.5f, 0.5f);
                    _flightTurnIn = context.Random.NextFloat(0.8f, 1.6f);
                }
                TurnToward(_motionHeading, 1.6f * dt);
                Elevation = 12;
                WingSpread = 1;
                speed = _motionSpeed;
                break;
            default: // Ladybug landing.
                Elevation = 12 * (1 - SmoothStep(t / 0.65f));
                WingSpread = 1 - SmoothStep((t - 0.65f) / 0.35f);
                speed = _motionSpeed + (_speed - _motionSpeed) * eased;
                TurnToward(_motionHeading, 1.6f * dt);
                break;
        }
        var next = Position + new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation)) * speed * movementTime;
        var allowed = context.Layout?.ConstrainMove(Position, next) ?? context.Bounds.Clamp(next);
        Velocity = (allowed - Position) / dt;
        Position = allowed;
        if (Vector2.DistanceSquared(next, allowed) > 0.0001f)
        {
            var inward = context.Layout?.NearestEdge(Position).Inward;
            _motionHeading = inward.HasValue ? MathF.Atan2(inward.Value.Y, inward.Value.X) : Rotation + MathF.PI;
            _flightTurnIn = 0.5f;
        }
        // Airborne distance does not advance the walking gait.
        if (t < 1) return;
        switch (MotionState)
        {
            case LocomotionState.JumpPreparing:
                EnterMotion(LocomotionState.Jumping, _jumpDuration);
                break;
            case LocomotionState.Jumping:
                EnterMotion(LocomotionState.JumpLanding, 0.15f);
                break;
            case LocomotionState.TakingOff:
                EnterMotion(LocomotionState.Flying, context.Random.NextFloat(2, 4));
                _flightTurnIn = 0;
                break;
            case LocomotionState.Flying:
                EnterMotion(LocomotionState.Landing, 0.35f);
                break;
            default:
                MotionState = LocomotionState.Walking;
                MotionProgress = Elevation = WingSpread = 0;
                _heading = _motionHeading;
                _turnIn = 0.5f;
                _jumpCooldown = 2.5f;
                ScheduleMotion(context);
                break;
        }
    }

    private void TurnToward(float heading, float maximum) =>
        Rotation += Math.Clamp(MathF.IEEERemainder(heading - Rotation, MathF.Tau), -maximum, maximum);

    private static float SmoothStep(float progress)
    {
        var t = Math.Clamp(progress, 0, 1);
        return t * t * (3 - 2 * t);
    }

    public override void Relocate(Vector2 position)
    {
        base.Relocate(position);
        if (Kind == CreatureKind.Spider) { EndSilkEscape(); return; }
        if (!CanJump && Kind != CreatureKind.Ladybug) return;
        MotionState = LocomotionState.Walking;
        MotionProgress = Elevation = WingSpread = RestingSeconds = 0;
        _motionElapsed = _pauseRemaining = 0;
        _motionIn = -1;
        _jumpCooldown = 2.5f;
        _heading = _motionHeading = Rotation;
        _turnIn = 0.5f;
    }
}

