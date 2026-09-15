using System.Numerics;
using DesktopLife.Creatures.Cockroach;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Displays;

public sealed record DisplayWorld(DisplayArea Display, SimulationWorld World);

/// <summary>One desktop-wide population, projected through a viewport for each physical display.</summary>
public sealed class DisplaySimulation(int seed)
{
    public SimulationWorld World { get; } = new(new(0, 0, 1920, 1080), new RandomSource(seed), []);
    public DesktopLayout Layout { get; private set; } = new([]);
    public IReadOnlyList<DisplayWorld> Worlds { get; private set; } = [];
    public int TotalFlyCount { get; private set; } = 1;
    public int TotalCockroachCount { get; private set; } = 20;

    public void Synchronize(IReadOnlyList<DisplayArea> displays)
    {
        var next = new DesktopLayout(displays); // Validate before mutating live state.
        var changed = Layout.Displays.Count != next.Displays.Count ||
            next.Displays.Any(d => !Layout.Displays.Any(old => old.Id == d.Id && old.Bounds == d.Bounds));
        Layout = next;
        World.Layout = next;
        World.Bounds = next.Bounds;
        Worlds = Array.AsReadOnly(next.Displays.Select(d => new DisplayWorld(d, World)).ToArray());
        if (changed && displays.Count > 0)
        {
            foreach (var creature in World.Manager.Creatures.OfType<Creature>())
                if (!next.Contains(creature.Position)) creature.Relocate(next.Clamp(creature.Position));
            World.Mouse.Reset();
        }
        SetPopulation(TotalFlyCount, TotalCockroachCount);
    }

    public void SetPopulation(int flies, int cockroaches)
    {
        new PopulationSettings(flies, cockroaches).Validate();
        var population = new List<ICreature>(flies + cockroaches);
        population.AddRange(World.Manager.Creatures.Where(c => c.Kind == CreatureKind.Fly).Take(flies));
        while (population.Count < flies) population.Add(new FlyCreature(SpawnPoint(outside: true)));
        population.AddRange(World.Manager.Creatures.Where(c => c.Kind == CreatureKind.Cockroach).Take(cockroaches));
        while (population.Count < flies + cockroaches) population.Add(new CockroachCreature(SpawnPoint(), initiallyHidden: true));
        World.Manager.Replace(population);
        TotalFlyCount = flies;
        TotalCockroachCount = cockroaches;
    }

    private Vector2 SpawnPoint(bool outside = false)
    {
        if (Layout.Edges.Count == 0) return World.Bounds.Center;
        var index = System.Math.Min((int)World.Random.NextFloat(0, Layout.Edges.Count), Layout.Edges.Count - 1);
        var edge = Layout.Edges[index];
        return Vector2.Lerp(edge.Start, edge.End, World.Random.NextFloat(0.1f, 0.9f)) + edge.Inward * (outside ? -0.01f : 1);
    }

    public void Update(float elapsedSeconds, Vector2 cursor)
    {
        if (Worlds.Count > 0) World.Update(elapsedSeconds, cursor);
    }

    public void ResetInput() => World.Mouse.Reset();
}
