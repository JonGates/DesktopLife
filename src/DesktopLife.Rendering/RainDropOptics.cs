using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

/// <summary>Cached glass silhouettes and an approximate inverted local image lens.</summary>
internal static class RainDropOptics
{
    private static readonly Geometry[] Shapes = Enumerable.Range(0, 12).Select(Shape).Concat(Enumerable.Range(0, 48).Select(RestingShape)).ToArray();
    private static readonly Brush Body = Freeze(new LinearGradientBrush(new GradientStopCollection {
        new(Color.FromArgb(155, 6, 15, 22), 0), new(Color.FromArgb(45, 8, 19, 28), .22),
        new(Color.FromArgb(5, 125, 157, 174), .5), new(Color.FromArgb(30, 203, 221, 228), .77),
        new(Color.FromArgb(190, 232, 245, 250), .91), new(Color.FromArgb(110, 10, 27, 38), 1) }, 105));
    private static readonly Brush RestingBody = Freeze(new LinearGradientBrush(new GradientStopCollection {
        new(Color.FromArgb(95, 9, 19, 23), 0), new(Color.FromArgb(130, 12, 23, 27), .15),
        new(Color.FromArgb(20, 40, 58, 64), .38), new(Color.FromArgb(3, 150, 178, 185), .58),
        new(Color.FromArgb(35, 218, 235, 239), .82), new(Color.FromArgb(85, 15, 29, 34), 1) }, 100));
    private static readonly Pen RestingEdge = Freeze(new Pen(new SolidColorBrush(Color.FromArgb(92, 8, 20, 25)), .035));
    private static readonly Brush Glint = Freeze(new RadialGradientBrush(Color.FromArgb(210, 245, 251, 253), Colors.Transparent));
    private static readonly Brush Caustic = Freeze(new RadialGradientBrush(Color.FromArgb(90, 213, 237, 248), Colors.Transparent));
    private static readonly Pen Edge = Freeze(new Pen(new SolidColorBrush(Color.FromArgb(135, 6, 17, 25)), .06));
    private static readonly Pen Lip = Freeze(new Pen(new SolidColorBrush(Color.FromArgb(115, 228, 245, 252)), .045));
    private static readonly Lazy<BitmapSource[]> Sprites = new(() => Shapes.Select((shape, index) =>
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.PushTransform(new TranslateTransform(32, 32)); dc.PushTransform(new ScaleTransform(24, 24));
            var resting = index >= 12;
            dc.DrawGeometry(resting ? RestingBody : Body, resting ? RestingEdge : Edge, shape);
            dc.PushClip(shape);
            if (resting)
            {
                var bounds = shape.Bounds;
                var variant = (index - 12) % 12;
                var gx = bounds.Left + bounds.Width * (.22 + variant % 4 * .13);
                var gy = bounds.Top + bounds.Height * (.18 + variant % 3 * .055);
                dc.PushOpacity(.48 + variant % 4 * .16);
                dc.DrawEllipse(Glint, null, new(gx, gy), .055 + variant % 4 * .045, .025 + variant % 3 * .016);
                dc.DrawEllipse(Caustic, null, new(bounds.Left + bounds.Width * (.32 + variant % 3 * .13), bounds.Bottom - .14), .14 + variant % 5 * .055, .055 + variant % 3 * .02);
                dc.Pop();
            }
            else
            {
                dc.DrawEllipse(Caustic, null, new(.22 - index % 3 * .12, .65), .25 + index % 4 * .06, .1);
                dc.DrawEllipse(Glint, null, new(-.22 + index % 3 * .07, -.12), .07 + index % 3 * .03, .04);
                dc.DrawEllipse(Glint, null, new(.02, .79), .43, .1);
            }
            dc.Pop();
            dc.Pop(); dc.Pop();
        }
        var bitmap = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual); bitmap.Freeze();
        return (BitmapSource)bitmap;
    }).ToArray());
    private static readonly Lazy<BitmapSource[]> Necks = new(() => Enumerable.Range(0, 12).Select(index =>
    {
        var shape = new StreamGeometry();
        var bend = (index % 4 - 1.5) * 2;
        using (var c = shape.Open())
        {
            c.BeginFigure(new(12 + bend, 0), true, true);
            c.BezierTo(new(12 + bend, 20), new(19, 39), new(22, 64), true, false);
            c.LineTo(new(2, 64), true, false);
            c.BezierTo(new(9, 38), new(9 + bend, 18), new(12 + bend, 0), true, false);
        }
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            var brush = new LinearGradientBrush(new GradientStopCollection {
                new(Color.FromArgb(12, 35, 51, 57), 0), new(Color.FromArgb(90, 16, 32, 39), .32),
                new(Color.FromArgb(32, 157, 185, 195), .64), new(Color.FromArgb(68, 207, 231, 238), 1) }, 0);
            dc.DrawGeometry(brush, null, shape);
        }
        var bitmap = new RenderTargetBitmap(24, 64, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual); bitmap.Freeze();
        return (BitmapSource)bitmap;
    }).ToArray());
    private static T Freeze<T>(T value) where T : Freezable { value.Freeze(); return value; }
    private static Geometry Shape(int index)
    {
        var lean = (index % 4 - 1.5) * .07;
        var geometry = new StreamGeometry();
        using (var c = geometry.Open())
        {
            // The moving front is a rounded liquid bead, not a pointed droplet icon.
            c.BeginFigure(new(lean, -.78), true, true);
            c.BezierTo(new(.48 + lean, -.8), new(.83, -.38), new(.86, .12), true, false);
            c.BezierTo(new(.91, .65), new(.55, .98), new(.02, .98), true, false);
            c.BezierTo(new(-.51, .99), new(-.86, .7), new(-.88, .18), true, false);
            c.BezierTo(new(-.9, -.28), new(-.52 + lean, -.75), new(lean, -.78), true, false);
        }
        return Freeze(geometry);
    }
    private static Geometry RestingShape(int index)
    {
        var family = index / 12;
        var variant = index % 12;
        var phase = variant * 2.39996;
        var points = new Point[12];
        for (var i = 0; i < points.Length; i++)
        {
            var angle = i * Math.PI * 2 / points.Length;
            var x = Math.Cos(angle); var y = Math.Sin(angle);
            var wobble = 1 + (.025 + family * .022) * Math.Sin(3 * angle + phase);
            // Distinct contact-line families, not differently scaled copies of one oval.
            var width = family switch { 0 => .84, 1 => .62 + .2 * y, 2 => .96, _ => .76 + .14 * Math.Cos(2 * angle + phase) };
            var height = family switch { 0 => .84, 1 => .96, 2 => .57 + variant % 3 * .05, _ => .77 };
            var lean = family == 0 ? .015 : (variant % 2 == 0 ? .18 : -.18);
            points[i] = new(Math.Clamp(x * width * wobble + lean * y, -.98, .98), Math.Clamp(y * height * wobble, -.98, .98));
        }
        var geometry = new StreamGeometry();
        using (var c = geometry.Open())
        {
            c.BeginFigure(points[0], true, true);
            for (var i = 0; i < points.Length; i++)
            {
                var previous = points[(i + points.Length - 1) % points.Length];
                var start = points[i]; var end = points[(i + 1) % points.Length]; var next = points[(i + 2) % points.Length];
                c.BezierTo(start + (end - previous) / 6, end - (next - start) / 6, end, true, false);
            }
        }
        return Freeze(geometry);
    }
    public static void Draw(DrawingContext dc, GlassDrop drop, float time, WorldBounds viewport, ImageSource? image)
    {
        var index = drop.Sliding ? drop.ShapeIndex % 12 : 12 + drop.RestingShapeIndex % 48;
        var shape = Shapes[index];
        var r = (double)drop.Radius;
        var impact = Math.Clamp((time - drop.Born) / .2, 0, 1);
        var stretch = .82 + index % 4 * .06 + Math.Min(.48, drop.Speed / 500);
        var rx = r * (1.13 - impact * .13);
        var ry = r * stretch;
        if (!drop.Sliding) { rx *= .9 + index % 4 * .07; ry *= .94 + index % 3 * .05; }
        else
        {
            var phase = time * 2.3 + drop.ShapeIndex * 1.71;
            var swelling = 1 + .16 * drop.MergePulse;
            var tension = .5 + .5 * Math.Sin(phase);
            rx = r * swelling * (.94 + .025 * Math.Sin(phase * .73));
            ry = r * (1.02 + Math.Min(.18, drop.Speed / 1200)) / Math.Sqrt(swelling);
            var neckLength = r * (.45 + Math.Min(2.8, drop.Speed / 75)) * (.85 + .15 * tension);
            var neckWidth = r * (.3 + .07 * tension);
            dc.DrawImage(Necks.Value[drop.ShapeIndex % 12], new Rect(drop.Position.X - neckWidth / 2, drop.Position.Y - ry * .45 - neckLength, neckWidth, neckLength));
        }
        if (image != null && r >= 4 && image.Width > 0 && image.Height > 0)
            dc.DrawImage(RainImageOptics.LensImage(image, drop, viewport, shape), new Rect(drop.Position.X - rx, drop.Position.Y - ry, rx * 2, ry * 2));
        // Reuse the same baked edge/glint layer as desktop mode: no per-drop opacity groups.
        dc.DrawImage(Sprites.Value[index], new Rect(drop.Position.X - rx * 4 / 3, drop.Position.Y - ry * 4 / 3, rx * 8 / 3, ry * 8 / 3));
    }
}