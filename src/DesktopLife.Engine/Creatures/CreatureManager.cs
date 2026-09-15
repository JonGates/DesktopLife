namespace DesktopLife.Engine.Creatures;
public sealed class CreatureManager(IEnumerable<ICreature> creatures)
{
    public IReadOnlyList<ICreature> Creatures { get; } = Array.AsReadOnly(creatures.ToArray());
    public void Update(float dt, in CreatureContext context)
    {
        var sharedContext = context with { Neighbors = Creatures };
        for (var i = 0; i < Creatures.Count; i++) Creatures[i].Update(dt, in sharedContext);
    }
}
