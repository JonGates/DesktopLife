using System.Numerics;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Creatures.Cockroach;

public sealed class CockroachCreature : Creature
{
    public override CreatureKind Kind => CreatureKind.Cockroach;
    private readonly CockroachBrain _brain;
    public CockroachState State => _brain.State;

    public CockroachCreature(Vector2 position, bool initiallyHidden = false, CockroachOptions? options = null)
    {
        Position = position;
        _brain = new(options ?? new CockroachOptions(), initiallyHidden);
        IsVisible = !initiallyHidden;
    }

    public override void Update(float deltaTime, in CreatureContext context)
    {
        var motion = _brain.Update(this, deltaTime, in context);
        Position = motion.Position;
        Velocity = motion.Velocity;
        if (Velocity.LengthSquared() > 1) Rotation = MathF.Atan2(Velocity.Y, Velocity.X);
        IsVisible = State != CockroachState.Hidden;
    }
}
