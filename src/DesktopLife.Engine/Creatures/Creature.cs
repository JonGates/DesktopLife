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
    public virtual void Relocate(Vector2 position) { Position = position; Velocity = Vector2.Zero; }
    public abstract void Update(float deltaTime, in CreatureContext context);
}
