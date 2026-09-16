using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
using System.Numerics;
using DesktopLife.Creatures.Cockroach;
using DesktopLife.Creatures.Fly;

namespace DesktopLife.Creatures;

public static class DesktopPopulation
{
    public static IReadOnlyList<ICreature> Create(WorldBounds bounds, IRandomSource random, int cockroachCount = 20)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cockroachCount);
        var population = new ICreature[cockroachCount + 1];
        population[0] = new FlyCreature(new(bounds.Left - 90, bounds.Center.Y));
        for (var i = 0; i < cockroachCount; i++)
        {
            var along = random.NextFloat(0.05f, 0.95f);
            var position = (i % 4) switch
            {
                0 => new Vector2(bounds.Left - 12, bounds.Top + bounds.Height * along),
                1 => new Vector2(bounds.Right + 12, bounds.Top + bounds.Height * along),
                2 => new Vector2(bounds.Left + bounds.Width * along, bounds.Top - 12),
                _ => new Vector2(bounds.Left + bounds.Width * along, bounds.Bottom + 12)
            };
            population[i + 1] = new CockroachCreature(position, initiallyHidden: true, scale: random.NextFloat(0.6f, 1.8f));
        }
        return Array.AsReadOnly(population);
    }
}
