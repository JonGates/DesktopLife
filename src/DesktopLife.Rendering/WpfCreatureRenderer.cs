using System.Numerics;
using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
namespace DesktopLife.Rendering;
public sealed class WpfCreatureRenderer : IRenderer
{
    private readonly DrawingGroup[] _frames = [FlySprite.Create(false), FlySprite.Create(true)];
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
            else dc.DrawDrawing(_frames[(int)(time * 36) % 2]);
            dc.Pop();
        }
    }
}
