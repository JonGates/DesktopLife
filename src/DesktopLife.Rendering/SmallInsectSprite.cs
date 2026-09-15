using System.Windows;
using System.Windows.Media;
namespace DesktopLife.Rendering;
internal static class SmallInsectSprite
{
    public static DrawingGroup Create(bool caterpillar, int phase)
    {
        var drawing = new DrawingGroup();
        using (var dc = drawing.Open())
        {
            if (caterpillar) DrawCaterpillar(dc, phase);
            else
            {
                WalkingAppendages.Draw(dc, phase, true);
                dc.DrawImage(InsectBodyTextures.Ant, new Rect(-8, -2.1, 15, 4.2));
            }
        }
        RenderOptions.SetBitmapScalingMode(drawing, BitmapScalingMode.HighQuality);
        drawing.Freeze(); return drawing;
    }

    private static void DrawCaterpillar(DrawingContext dc, int phase)
    {
        var angle = phase * Math.PI / 4;
        var foot = new Pen(new SolidColorBrush(Color.FromRgb(86, 78, 35)), 0.5);
        // Small thoracic legs near the head and soft abdominal prolegs.
        foreach (var x in new[] { -12.0, -7, -3, 1, 7, 9, 11 })
        for (var side = -1; side <= 1; side += 2)
        {
            var wave = Math.Sin(angle - x * 0.18);
            dc.DrawLine(foot, new(x, side * 2.2), new(x - wave * 0.55, side * (3.3 + Math.Abs(wave) * 0.3)));
        }
        // Deform one continuous textured body longitudinally; no circular bead outlines.
        var edges = new double[13];
        for (var i = 0; i <= 12; i++)
            edges[i] = -14 + i * 28.0 / 12 + (i is 0 or 12 ? 0 : Math.Sin(angle - i * 0.62) * 0.3);
        for (var i = 0; i < 12; i++)
        {
            var width = edges[i + 1] - edges[i];
            // Sample the full texture through an overlapping clip. Cropped bitmaps introduce
            // a transparent interpolation border at every segment when rendered at small sizes.
            dc.PushClip(new RectangleGeometry(new Rect(edges[i] - 0.18, -3, width + 0.36, 6)));
            dc.DrawImage(InsectBodyTextures.Caterpillar, new Rect(edges[i] - i * width, -2.9, width * 12, 5.8));
            dc.Pop();
        }
    }
}
