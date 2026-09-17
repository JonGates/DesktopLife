using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

/// <summary>Cached glass silhouettes and an approximate inverted local image lens.</summary>
internal static class RainDropOptics
{
    private static readonly Geometry[] Shapes = Enumerable.Range(0, 12).Select(Shape).ToArray();
    private static readonly Brush Body = Freeze(new LinearGradientBrush(new GradientStopCollection {
        new(Color.FromArgb(155, 6, 15, 22), 0), new(Color.FromArgb(45, 8, 19, 28), .22),
        new(Color.FromArgb(5, 125, 157, 174), .5), new(Color.FromArgb(30, 203, 221, 228), .77),
        new(Color.FromArgb(115, 10, 27, 38), 1) }, 105));
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
            dc.DrawGeometry(Body, Edge, shape);
            dc.DrawEllipse(Caustic, null, new(.22 - index % 3 * .12, .65), .25 + index % 4 * .06, .1);
            dc.DrawEllipse(Glint, null, new(-.35 + index % 3 * .12, -.61), .07 + index % 3 * .03, .04);
            dc.DrawLine(Lip, new(-.62, .48), new(-.37, .72)); dc.Pop(); dc.Pop();
        }
        var bitmap = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual); bitmap.Freeze();
        return (BitmapSource)bitmap;
    }).ToArray());
    private static T Freeze<T>(T value) where T : Freezable { value.Freeze(); return value; }
    private static Geometry Shape(int index)
    {
        var lean = (index % 4 - 1.5) * .07;
        var shoulder = .48 + index % 3 * .08;
        var geometry = new StreamGeometry();
        using (var c = geometry.Open())
        {
            // Broad, rounded crown: beads adhering to glass have no pointed tip.
            c.BeginFigure(new(lean, -.88), true, true);
            c.BezierTo(new(.58 + lean, -.91), new(.94, -shoulder), new(.94, .08), true, false);
            c.BezierTo(new(.99, .69), new(.49, .98), new(-.06, .94), true, false);
            c.BezierTo(new(-.68, .97), new(-.98, .54), new(-.93, -.04), true, false);
            c.BezierTo(new(-.91, -.57), new(-.5 + lean, -.9), new(lean, -.88), true, false);
        }
        return Freeze(geometry);
    }
    public static void Draw(DrawingContext dc, GlassDrop drop, float time, WorldBounds viewport, ImageSource? image)
    {
        var index = drop.ShapeIndex % Shapes.Length;
        var shape = Shapes[index];
        var r = (double)drop.Radius;
        var impact = Math.Clamp((time - drop.Born) / .2, 0, 1);
        var stretch = .82 + index % 4 * .06 + Math.Min(.48, drop.Speed / 500);
        var rx = r * (1.13 - impact * .13);
        var ry = r * stretch;
        if (image != null && r >= 4 && image.Width > 0 && image.Height > 0)
            dc.DrawImage(RainImageOptics.LensImage(image, drop, viewport, shape), new Rect(drop.Position.X - rx, drop.Position.Y - ry, rx * 2, ry * 2));
        // Reuse the same baked edge/glint layer as desktop mode: no per-drop opacity groups.
        dc.DrawImage(Sprites.Value[index], new Rect(drop.Position.X - rx * 4 / 3, drop.Position.Y - ry * 4 / 3, rx * 8 / 3, ry * 8 / 3));
    }
}