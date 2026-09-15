using System.Numerics;
namespace DesktopLife.Engine.Creatures;
public interface ICreature
{
    CreatureKind Kind { get; }
    Guid Id { get; }
    Vector2 Position { get; }
    Vector2 Velocity { get; }
    float Rotation { get; }
    float Scale { get; }
    bool IsVisible { get; }
    void Update(float deltaTime, in CreatureContext context);
}
