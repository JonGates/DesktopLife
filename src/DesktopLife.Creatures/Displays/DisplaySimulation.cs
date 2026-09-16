using System.Numerics;
using DesktopLife.Creatures.Cockroach;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Displays;

public sealed record DisplayWorld(DisplayArea Display, SimulationWorld World);

/// <summary>One desktop-wide population, projected through a viewport for each physical display.</summary>
public sealed class DisplaySimulation(int seed)
{
    public SimulationWorld World { get; } = new(new(0, 0, 1920, 1080), new RandomSource(seed), []);
    public DesktopLayout Layout { get; private set; } = new([]);
    public IReadOnlyList<DisplayWorld> Worlds { get; private set; } = [];
    public PopulationSettings Settings { get; private set; } = new();
    public int TotalFlyCount => 1;
    public int TotalCockroachCount { get; private set; } = 20;

    public int TotalAntCount { get; private set; } = 20;
    public int TotalCaterpillarCount { get; private set; } = 3;
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
        SetPopulation(Settings);
    }

    public void SetPopulation(PopulationSettings settings)
    {
        settings.Validate();
        var population = new List<ICreature>();
        Add(CreatureKind.Fly, 1, () => new FlyCreature(SpawnPoint(outside: true)));
        Add(CreatureKind.Cockroach, settings.Cockroaches, () => new CockroachCreature(SpawnPoint(), initiallyHidden: true, scale: World.Random.NextFloat(settings.RoachMin / 100f, settings.RoachMax / 100f)));
        Add(CreatureKind.Ant, settings.Ants, () => new CrawlingInsect(SpawnPoint(), CreatureKind.Ant, World.Random.NextFloat(settings.AntMin / 100f, settings.AntMax / 100f)));
        Add(CreatureKind.Caterpillar, settings.Caterpillars, () => new CrawlingInsect(SpawnPoint(), CreatureKind.Caterpillar, World.Random.NextFloat(settings.CaterpillarMin / 100f, settings.CaterpillarMax / 100f)));
        Resize(CreatureKind.Cockroach, Settings.RoachMin, Settings.RoachMax, settings.RoachMin, settings.RoachMax);
        Resize(CreatureKind.Ant, Settings.AntMin, Settings.AntMax, settings.AntMin, settings.AntMax);
        Resize(CreatureKind.Caterpillar, Settings.CaterpillarMin, Settings.CaterpillarMax, settings.CaterpillarMin, settings.CaterpillarMax);
        Settings = settings;
        World.Manager.Replace(population);
        TotalCockroachCount = settings.Cockroaches;
        TotalAntCount = settings.Ants;
        TotalCaterpillarCount = settings.Caterpillars;
        void Resize(CreatureKind kind, int oldMin, int oldMax, int min, int max)
        {
            if (oldMin == min && oldMax == max) return;
            foreach (var creature in World.Manager.Creatures.OfType<Creature>().Where(c => c.Kind == kind))
            {
                var fraction = oldMax == oldMin ? 0.5f : Math.Clamp((creature.Scale * 100 - oldMin) / (oldMax - oldMin), 0, 1);
                creature.SetScale((min + fraction * (max - min)) / 100f);
            }
        }
        void Add(CreatureKind kind, int count, Func<ICreature> create)
        {
            var retained = World.Manager.Creatures.Where(c => c.Kind == kind).Take(count).ToArray();
            population.AddRange(retained);
            for (var i = retained.Length; i < count; i++) population.Add(create());
        }
    }
    private Vector2 SpawnPoint(bool outside = false)
    {
        if (Layout.Edges.Count == 0) return World.Bounds.Center;
        var index = System.Math.Min((int)World.Random.NextFloat(0, Layout.Edges.Count), Layout.Edges.Count - 1);
        var edge = Layout.Edges[index];
        return Vector2.Lerp(edge.Start, edge.End, World.Random.NextFloat(0.1f, 0.9f)) + edge.Inward * (outside ? -0.01f : 1);
    }

    public void Update(float elapsedSeconds, Vector2 cursor, MouseClick? click = null)
    {
        if (Worlds.Count > 0) World.Update(elapsedSeconds, cursor, click);
    }

    public void ResetInput() => World.Mouse.Reset();
}
