using System.Numerics;
namespace DesktopLife.Engine.Creatures;
public abstract class Creature : ICreature
{
    public virtual CreatureKind Kind => CreatureKind.Debug;
    public Guid Id { get; } = Guid.NewGuid();
    public Vector2 Position { get; protected set; }
    public Vector2 Velocity { get; protected set; }
    public float Rotation { get; protected set; }
    public float Scale { get; protected set; } = 1;
    public bool IsVisible { get; protected set; } = true;
    public virtual bool IsResting => false;
    public float AnimationPhase { get; protected set; }
    public float RestingSeconds { get; protected set; }
    public virtual LocomotionState MotionState { get; protected set; } = LocomotionState.Walking;
    public virtual float MotionProgress { get; protected set; }
    /// <summary>Height in logical pixels; renderers apply Scale once.</summary>
    public virtual float Elevation { get; protected set; }
    public virtual float WingSpread { get; protected set; }
    protected void AdvanceGait(Vector2 previous, float stride) =>
        AnimationPhase = (AnimationPhase + Vector2.Distance(previous, Position) / stride) % 1;
    public void SetScale(float scale)
    {
        if (!float.IsFinite(scale) || scale < 0.1f || scale > 3f) throw new ArgumentOutOfRangeException(nameof(scale));
        Scale = scale;
    }
    public virtual void Relocate(Vector2 position) { Position = position; Velocity = Vector2.Zero; }
    public abstract void Update(float deltaTime, in CreatureContext context);
}
