using System.Numerics;
using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

internal static class SpiderSilkEffect
{
    private static readonly Pen Edge = MakePen(Color.FromArgb(65, 53, 67, 75), 1.05);
    private static readonly Pen Thread = MakePen(Color.FromArgb(195, 233, 241, 246), 0.42);
    private static readonly Pen[] WebPens = [Edge, Thread];
    private static Pen MakePen(Color color, double width)
    {
        var pen = new Pen(new SolidColorBrush(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        pen.Freeze(); return pen;
    }

    public static void Draw(DrawingContext dc, ICreature spider, WorldBounds viewport, double scaleX, double scaleY)
    {
        if (spider.Kind != CreatureKind.Spider || spider.SilkAnchor is not { } anchor) return;
        var casting = spider.MotionState == LocomotionState.SilkCasting;
        var opacity = spider.MotionState == LocomotionState.SilkSettling ? 1 - spider.MotionProgress : 1;
        var origin = new Vector2(viewport.Left, viewport.Top);
        var direction = new Vector2(MathF.Cos(spider.Rotation), MathF.Sin(spider.Rotation));
        var start = spider.Position - direction * (8 * spider.Scale);
        var end = casting ? Vector2.Lerp(start, anchor, spider.MotionProgress) : anchor;
        Point Local(Vector2 point) => new(point.X - origin.X, point.Y - origin.Y);
        dc.PushClip(new RectangleGeometry(new Rect(0, 0, viewport.Width / scaleX, viewport.Height / scaleY)));
        dc.PushTransform(new ScaleTransform(1 / scaleX, 1 / scaleY));
        dc.PushOpacity(opacity);
        var strand = new StreamGeometry();
        var a = Local(start); var b = Local(end);
        var sag = Math.Min(8, Vector2.Distance(start, end) * .024) * (casting ? 1 : .22);
        using (var path = strand.Open())
        {
            path.BeginFigure(a, false, false);
            path.QuadraticBezierTo(new((a.X + b.X) / 2, (a.Y + b.Y) / 2 + sag), b, true, false);
        }
        foreach (var pen in WebPens) dc.DrawGeometry(null, pen, strand);
        // A small web marks the fixed landing point; it appears as the strand reaches its target.
        var webProgress = casting ? Math.Clamp((spider.MotionProgress - 0.75f) * 4, 0, 1) : 1;
        if (webProgress > 0)
        {
            dc.PushOpacity(webProgress);
            var center = Local(anchor);
            var radius = Math.Clamp(12 * spider.Scale, 7, 23);
            const int spokes = 9;
            Point Node(int spoke, double ring)
            {
                var angle = spoke * Math.PI * 2 / spokes + .17;
                var r = radius * (1 + .13 * Math.Sin(spoke * 2.3)) * ring;
                return new(center.X + Math.Cos(angle) * r, center.Y + Math.Sin(angle) * r * .83);
            }
            foreach (var pen in WebPens)
            {
                for (var spoke = 0; spoke < spokes; spoke++) dc.DrawLine(pen, center, Node(spoke, 1.1));
                foreach (var ring in new[] { .25, .48, .73, 1.0 })
                {
                    var mesh = new StreamGeometry();
                    using (var path = mesh.Open())
                    {
                        path.BeginFigure(Node(0, ring), false, true);
                        for (var spoke = 0; spoke < spokes; spoke++)
                        {
                            var from = Node(spoke, ring); var to = Node((spoke + 1) % spokes, ring);
                            var mid = new Point((from.X + to.X) / 2, (from.Y + to.Y) / 2);
                            path.QuadraticBezierTo(new(center.X + (mid.X - center.X) * .89, center.Y + (mid.Y - center.Y) * .89), to, true, false);
                        }
                    }
                    dc.DrawGeometry(null, pen, mesh);
                }
            }
            dc.Pop();
        }
        dc.Pop(); dc.Pop(); dc.Pop();
    }
}
