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
            var leg = new Pen(new SolidColorBrush(Color.FromRgb(66, 49, 29)), 0.75);
            if (caterpillar)
            {
                for (var i = 0; i < 8; i++)
                {
                    var x = -13 + i * 3.4;
                    var wave = Math.Sin(phase * Math.PI / 4 - i * 0.75);
                    var y = wave * 1.1;
                    dc.DrawLine(leg, new(x, y), new(x - wave, y + 4));
                    dc.DrawLine(leg, new(x, y), new(x + wave, y - 4));
                    var fill = new RadialGradientBrush(Color.FromRgb(177, 211, 93), Color.FromRgb(66, 111, 38));
                    dc.DrawEllipse(fill, null, new(x, y), 3.1 + wave * 0.25, 3.4);
                }
                dc.DrawEllipse(Brushes.SaddleBrown, null, new(13, 0), 2.4, 2.6);
            }
            else
            {
                for (var i = 0; i < 3; i++)
                for (var side = -1; side <= 1; side += 2)
                {
                    var step = Math.Sin(phase * Math.PI / 4 + i * Math.PI + side) * 1.6;
                    var root = new Point(i * 2 - 2, side);
                    var knee = new Point(root.X - 1 + step, side * 4);
                    dc.DrawLine(leg, root, knee); dc.DrawLine(leg, knee, new(root.X - 3 + step, side * 6));
                }
                var body = new RadialGradientBrush(Color.FromRgb(133, 78, 42), Color.FromRgb(38, 25, 19));
                dc.DrawEllipse(body, null, new(-5, 0), 3.4, 2.5);
                dc.DrawEllipse(body, null, new(0, 0), 2.6, 1.5);
                dc.DrawEllipse(body, null, new(4, 0), 2.3, 2);
                dc.DrawLine(leg, new(5, -1), new(8, -4)); dc.DrawLine(leg, new(5, 1), new(8, 4));
            }
        }
        drawing.Freeze(); return drawing;
    }
}
