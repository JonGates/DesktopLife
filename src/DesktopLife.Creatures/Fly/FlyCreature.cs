using System.Numerics;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Creatures.Fly;
public sealed class FlyCreature : Creature
{
    public override CreatureKind Kind => CreatureKind.Fly;
    private readonly FlyBrain _brain;
    public FlyState State => _brain.State;
    public override bool IsResting => State == FlyState.Landed;
    public FlyCreature(Vector2 position, FlyOptions? options = null)
    {
        Position = position;
        IsVisible = false;
        _brain = new(options ?? new FlyOptions());
    }
    public override void Update(float deltaTime, in CreatureContext context)
    {
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        var wasResting = IsResting;
        var motion = _brain.Update(Position, Velocity, deltaTime, in context);
        RestingSeconds = IsResting ? (wasResting ? RestingSeconds + MathF.Min(deltaTime, 0.05f) : 0) : 0;
        AnimationPhase = (AnimationPhase + MathF.Min(deltaTime, 0.05f) * (53f / 8)) % 1;
        Position = motion.Position;
        Velocity = motion.Velocity;
        if (Velocity.LengthSquared() > 35 * 35 && float.IsFinite(deltaTime) && deltaTime > 0)
        {
            var heading = MathF.Atan2(Velocity.Y, Velocity.X);
            var turn = MathF.IEEERemainder(heading - Rotation, MathF.Tau);
            var maxTurn = 20 * MathF.Min(deltaTime, 0.05f);
            Rotation += Math.Clamp(turn, -maxTurn, maxTurn);
        }
        IsVisible = State != FlyState.Offscreen;
    }
}
