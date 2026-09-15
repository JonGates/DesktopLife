using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.Rendering;

/// <summary>Small, top-down companions. All frames share the same body anchor.</summary>
internal static class CuteInsectSprite
{
    public static DrawingGroup Create(CreatureKind kind, int phase, bool resting = false, bool grooming = true)
    {
        var drawing = new DrawingGroup();
        using (var dc = drawing.Open())
        {
            var outline = new Pen(new SolidColorBrush(Color.FromRgb(57, 69, 64)), 0.65);
            var leg = new Pen(new SolidColorBrush(Color.FromRgb(88, 101, 82)), 0.9) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            var angle = phase * Math.PI / 4;
            if (kind == CreatureKind.Caterpillar)
            {
                for (var i = 0; i < 7; i++)
                {
                    var wave = Math.Sin(angle - i * 0.8);
                    var x = -12 + i * 3.8 + wave * 0.45;
                    dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(210, 165, 91)), null, new(x, 4 + wave * 0.3), 1.3, 1);
                    dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(210, 165, 91)), null, new(x, -4 - wave * 0.3), 1.3, 1);
                    dc.DrawEllipse(new SolidColorBrush(Color.FromRgb((byte)(143 + i * 8), (byte)(185 + i * 5), 113)), outline, new(x, wave * 0.65), 3.5, 4);
                }
            }
            else
            {
                for (var side = -1; side <= 1; side += 2)
                for (var pair = 0; pair < 3; pair++)
                {
                    var step = Math.Sin(angle + pair * Math.PI + (side > 0 ? Math.PI : 0));
                    var root = new Point(3 - pair * 4, side * 2);
                    var knee = new Point(root.X + step, side * 5);
                    var foot = new Point(root.X - 2 + step * 1.4, side * 7);
                    if (kind == CreatureKind.Fly)
                    {
                        knee = new(root.X + 2, side * 5);
                        foot = new(root.X + 3, side * 6);
                        if (resting && grooming && pair == 0) { knee = new(9 + step, side * 4); foot = new(12 + step, side * 0.8); }
                    }
                    dc.DrawLine(leg, root, knee); dc.DrawLine(leg, knee, foot);
                }
                var fill = kind switch
                {
                    CreatureKind.Fly => Color.FromRgb(134, 170, 177),
                    CreatureKind.Ant => Color.FromRgb(207, 137, 104),
                    _ => Color.FromRgb(196, 151, 104)
                };
                var body = new SolidColorBrush(fill);
                dc.DrawEllipse(body, outline, new(-5, 0), kind == CreatureKind.Ant ? 4 : 8, kind == CreatureKind.Ant ? 3.3 : 5.8);
                dc.DrawEllipse(body, outline, new(2, 0), 4, 4);
                if (kind == CreatureKind.Cockroach)
                    dc.DrawLine(outline, new(-11, 0), new(0, 0));
                if (kind == CreatureKind.Fly)
                {
                    var wing = new SolidColorBrush(Color.FromArgb(resting ? (byte)170 : (byte)100, 227, 246, 248));
                    for (var side = -1; side <= 1; side += 2)
                    {
                        dc.PushTransform(new RotateTransform(side * (resting ? 16 : 35 + Math.Sin(angle) * 12), 1, 0));
                        dc.DrawEllipse(wing, new Pen(Brushes.LightSlateGray, 0.4), new(-5, side * (resting ? 2.5 : 6)), 7.5, 3.8);
                        dc.Pop();
                    }
                }
                else
                    for (var side = -1; side <= 1; side += 2)
                    {
                        dc.DrawLine(leg, new(8, side * 2), new(13, side * 5));
                        dc.DrawEllipse(body, null, new(13, side * 5), 0.9, 0.9);
                    }
            }
            var headX = kind == CreatureKind.Caterpillar ? 12 : 7;
            dc.DrawEllipse(new SolidColorBrush(kind == CreatureKind.Caterpillar ? Color.FromRgb(189, 214, 139) : Color.FromRgb(222, 198, 161)), outline, new(headX, 0), 4.2, 4);
            for (var side = -1; side <= 1; side += 2)
            {
                dc.DrawEllipse(Brushes.WhiteSmoke, null, new(headX + 1.6, side * 1.8), 1.7, 1.45);
                dc.DrawEllipse(Brushes.DarkSlateGray, null, new(headX + 2.2, side * 1.8), 0.8, 0.95);
                dc.DrawEllipse(Brushes.White, null, new(headX + 2.4, side * 1.8 - 0.3), 0.25, 0.25);
            }
        }
        drawing.Freeze();
        return drawing;
    }
}
