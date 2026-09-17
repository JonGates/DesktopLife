using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

public static class RainGlassRenderer
{
    private static readonly Brush Water = Freeze(new RadialGradientBrush(new GradientStopCollection {
        new(Color.FromArgb(12, 210, 235, 250), .15), new(Color.FromArgb(55, 140, 178, 199), .72), new(Color.FromArgb(145, 235, 250, 255), .89), new(Color.FromArgb(100, 15, 36, 48), 1) }) { GradientOrigin = new(.32, .2) });
    private static readonly Pen Rim = Pen(Color.FromArgb(80, 10, 25, 38), .7);
    private static readonly Pen Shine = Pen(Color.FromArgb(220, 245, 253, 255), 1.15);
    private static readonly Pen CrackDark = Pen(Color.FromArgb(150, 10, 26, 40), 2.1);
    private static readonly Pen CrackLight = Pen(Color.FromArgb(225, 223, 246, 255), .8);
    private static readonly Brush Shard = Freeze(new SolidColorBrush(Color.FromArgb(35, 215, 241, 255)));
    private static readonly Pen[] TrailPens = Enumerable.Range(1, 10).Select(i => Pen(Color.FromArgb(22, 205, 239, 253), i)).ToArray();
    private static T Freeze<T>(T value) where T : Freezable { value.Freeze(); return value; }
    private static Pen Pen(Color color, double width) => Freeze(new Pen(new SolidColorBrush(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round });
    public static void Render(DrawingContext dc, RainGlass rain, WorldBounds viewport, double dpiX, double dpiY)
    {
        if (!rain.Enabled) return;
        dc.PushClip(new RectangleGeometry(new Rect(0, 0, viewport.Width / dpiX, viewport.Height / dpiY)));
        var matrix = Matrix.Identity; matrix.Translate(-viewport.Left, -viewport.Top); matrix.Scale(1 / dpiX, 1 / dpiY); dc.PushTransform(new MatrixTransform(matrix));
        foreach (var trail in rain.Trails)
        {
            var opacity = Math.Max(0, 1 - (rain.Time - trail.Born) / 2.5);
            dc.PushOpacity(opacity);
            dc.DrawLine(TrailPens[Math.Clamp((int)trail.Width - 1, 0, 9)], new(trail.Start.X, trail.Start.Y), new(trail.End.X, trail.End.Y)); dc.Pop();
        }
        foreach (var drop in rain.Drops)
        {
            var p = new Point(drop.Position.X, drop.Position.Y); var r = drop.Radius;
            var ry = r * (drop.Sliding ? 1.35 : 1.08);
            var age = rain.Time - drop.Born;
            if (age < .25f)
            {
                dc.PushOpacity((1 - age / .25) * .45);
                dc.DrawEllipse(null, Shine, p, r * (1 + age * 3), r * (1 + age * 3)); dc.Pop();
            }
            dc.DrawEllipse(Water, Rim, p, r, ry);
            dc.DrawEllipse(null, Shine, new Point(p.X - r * .25, p.Y - ry * .38), r * .28, r * .12);
            dc.DrawEllipse(Brushes.White, null, new Point(p.X + r * .3, p.Y + ry * .56), .65, .5);
        }
        foreach (var fracture in rain.Fractures)
        {
            var age = rain.Time - fracture.Born;
            dc.PushOpacity(Math.Clamp((4 - age) / 1.4, 0, 1));
            var random = new Random(fracture.Seed);
            var center = new Point(fracture.Center.X, fracture.Center.Y);
            var spokes = fracture.Style == 0 ? 9 : fracture.Style == 1 ? 15 : 11;
            var ends = new Point[spokes];
            var growth = Math.Min(1, age * 12 + .12);
            for (var i = 0; i < spokes; i++)
            {
                var angle = Math.PI * 2 * (i + random.NextDouble() * .55) / spokes;
                var length = (90 + random.NextDouble() * 180) * growth;
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
                    var fall = age * age * 55;
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
