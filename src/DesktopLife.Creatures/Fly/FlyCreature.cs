using System.Numerics;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Creatures.Fly;
public sealed class FlyCreature : Creature
{
    public override CreatureKind Kind => CreatureKind.Fly;
    private readonly FlyBrain _brain;
    public FlyState State => _brain.State;
    public FlyCreature(Vector2 position, FlyOptions? options = null)
    {
        Position = position;
        IsVisible = false;
        _brain = new(options ?? new FlyOptions());
    }
    public override void Update(float deltaTime, in CreatureContext context)
    {
        var motion = _brain.Update(Position, Velocity, deltaTime, in context);
        Position = motion.Position;
        Velocity = motion.Velocity;
        if (Velocity.LengthSquared() > 1) Rotation = MathF.Atan2(Velocity.Y, Velocity.X);
        IsVisible = State != FlyState.Offscreen;
    }
}
