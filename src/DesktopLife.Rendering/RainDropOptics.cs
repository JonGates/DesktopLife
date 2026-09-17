using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

/// <summary>Cached glass silhouettes and an approximate inverted local image lens.</summary>
internal static class RainDropOptics
{
    private static readonly Geometry[] Shapes = Enumerable.Range(0, 12).Select(Shape).ToArray();
    private static readonly Brush Body = Freeze(new LinearGradientBrush(new GradientStopCollection {
        new(Color.FromArgb(220, 6, 15, 22), 0), new(Color.FromArgb(165, 8, 19, 28), .22),
        new(Color.FromArgb(20, 125, 157, 174), .5), new(Color.FromArgb(95, 203, 221, 228), .77),
        new(Color.FromArgb(175, 10, 27, 38), 1) }, 90));
    private static readonly Brush Glint = Freeze(new RadialGradientBrush(Color.FromArgb(210, 245, 251, 253), Colors.Transparent));
    private static readonly Brush Caustic = Freeze(new RadialGradientBrush(Color.FromArgb(155, 213, 237, 248), Colors.Transparent));
    private static readonly Pen Edge = Freeze(new Pen(new SolidColorBrush(Color.FromArgb(190, 6, 17, 25)), .12));
    private static readonly Pen Lip = Freeze(new Pen(new SolidColorBrush(Color.FromArgb(115, 228, 245, 252)), .045));
    private static T Freeze<T>(T value) where T : Freezable { value.Freeze(); return value; }
    private static Geometry Shape(int index)
    {
        var lean = (index % 4 - 1.5) * .09;
        var shoulder = .55 + index % 3 * .12;
        var geometry = new StreamGeometry();
        using (var c = geometry.Open())
        {
            c.BeginFigure(new(lean, -1), true, true);
            c.BezierTo(new(.52 + lean, -1.04), new(.98, -shoulder), new(.93, .12), true, false);
            c.BezierTo(new(1.02, .74), new(.43, 1.06), new(-.12, .96), true, false);
            c.BezierTo(new(-.9, .93), new(-1.02, .33), new(-.81, -.18), true, false);
            c.BezierTo(new(-.65, -.65), new(-.46 + lean, -.95), new(lean, -1), true, false);
        }
        return Freeze(geometry);
    }
    public static void Draw(DrawingContext dc, GlassDrop drop, float time, WorldBounds viewport, ImageSource? image)
    {
        var index = (int)(drop.Born * 117) % Shapes.Length;
        var shape = Shapes[index];
        var r = (double)drop.Radius;
        var impact = Math.Clamp((time - drop.Born) / .2, 0, 1);
        var stretch = drop.Sliding ? 1.2 + Math.Min(.6, drop.Speed / 700) : .95 + index % 3 * .08;
        dc.PushTransform(new TranslateTransform(drop.Position.X, drop.Position.Y));
        dc.PushTransform(new ScaleTransform(r * (1.13 - impact * .13), r * stretch));
        dc.DrawGeometry(Body, Edge, shape);
        if (image != null && image.Width > 0 && image.Height > 0)
        {
            var cover = Math.Max(viewport.Width / image.Width, viewport.Height / image.Height);
            var u = .5 + (drop.Position.X - viewport.Center.X) / (cover * image.Width);
            var v = .5 + (drop.Position.Y - viewport.Center.Y) / (cover * image.Height);
            var w = Math.Min(1, r * 7 / (cover * image.Width));
            var h = Math.Min(1, r * 7 * stretch / (cover * image.Height));
            var lens = new ImageBrush(image) {
                Viewbox = new Rect(Math.Clamp(u - w / 2, 0, 1 - w), Math.Clamp(v - h / 2, 0, 1 - h), w, h),
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox, Stretch = Stretch.Fill,
                RelativeTransform = new ScaleTransform(-1, -1, .5, .5)
            };
            dc.PushOpacity(.85); dc.DrawGeometry(lens, null, shape); dc.Pop();
            dc.PushOpacity(.48); dc.DrawGeometry(Body, Edge, shape); dc.Pop();
        }
        dc.DrawEllipse(Caustic, null, new(.1, .65), .58, .19);
        dc.DrawEllipse(Glint, null, new(-.32, -.61), .17, .08);
        dc.DrawLine(Lip, new(-.62, .48), new(-.37, .72));
        dc.Pop(); dc.Pop();
    }
}
