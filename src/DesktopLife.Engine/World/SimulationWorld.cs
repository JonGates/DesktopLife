using System.Numerics;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
namespace DesktopLife.Engine.World;
public sealed class SimulationWorld(WorldBounds bounds, IRandomSource random, IEnumerable<ICreature> creatures)
{
    public MouseTracker Mouse { get; } = new();
    public CreatureManager Manager { get; } = new(creatures);
    public WorldBounds Bounds { get; set; } = bounds;
    public IRandomSource Random { get; } = random;
    public float TotalTime { get; private set; }
    public void Update(float elapsedSeconds, Vector2 cursor)
    {
        if (!float.IsFinite(elapsedSeconds) || elapsedSeconds <= 0) return;
        Mouse.Update(cursor, elapsedSeconds);
        var dt = MathF.Min(elapsedSeconds, 0.05f);
        TotalTime += dt;
        var context = new CreatureContext(Mouse.State, Bounds, TotalTime, Random);
        Manager.Update(dt, in context);
    }
}
