using System.Numerics;
using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
namespace DesktopLife.Rendering;
public sealed class WpfCreatureRenderer : IRenderer
{
    private static readonly DrawingGroup[] FlyingFly = Enumerable.Range(0, 8).Select(i => FlySprite.Create(false, i)).ToArray();
    private static readonly DrawingGroup[] RestingFly = Enumerable.Range(0, 8).Select(i => FlySprite.Create(true, i)).ToArray();
    private static readonly DrawingGroup[] Ants = Enumerable.Range(0, 8).Select(i => SmallInsectSprite.Create(false, i)).ToArray();
    private static readonly DrawingGroup[] Caterpillars = Enumerable.Range(0, 8).Select(i => SmallInsectSprite.Create(true, i)).ToArray();
    private readonly DrawingGroup[] _roachFrames = [CockroachSprite.Create(false), CockroachSprite.Create(true)];
    public void Render(DrawingContext dc, IReadOnlyList<ICreature> creatures, WorldBounds bounds, float time, double scaleX, double scaleY)
    {
        for (var i = 0; i < creatures.Count; i++)
        {
            var creature = creatures[i];
            if (!creature.IsVisible || !bounds.Contains(creature.Position, 40)) continue;
            var p = ScreenCoordinates.ToLocal(creature.Position, new Vector2(bounds.Left, bounds.Top), (float)scaleX, (float)scaleY);
            var matrix = Matrix.Identity;
            matrix.Scale(creature.Scale, creature.Scale);
            matrix.Rotate(creature.Rotation * 180 / Math.PI);
            matrix.Scale(1 / scaleX, 1 / scaleY);
            matrix.Translate(p.X, p.Y);
            var transform = new MatrixTransform(matrix);
            transform.Freeze();
            dc.PushTransform(transform);
            if (creature.Kind == CreatureKind.Cockroach)
            {
                var step = (int)(time * 10 + creature.Position.X * 0.03f + creature.Position.Y * 0.02f);
                dc.DrawDrawing(_roachFrames[step & 1]);
            }
                        else
            {
                var frame = (int)(time * (creature.IsResting ? 18 : creature.Kind == CreatureKind.Fly ? 53 : 12)) & 7;
                dc.DrawDrawing(creature.Kind switch
                {
                    CreatureKind.Ant => Ants[frame],
                    CreatureKind.Caterpillar => Caterpillars[frame],
                    _ => creature.IsResting ? RestingFly[frame] : FlyingFly[frame]
                });
            }
            dc.Pop();
        }
    }
}
