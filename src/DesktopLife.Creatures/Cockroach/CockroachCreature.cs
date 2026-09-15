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
        if (!float.IsFinite(deltaTime) || deltaTime <= 0) return;
        var previous = Position;
        var motion = _brain.Update(this, deltaTime, in context);
        Position = motion.Position;
        Velocity = motion.Velocity;
        AdvanceGait(previous, 20);
        if (Velocity.LengthSquared() > 1)
        {
            var turn = MathF.IEEERemainder(MathF.Atan2(Velocity.Y, Velocity.X) - Rotation, MathF.Tau);
            Rotation += Math.Clamp(turn, -12 * MathF.Min(deltaTime, 0.05f), 12 * MathF.Min(deltaTime, 0.05f));
        }
        IsVisible = State != CockroachState.Hidden;
    }
}
