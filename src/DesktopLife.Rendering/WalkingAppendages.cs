using System.Windows;
using System.Windows.Media;
namespace DesktopLife.Rendering;

internal static class WalkingAppendages
{
    public static void Draw(DrawingContext dc, int frame, bool ant)
    {
        var leg = new Pen(new SolidColorBrush(Color.FromRgb(59, 38, 24)), ant ? 0.48 : 0.7)
            { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var fine = new Pen(leg.Brush, ant ? 0.25 : 0.35);
        for (var side = -1; side <= 1; side += 2)
        {
            for (var pair = 0; pair < 3; pair++)
            {
                // Alternating tripods: a longer planted stroke and a shorter return stroke.
                var phase = (frame / 8.0 + pair * 0.5 + (side > 0 ? 0.5 : 0)) % 1;
                var stroke = phase < 0.65 ? 1 - 2 * phase / 0.65 : -Math.Cos((phase - 0.65) / 0.35 * Math.PI);
                var rootX = ant ? 1.6 - pair * 1.3 : 6 - pair * 4;
                var reachX = ant ? 3 - pair * 3 : 5 - pair * 5;
                var width = ant ? 5.5 : 10.5;
                var root = new Point(rootX, side * (ant ? 0.6 : 2.2));
                var knee = new Point(rootX + reachX * 0.65 + stroke * (ant ? 1.2 : 1.9), side * width * 0.55);
                var foot = new Point(rootX + reachX + stroke * (ant ? 3.9 : 6.5), side * (width - (phase > 0.65 ? Math.Sin((phase - 0.65) / 0.35 * Math.PI) * 1.2 : 0)));
                dc.DrawLine(leg, root, knee); dc.DrawLine(leg, knee, foot);
                dc.DrawLine(fine, foot, new(foot.X - 0.9, foot.Y + side * 0.4));
                if (!ant)
                    for (var thorn = 1; thorn <= 3; thorn++)
                    {
                        var t = thorn / 4.0;
                        var p = new Point(knee.X + (foot.X - knee.X) * t, knee.Y + (foot.Y - knee.Y) * t);
                        dc.DrawLine(fine, p, new(p.X - 0.7, p.Y + side * 0.65));
                    }
            }
            var sweep = Math.Sin(frame * Math.PI / 4 + side * 0.4);
            var start = new Point(ant ? 5.5 : 11, side * (ant ? 0.7 : 1.1));
            var elbow = new Point(ant ? 7.6 : 16, side * (ant ? 2.7 : 3.2));
            var end = new Point(ant ? 10 : 25, side * ((ant ? 3.5 : 6.5) + sweep * 0.75));
            dc.DrawLine(fine, start, elbow);
            var geometry = new StreamGeometry();
            using (var path = geometry.Open())
            {
                path.BeginFigure(elbow, false, false);
                path.QuadraticBezierTo(new((elbow.X + end.X) / 2, end.Y), end, true, false);
            }
            geometry.Freeze(); dc.DrawGeometry(null, fine, geometry);
        }
    }
}
