using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
namespace DesktopLife.Engine.Creatures;
public readonly record struct CreatureContext(MouseState Mouse, WorldBounds Bounds, float TotalTime, IRandomSource Random, IReadOnlyList<ICreature>? Neighbors = null);
