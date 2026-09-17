using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

public static class RainGlassRenderer
{
    private static readonly Pen CrackDark = Pen(Color.FromArgb(150, 10, 26, 40), 2.1);
    private static readonly Pen CrackLight = Pen(Color.FromArgb(225, 223, 246, 255), .8);
    private static readonly Brush Shard = Freeze(new SolidColorBrush(Color.FromArgb(35, 215, 241, 255)));
    private static readonly Pen[] TrailPens = Enumerable.Range(0, 24).Select(i => Pen(Color.FromArgb((byte)((i / 3 + 1) * 3), 205, 239, 253), i % 3 + 1)).ToArray();
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
            var opacity = Math.Max(0, 1 - (rain.Time - trail.Born) / 5.0);
            var bucket = Math.Clamp((int)(opacity * 8), 0, 7);
            dc.DrawLine(TrailPens[bucket * 3 + Math.Clamp((int)(trail.Width * .35) - 1, 0, 2)], new(trail.Start.X, trail.Start.Y), new(trail.End.X, trail.End.Y));
        }
        foreach (var drop in rain.Drops)
        {
            if (!viewport.Contains(drop.Position, drop.Radius * 3)) continue;
            RainDropOptics.Draw(dc, drop, rain.Time, viewport, backgroundImage);
        }
        foreach (var fracture in rain.Fractures)
        {
            if (!viewport.Contains(fracture.Center, 1200)) continue;
            var age = rain.Time - fracture.Born;
            dc.PushOpacity(Math.Clamp((3 - age) / .7, 0, 1));
            var random = new Random(fracture.Seed);
            var center = new Point(fracture.Center.X, fracture.Center.Y);
            var spokes = fracture.Style == 0 ? 9 : fracture.Style == 1 ? 15 : 11;
            var ends = new Point[spokes];
            var growth = Math.Min(1, age * 12 + .12);
            for (var i = 0; i < spokes; i++)
            {
                var angle = Math.PI * 2 * (i + random.NextDouble() * .55) / spokes;
                var length = (40 + random.NextDouble() * 80) * growth;
                ends[i] = new(center.X + Math.Cos(angle) * length, center.Y + Math.Sin(angle) * length);
                var previous = center;
                for (var step = 1; step <= 5; step++)
                {
                    var t = step / 5.0;
                    var next = new Point(center.X + (ends[i].X - center.X) * t + (random.NextDouble() - .5) * 12, center.Y + (ends[i].Y - center.Y) * t + (random.NextDouble() - .5) * 12);
                    dc.DrawLine(CrackDark, previous, next); dc.DrawLine(CrackLight, previous, next);
                    if (step == 3) dc.DrawLine(CrackLight, next, new(next.X + Math.Cos(angle + .7) * length * .2, next.Y + Math.Sin(angle + .7) * length * .2));
                    previous = next;
                }
            }
            for (var i = 0; i < spokes; i++)
            {
                var next = ends[(i + 1) % spokes];
                if (fracture.Style == 1)
                    for (var ring = 1; ring <= 3; ring++)
                    {
                        var t = ring * .2;
                        dc.DrawLine(CrackLight, new(center.X + (ends[i].X - center.X) * t, center.Y + (ends[i].Y - center.Y) * t), new(center.X + (next.X - center.X) * t, center.Y + (next.Y - center.Y) * t));
                    }
                if (fracture.Style == 2)
                {
                    var fall = age * age * 12;
                    var geometry = new StreamGeometry();
                    using (var context = geometry.Open())
                    {
                        context.BeginFigure(new(center.X, center.Y + fall), true, true);
                        context.LineTo(new(ends[i].X, ends[i].Y + fall), true, false);
                        context.LineTo(new(next.X, next.Y + fall), true, false);
                    }
                    geometry.Freeze(); dc.DrawGeometry(Shard, CrackLight, geometry);
                }
            }
            dc.Pop();
        }
        dc.Pop(); dc.Pop();
    }
}
