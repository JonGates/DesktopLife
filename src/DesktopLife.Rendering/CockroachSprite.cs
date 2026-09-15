using System.Windows;
using System.Windows.Media;

namespace DesktopLife.Rendering;

/// <summary>Cached top-down placeholder with six animated legs and long antennae; +X is forward.</summary>
internal static class CockroachSprite
{
    public static DrawingGroup Create(bool alternateStep)
    {
        var drawing = new DrawingGroup();
        var body = new LinearGradientBrush(Color.FromRgb(128, 72, 35), Color.FromRgb(69, 34, 18), 90);
        var thorax = new SolidColorBrush(Color.FromRgb(112, 61, 28));
        var head = new SolidColorBrush(Color.FromRgb(65, 35, 20));
        var outline = new Pen(new SolidColorBrush(Color.FromArgb(190, 199, 144, 80)), 0.65);
        var leg = new Pen(new SolidColorBrush(Color.FromRgb(80, 43, 21)), 1.1);
        var antenna = new Pen(new SolidColorBrush(Color.FromRgb(106, 64, 31)), 0.8);
        using (var dc = drawing.Open())
        {
            for (var side = -1; side <= 1; side += 2)
            {
                for (var pair = 0; pair < 3; pair++)
                {
                    var x = 5 - pair * 6;
                    var step = ((pair + (side > 0 ? 1 : 0)) % 2 == 0) == alternateStep ? 2 : -2;
                    var knee = new Point(x + step, side * 9);
                    dc.DrawLine(leg, new Point(x, side * 4), knee);
                    dc.DrawLine(leg, knee, new Point(x - 4 + step, side * 12));
                }
                dc.DrawLine(antenna, new Point(10, side * 2), new Point(17, side * 5));
                dc.DrawLine(antenna, new Point(17, side * 5), new Point(22, side * 9));
            }
            dc.DrawEllipse(body, outline, new Point(-4, 0), 11, 6);
            dc.DrawEllipse(thorax, outline, new Point(5, 0), 5, 4.8);
            dc.DrawEllipse(head, outline, new Point(10, 0), 3, 3.2);
            dc.DrawLine(leg, new Point(-13, 0), new Point(3, 0));
            dc.DrawLine(outline, new Point(-9, -3), new Point(-3, -4));
            dc.DrawLine(outline, new Point(-9, 3), new Point(-3, 4));
        }
        drawing.Freeze();
        return drawing;
    }
}
