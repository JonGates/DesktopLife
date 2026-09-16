using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Rendering;

internal static class CuteOceanBody
{
    private static Brush Color(byte r, byte g, byte b) => new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
    public static void Draw(DrawingContext dc, InsectDefinition definition, double beat)
    {
        double l = definition.BodyLength, w = definition.BodyWidth;
        var kind = definition.Kind;
        var fill = kind switch
        {
            CreatureKind.Clownfish => Color(245, 154, 87),
            CreatureKind.BlueTang => Color(111, 162, 224),
            CreatureKind.YellowTang => Color(240, 213, 99),
            CreatureKind.Butterflyfish => Color(245, 231, 180),
            CreatureKind.Angelfish => Color(133, 180, 205),
            CreatureKind.Lionfish => Color(202, 150, 129),
            CreatureKind.Pufferfish => Color(171, 192, 128),
            CreatureKind.Seahorse => Color(241, 191, 106),
            CreatureKind.MandarinFish => Color(116, 191, 177),
            CreatureKind.RoyalGramma => Color(181, 137, 207),
            CreatureKind.MoorishIdol => Color(245, 231, 152),
            _ => Color(223, 148, 152)
        };
        var outline = new Pen(Color(64, 103, 117), 0.5) { LineJoin = PenLineJoin.Round, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        if (kind == CreatureKind.Seahorse) { Seahorse(dc, l, w, fill, outline, beat); return; }
        StreamGeometry Polygon(params Point[] points)
        {
            var path = new StreamGeometry(); using (var p = path.Open()) { p.BeginFigure(points[0], true, true); foreach (var point in points.Skip(1)) p.LineTo(point, true, false); }
            return path;
        }
        var yellowTail = kind is CreatureKind.BlueTang or CreatureKind.RoyalGramma;
        dc.PushTransform(new RotateTransform(beat * 15, -l * 0.27, 0));
        dc.DrawGeometry(yellowTail ? Color(245, 210, 96) : fill, outline,
            Polygon(new(-l * 0.24, 0), new(-l * 0.5, -w * 0.28), new(-l * 0.46, 0), new(-l * 0.5, w * 0.28)));
        dc.Pop();
        dc.DrawGeometry(fill, outline, Polygon(new(-l * 0.18, -w * 0.24), new(-l * 0.06, -w * 0.48), new(l * 0.15, -w * 0.3)));
        dc.DrawGeometry(fill, outline, Polygon(new(-l * 0.14, w * 0.23), new(-l * 0.07, w * 0.43), new(l * 0.13, w * 0.25)));
        if (kind == CreatureKind.Lionfish)
            for (var i = 0; i < 7; i++)
            {
                var x = l * (-0.22 + i * 0.075);
                dc.DrawLine(new Pen(fill, 1.4), new(x, -w * 0.14), new(x - l * 0.13, -w * (0.42 + 0.05 * Math.Sin(i))));
                dc.DrawLine(outline, new(x, w * 0.1), new(x - l * 0.19, w * (0.35 + 0.06 * Math.Cos(i))));
            }
        if (kind == CreatureKind.MoorishIdol)
        {
            var streamer = new StreamGeometry(); using (var p = streamer.Open())
            { p.BeginFigure(new(l * 0.1, -w * 0.22), false, false); p.QuadraticBezierTo(new(l * 0.1, -w * 0.66), new(-l * 0.32, -w * 0.47), true, false); }
            dc.DrawGeometry(null, new Pen(Color(244, 245, 222), 1.1), streamer);
        }
        var body = new StreamGeometry();
        using (var p = body.Open())
        {
            p.BeginFigure(new(-l * 0.3, 0), true, true);
            p.BezierTo(new(-l * 0.27, -w * 0.5), new(l * 0.31, -w * 0.5), new(l * 0.43, -w * 0.05), true, false);
            p.BezierTo(new(l * 0.48, w * 0.27), new(-l * 0.15, w * 0.49), new(-l * 0.3, 0), true, false);
        }
        dc.DrawGeometry(fill, outline, body);
        dc.PushClip(body);
        if (kind == CreatureKind.RoyalGramma)
            dc.DrawRectangle(Color(246, 214, 111), null, new Rect(-l * 0.5, -w, l * 0.52, w * 2));
        if (kind is CreatureKind.Clownfish or CreatureKind.Butterflyfish or CreatureKind.MoorishIdol or CreatureKind.Lionfish)
        {
            var stripe = kind switch { CreatureKind.Clownfish => Brushes.Ivory, CreatureKind.MoorishIdol => Color(75, 94, 101), CreatureKind.Butterflyfish => Color(220, 155, 101), _ => Color(247, 231, 211) };
            foreach (var fraction in new[] { -0.17, 0.06, 0.28 })
                dc.DrawLine(new Pen(stripe, l * (kind == CreatureKind.Lionfish ? 0.055 : 0.075)), new(l * fraction, -w), new(l * (fraction - 0.05), w));
        }
        if (kind is CreatureKind.Angelfish or CreatureKind.Wrasse)
            for (var i = -2; i <= 2; i++)
                dc.DrawLine(new Pen(kind == CreatureKind.Angelfish ? Color(242, 219, 127) : Color(117, 166, 213), 0.8), new(-l * 0.4, i * w * 0.11), new(l * 0.4, i * w * 0.11 - w * 0.04));
        if (kind == CreatureKind.BlueTang)
            dc.DrawEllipse(Color(70, 100, 156), null, new(-l * 0.02, -w * 0.09), l * 0.21, w * 0.16);
        if (kind == CreatureKind.MandarinFish)
            for (var i = 0; i < 4; i++)
                dc.DrawEllipse(null, new Pen(Color(239, 159, 96), 1.1), new(l * (-0.18 + i * 0.14), (i % 2 == 0 ? -1 : 1) * w * 0.12), l * 0.06, w * 0.17);
        if (kind == CreatureKind.Pufferfish)
        {
            dc.DrawEllipse(Color(235, 231, 178), null, new(0, w * 0.22), l * 0.36, w * 0.25);
            for (var i = 0; i < 7; i++) dc.DrawEllipse(Color(107, 143, 103), null, new(l * (-0.21 + (i % 4) * 0.13), -w * (i < 4 ? 0.16 : 0.02)), 0.65, 0.65);
        }
        dc.Pop();
        dc.PushTransform(new RotateTransform(beat * 22, l * 0.12, w * 0.08));
        dc.DrawGeometry(fill, outline, Polygon(new(l * 0.16, 0), new(-l * 0.02, w * 0.22), new(l * 0.12, w * 0.24)));
        dc.Pop();
        Eye(dc, new(l * 0.3, -w * 0.1), Math.Clamp(l * 0.048, 1.1, 2));
    }

    private static void Seahorse(DrawingContext dc, double l, double w, Brush fill, Pen outline, double beat)
    {
        var path = new StreamGeometry();
        using (var p = path.Open())
        {
            p.BeginFigure(new(-l * 0.12, -w * 0.31), false, false);
            p.BezierTo(new(-l * 0.45, -w * 0.08), new(l * 0.2, w * 0.08), new(-l * 0.02, w * 0.29), true, false);
            p.BezierTo(new(-l * 0.2, w * 0.5), new(-l * 0.48, w * 0.28), new(-l * 0.2 + beat * 0.7, w * 0.26), true, false);
        }
        dc.DrawGeometry(null, new Pen(fill, l * 0.27) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, path);
        dc.DrawEllipse(fill, outline, new(-l * 0.07, -w * 0.05), l * 0.2, w * 0.22);
        dc.DrawEllipse(fill, outline, new(l * 0.05, -w * 0.3), l * 0.25, w * 0.11);
        dc.DrawLine(new Pen(fill, l * 0.16), new(l * 0.18, -w * 0.29), new(l * 0.45, -w * 0.25));
        dc.DrawEllipse(Color(238, 216, 157), outline, new(-l * 0.3, -w * 0.05), l * (0.08 + beat * 0.025), w * 0.1);
        Eye(dc, new(l * 0.12, -w * 0.32), 1.2);
    }

    private static void Eye(DrawingContext dc, Point at, double size)
    {
        dc.DrawEllipse(Brushes.Ivory, null, at, size, size);
        dc.DrawEllipse(Color(45, 74, 88), null, new(at.X + size * 0.3, at.Y), size * 0.5, size * 0.6);
    }
}
