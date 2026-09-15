using System.Windows;
using System.Windows.Media;

namespace DesktopLife.Rendering;

/// <summary>Two vector wing frames, built and frozen once. Local +X points toward the head.</summary>
internal static class FlySprite
{
    public static DrawingGroup Create(bool wingsUp)
    {
        var drawing = new DrawingGroup();
        var wing = new SolidColorBrush(Color.FromArgb(150, 200, 224, 228));
        var body = new SolidColorBrush(Color.FromRgb(31, 34, 30));
        var thorax = new SolidColorBrush(Color.FromRgb(62, 68, 49));
        var outline = new Pen(new SolidColorBrush(Color.FromArgb(155, 220, 220, 202)), 0.65);
        var leg = new Pen(Brushes.Black, 1);
        using (var dc = drawing.Open())
        {
            for (var side = -1; side <= 1; side += 2)
            {
                dc.DrawLine(leg, new Point(0, side * 3), new Point(3, side * 8));
                dc.DrawLine(leg, new Point(-3, side * 3), new Point(-6, side * 8));
                dc.DrawLine(leg, new Point(3, side * 2), new Point(7, side * 6));
                dc.DrawEllipse(wing, outline, new Point(-2, side * (wingsUp ? 6 : 4)), wingsUp ? 7 : 9, wingsUp ? 4 : 2.5);
            }
            dc.DrawEllipse(body, outline, new Point(-4, 0), 6, 3.5);
            dc.DrawEllipse(thorax, outline, new Point(1, 0), 4, 3.6);
            dc.DrawEllipse(body, outline, new Point(5.5, 0), 2.7, 3);
            dc.DrawEllipse(Brushes.IndianRed, null, new Point(6.5, -1.7), 1, 1.2);
            dc.DrawEllipse(Brushes.IndianRed, null, new Point(6.5, 1.7), 1, 1.2);
        }
        drawing.Freeze();
        return drawing;
    }
}
