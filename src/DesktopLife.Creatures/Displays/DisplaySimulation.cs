using System.Numerics;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;

namespace DesktopLife.Creatures.Displays;

public sealed record DisplayWorld(DisplayArea Display, SimulationWorld World);

public sealed class DisplaySimulation(int seed)
{
    public const int FliesPerDisplay = 1;
    public const int CockroachesPerDisplay = 20;
    public IReadOnlyList<DisplayWorld> Worlds { get; private set; } = [];
    public int TotalFlyCount => Worlds.Count * FliesPerDisplay;
    public int TotalCockroachCount => Worlds.Count * CockroachesPerDisplay;
    public int Seed { get; } = seed;
    private int _generation;

    public void Synchronize(IReadOnlyList<DisplayArea> displays)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var display in displays)
        {
            var b = display.Bounds;
            if (string.IsNullOrWhiteSpace(display.Id) || !ids.Add(display.Id) ||
                !float.IsFinite(b.Left) || !float.IsFinite(b.Top) || !float.IsFinite(b.Right) || !float.IsFinite(b.Bottom) ||
                b.Width < 32 || b.Height < 32)
                throw new ArgumentException("Displays must have unique IDs and finite bounds of at least 32 pixels.", nameof(displays));
        }
        var previous = Worlds.ToDictionary(w => w.Display.Id, StringComparer.Ordinal);
        var next = new DisplayWorld[displays.Count];
        for (var i = 0; i < displays.Count; i++)
        {
            var display = displays[i];
            if (previous.TryGetValue(display.Id, out var existing) && existing.Display.Bounds == display.Bounds)
                next[i] = new(display, existing.World);
            else
            {
                var random = new RandomSource(unchecked(Seed + ++_generation * 7919));
                next[i] = new(display, new SimulationWorld(display.Bounds, random,
                    DesktopPopulation.Create(display.Bounds, random, CockroachesPerDisplay)));
            }
        }
        Worlds = Array.AsReadOnly(next);
    }

    public void Update(float elapsedSeconds, Vector2 cursor)
    {
        foreach (var item in Worlds) item.World.Update(elapsedSeconds, cursor);
    }

    public void ResetInput()
    {
        foreach (var item in Worlds) item.World.Mouse.Reset();
    }
}
