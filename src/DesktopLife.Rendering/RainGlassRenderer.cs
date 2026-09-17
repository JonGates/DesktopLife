using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

public static class RainGlassRenderer
{
    private static readonly Pen[] TrailPens = Enumerable.Range(0, 24).Select(i => Pen(Color.FromArgb((byte)((i / 3 + 1) * 2), 193, 212, 220), .4 + i % 3 * .65)).ToArray();
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
            // Stable variation along the path, with occasional dry gaps; never random per frame.
            var variation = .5 + .5 * Math.Sin(trail.Start.Y * .17 + trail.Start.X * .11);
            if (variation < .13) continue;
            var opacity = Math.Max(0, 1 - (rain.Time - trail.Born) / 5.0) * (.35 + .65 * variation);
            var bucket = Math.Clamp((int)(opacity * 8), 0, 7);
            dc.DrawLine(TrailPens[bucket * 3 + Math.Clamp((int)(trail.Width * (.18 + .3 * variation)) - 1, 0, 2)], new(trail.Start.X, trail.Start.Y), new(trail.End.X, trail.End.Y));
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
