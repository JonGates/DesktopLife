using System.Numerics;
using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

internal static class SpiderSilkEffect
{
    private static readonly Pen Edge = MakePen(Color.FromArgb(105, 63, 80, 89), 1.6);
    private static readonly Pen Thread = MakePen(Color.FromArgb(230, 229, 237, 241), 0.65);
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
        dc.DrawLine(Edge, Local(start), Local(end));
        dc.DrawLine(Thread, Local(start), Local(end));
        // A small web marks the fixed landing point; it appears as the strand reaches its target.
        var webProgress = casting ? Math.Clamp((spider.MotionProgress - 0.75f) * 4, 0, 1) : 1;
        if (webProgress > 0)
        {
            dc.PushOpacity(webProgress);
            var center = Local(anchor);
            var radius = Math.Clamp(6 * spider.Scale, 4, 12);
            foreach (var pen in WebPens)
            {
                dc.DrawEllipse(null, pen, center, radius, radius);
                dc.DrawEllipse(null, pen, center, radius * 0.5, radius * 0.5);
                for (var spoke = 0; spoke < 8; spoke++)
                {
                    var angle = spoke * Math.PI / 4;
                    dc.DrawLine(pen, center, new(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius));
                }
            }
            dc.Pop();
        }
        dc.Pop(); dc.Pop(); dc.Pop();
    }
}
