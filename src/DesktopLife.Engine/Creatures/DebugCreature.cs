using System.Numerics;
namespace DesktopLife.Engine.Creatures;
public sealed class DebugCreature : Creature
{
    public DebugCreature(Vector2 position) { Position = position; Velocity = new(90, 45); }
    public override void Update(float deltaTime, in CreatureContext context)
    {
        Position += Velocity * deltaTime;
        if (Position.X < context.Bounds.Left || Position.X > context.Bounds.Right) Velocity = new(-Velocity.X, Velocity.Y);
        if (Position.Y < context.Bounds.Top || Position.Y > context.Bounds.Bottom) Velocity = new(Velocity.X, -Velocity.Y);
        Position = context.Bounds.Clamp(Position);
        Rotation = MathF.Atan2(Velocity.Y, Velocity.X);
    }
}
