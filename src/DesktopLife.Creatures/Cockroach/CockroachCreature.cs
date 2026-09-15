using System.Numerics;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Creatures.Cockroach;

public sealed class CockroachCreature : Creature
{
    public override CreatureKind Kind => CreatureKind.Cockroach;
    private CockroachBrain _brain;
    private readonly CockroachOptions _options;
    public CockroachState State => _brain.State;

    public CockroachCreature(Vector2 position, bool initiallyHidden = false, CockroachOptions? options = null)
    {
        Position = position;
        _options = options ?? new CockroachOptions();
        _brain = new(_options, initiallyHidden);
        IsVisible = !initiallyHidden;
    }

    public override void Relocate(Vector2 position)
    {
        base.Relocate(position);
        _brain = new(_options, !IsVisible);
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
