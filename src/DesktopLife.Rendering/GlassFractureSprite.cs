using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

internal static class GlassFractureSprite
{
    private static readonly ConditionalWeakTable<GlassFracture, BitmapSource> Cache = new();
    public static void Draw(DrawingContext dc, GlassFracture fracture, float age)
    {
        dc.PushOpacity(Math.Clamp((3 - age) / .7, 0, 1));
        dc.DrawImage(Cache.GetValue(fracture, Build), new Rect(fracture.Center.X - 144, fracture.Center.Y - 144, 288, 288));
        dc.Pop();
    }
    private static BitmapSource Build(GlassFracture fracture)
    {
        var random = new Random(fracture.Seed);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.PushTransform(new TranslateTransform(144, 144));
            var dark = new Pen(new SolidColorBrush(Color.FromArgb(145, 12, 19, 24)), 1.15); dark.Freeze();
            var bright = new Pen(new SolidColorBrush(Color.FromArgb(190, 233, 242, 244)), .5); bright.Freeze();
            var fine = new Pen(new SolidColorBrush(Color.FromArgb(100, 229, 241, 245)), .35); fine.Freeze();
            var arms = fracture.Style == 1 ? 11 : fracture.Style == 2 ? 7 : 5;
            var paths = new List<Point[]>();
            var offset = random.NextDouble() * Math.Tau;
            for (var arm = 0; arm < arms; arm++)
            {
                var angle = offset + (arm + random.NextDouble() * .7) * Math.Tau / arms;
                var length = 42 + random.NextDouble() * 78;
                var points = new Point[9];
                points[0] = new((random.NextDouble() - .5) * 6, (random.NextDouble() - .5) * 6);
                for (var step = 1; step < points.Length; step++)
                {
                    var distance = step / 8.0 * length;
                    var wander = (random.NextDouble() - .5) * (2 + step * 1.2);
                    points[step] = new(Math.Cos(angle) * distance - Math.Sin(angle) * wander, Math.Sin(angle) * distance + Math.Cos(angle) * wander);
                    if (step < 5) dc.DrawLine(dark, points[step - 1], points[step]);
                    dc.DrawLine(step < 5 ? bright : fine, points[step - 1], points[step]);
                    if (step is 3 or 5 or 7)
                    {
                        var branch = angle + (random.NextDouble() > .5 ? 1 : -1) * (.35 + random.NextDouble() * .6);
                        var current = points[step];
                        for (var b = 0; b < 3; b++)
                        {
                            var next = new Point(current.X + Math.Cos(branch) * (4 + random.NextDouble() * 8), current.Y + Math.Sin(branch) * (4 + random.NextDouble() * 8));
                            dc.DrawLine(fine, current, next); current = next; branch += (random.NextDouble() - .5) * .6;
                        }
                    }
                }
                paths.Add(points);
            }
            // Uneven, incomplete cross-fractures: never concentric regular polygons.
            for (var i = 0; i < arms; i++)
                for (var ring = 1; ring <= (fracture.Style == 1 ? 5 : 2); ring++)
                {
                    if (random.NextDouble() < .25) continue;
                    var a = paths[i][ring]; var b = paths[(i + 1) % arms][Math.Clamp(ring + random.Next(-1, 2), 1, 7)];
                    var mid = new Point((a.X + b.X) * .5 + (random.NextDouble() - .5) * 5, (a.Y + b.Y) * .5 + (random.NextDouble() - .5) * 5);
                    dc.DrawLine(fine, a, mid); dc.DrawLine(fine, mid, b);
                }
            for (var i = 0; i < (fracture.Style == 2 ? 65 : 28); i++)
            {
                var angle = random.NextDouble() * Math.Tau;
                var distance = random.NextDouble() * (fracture.Style == 2 ? 22 : 10);
                var a = new Point(Math.Cos(angle) * distance, Math.Sin(angle) * distance);
                var b = new Point(a.X + (random.NextDouble() - .5) * 9, a.Y + (random.NextDouble() - .5) * 9);
                dc.DrawLine(i % 4 == 0 ? bright : fine, a, b);
            }
            dc.Pop();
        }
        var bitmap = new RenderTargetBitmap(288, 288, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual); bitmap.Freeze(); return bitmap;
    }
}
