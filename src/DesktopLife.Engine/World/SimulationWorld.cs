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
    public DesktopLayout? Layout { get; set; }
    public IRandomSource Random { get; } = random;
    public float TotalTime { get; private set; }
    public void Update(float elapsedSeconds, Vector2 cursor, MouseClick? click = null)
    {
        if (!float.IsFinite(elapsedSeconds) || elapsedSeconds <= 0) return;
        Mouse.Update(cursor, elapsedSeconds, click);
        var dt = MathF.Min(elapsedSeconds, 0.05f);
        TotalTime += dt;
        var context = new CreatureContext(Mouse.State, Bounds, TotalTime, Random, Layout: Layout, ElapsedSeconds: elapsedSeconds);
        Manager.Update(dt, in context);
    }
}
