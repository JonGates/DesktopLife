using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

public static class RainGlassRenderer
{
    private static readonly Pen[] TrailPens = Enumerable.Range(0, 96).Select(i => Pen(Color.FromArgb((byte)((i / 12 + 1) * 2), 20, 40, 48), .5 + i % 12)).ToArray();
    private static readonly Pen[] TrailEdges = Enumerable.Range(0, 8).Select(i => Pen(Color.FromArgb((byte)((i + 1) * 3), 210, 231, 238), .55)).ToArray();
    private static T Freeze<T>(T value) where T : Freezable { value.Freeze(); return value; }
    private static Pen Pen(Color color, double width) => Freeze(new Pen(new SolidColorBrush(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round });
    public static void Render(DrawingContext dc, RainGlass rain, WorldBounds viewport, double dpiX, double dpiY, ImageSource? backgroundImage = null)
    {
        if (!rain.Enabled) return;
        dc.PushClip(new RectangleGeometry(new Rect(0, 0, viewport.Width / dpiX, viewport.Height / dpiY)));
        var matrix = Matrix.Identity; matrix.Translate(-viewport.Left, -viewport.Top); matrix.Scale(1 / dpiX, 1 / dpiY); dc.PushTransform(new MatrixTransform(matrix));
        foreach (var trail in rain.Trails)
        {
            if (!viewport.Contains(trail.Start, 50) && !viewport.Contains(trail.End, 50)) continue;
            var life = Math.Clamp(1 - (rain.Time - trail.Born) / 5.0, 0, 1);
            var variation = .5 + .5 * Math.Sin(trail.Start.Y * .17 + trail.Start.X * .11);
            // Fresh broad wakes stay connected; older narrow remnants can break apart.
            if (life < .6 && trail.Width < 4 && variation < .13) continue;
            var width = trail.Width * (.45 + .55 * life) * (.8 + .2 * variation);
            var opacity = life * (.65 + .35 * variation);
            var bucket = Math.Clamp((int)(opacity * 8), 0, 7);
            var start = new Point(trail.Start.X, trail.Start.Y); var end = new Point(trail.End.X, trail.End.Y);
            var pen = TrailPens[bucket * 12 + Math.Clamp((int)Math.Round(width - .5), 0, 11)];
            dc.DrawLine(pen, start, end);
            var direction = end - start;
            if (direction.LengthSquared > .01)
            {
                direction.Normalize();
                var edgeOffset = new Vector(-direction.Y, direction.X) * pen.Thickness * .42;
                dc.DrawLine(TrailEdges[bucket], start + edgeOffset, end + edgeOffset);
            }
        }
        foreach (var drop in rain.Drops)
        {
            if (!viewport.Contains(drop.Position, drop.Radius * 3)) continue;
            RainDropOptics.Draw(dc, drop, rain.Time, viewport, backgroundImage);
        }
        foreach (var fracture in rain.Fractures)
        {
            if (!viewport.Contains(fracture.Center, 1200)) continue;
            GlassFractureSprite.Draw(dc, fracture, rain.Time - fracture.Born);
        }
        dc.Pop(); dc.Pop();
    }
}
