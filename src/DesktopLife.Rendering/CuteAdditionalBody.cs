using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Rendering;

/// <summary>Rounded vector bodies, retaining the silhouettes and anchors of each insect group.</summary>
internal static class CuteAdditionalBody
{
    public static void Draw(DrawingContext dc, InsectDefinition insect)
    {
        var kind = insect.Kind;
        double length = insect.BodyLength, width = insect.BodyWidth;
        Brush ColorBrush(byte r, byte g, byte b) => new SolidColorBrush(Color.FromRgb(r, g, b));
        var outline = new Pen(ColorBrush(65, 89, 74), Math.Clamp(length / 90, 0.35, 0.6))
            { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
        var fill = kind switch
        {
            CreatureKind.Ladybug => ColorBrush(239, 129, 125),
            CreatureKind.GroundBeetle => ColorBrush(117, 188, 171),
            CreatureKind.Earwig => ColorBrush(224, 171, 116),
            CreatureKind.Silverfish => ColorBrush(184, 186, 214),
            CreatureKind.Cricket => ColorBrush(186, 147, 175),
            CreatureKind.Grasshopper => ColorBrush(160, 198, 111),
            CreatureKind.Mantis => ColorBrush(174, 207, 139),
            _ => ColorBrush(210, 178, 127)
        };
        var shine = ColorBrush(232, 242, 216);
        var headX = length * 0.38;
        var headRadiusX = length * 0.115;
        var headRadiusY = width * 0.48;
        switch (kind)
        {
            case CreatureKind.Ladybug:
                dc.DrawEllipse(fill, outline, new(-length * 0.1, 0), length * 0.39, width * 0.5);
                dc.DrawLine(outline, new(-length * 0.48, 0), new(length * 0.23, 0));
                for (var side = -1; side <= 1; side += 2)
                {
                    dc.DrawEllipse(outline.Brush, null, new(-length * 0.22, side * width * 0.23), 0.64, 0.64);
                    dc.DrawEllipse(outline.Brush, null, new(length * 0.05, side * width * 0.25), 0.51, 0.51);
                }
                headRadiusX = 1.5; headRadiusY = 2;
                break;
            case CreatureKind.Silverfish:
                for (var segment = 0; segment < 7; segment++)
                {
                    var t = segment / 6.0;
                    dc.DrawEllipse(fill, outline, new(length * (-0.45 + t * 0.7), 0), length * 0.095, width * (0.12 + t * 0.37));
                }
                break;
            case CreatureKind.Earwig:
                dc.DrawRoundedRectangle(fill, outline, new Rect(-length * 0.48, -width * 0.48, length * 0.68, width * 0.96), 2, 2);
                for (var segment = 0; segment < 4; segment++)
                    dc.DrawLine(outline, new(length * (-0.35 + segment * 0.12), -width * 0.4), new(length * (-0.35 + segment * 0.12), width * 0.4));
                dc.DrawEllipse(shine, outline, new(length * 0.2, 0), length * 0.12, width * 0.45);
                break;
            case CreatureKind.Mantis:
                dc.DrawEllipse(fill, outline, new(-length * 0.23, 0), length * 0.27, width * 0.5);
                dc.DrawLine(outline, new(-length * 0.46, 0), new(length * 0.015, 0));
                dc.DrawRoundedRectangle(fill, outline, new Rect(-length * 0.025, -width * 0.15, length * 0.44, width * 0.3), 1.1, 1.1);
                headX = length * 0.43;
                headRadiusX = length * 0.066; headRadiusY = width * 0.6;
                break;
            case CreatureKind.StickInsect:
                dc.DrawRoundedRectangle(fill, outline, new Rect(-length * 0.49, -width * 0.45, length * 0.93, width * 0.9), 1.6, 1.6);
                for (var joint = 0; joint < 5; joint++)
                {
                    var x = length * (-0.36 + joint * 0.16);
                    dc.DrawLine(outline, new(x, -width * 0.38), new(x + 0.7, width * 0.38));
                }
                headX = length * 0.44; headRadiusX = length * 0.055; headRadiusY = width * 0.68;
                break;
            default:
                dc.DrawEllipse(fill, outline, new(-length * 0.17, 0), length * 0.33, width * 0.52);
                dc.DrawEllipse(shine, outline, new(length * 0.19, 0), length * 0.14, width * 0.45);
                dc.DrawLine(outline, new(-length * 0.44, 0), new(length * 0.13, 0));
                if (kind == CreatureKind.GroundBeetle)
                    for (var side = -1; side <= 1; side += 2)
                        dc.DrawLine(new Pen(shine, 0.5), new(-length * 0.31, side * width * 0.23), new(length * 0.02, side * width * 0.23));
                else
                    for (var side = -1; side <= 1; side += 2)
                        dc.DrawLine(outline, new(-length * 0.38, 0), new(length * 0.07, side * width * 0.37));
                break;
        }
        if (kind == CreatureKind.Mantis)
        {
            var head = new StreamGeometry();
            using (var path = head.Open())
            {
                path.BeginFigure(new(headX - headRadiusX, 0), true, true);
                path.QuadraticBezierTo(new(headX + headRadiusX, -headRadiusY * 1.3), new(headX + headRadiusX, -headRadiusY * 0.6), true, false);
                path.LineTo(new(headX + headRadiusX, headRadiusY * 0.6), true, false);
                path.QuadraticBezierTo(new(headX + headRadiusX, headRadiusY * 1.3), new(headX - headRadiusX, 0), true, false);
            }
            head.Freeze(); dc.DrawGeometry(fill, outline, head);
        }
        else dc.DrawEllipse(fill, outline, new(headX, 0), headRadiusX, headRadiusY);
        for (var side = -1; side <= 1; side += 2)
        {
            var eye = new Point(headX + headRadiusX * 0.27, side * headRadiusY * 0.47);
            var radius = Math.Clamp(headRadiusY * 0.39, 0.6, 1.5);
            dc.DrawEllipse(Brushes.WhiteSmoke, null, eye, radius, radius);
            dc.DrawEllipse(outline.Brush, null, new(eye.X + radius * 0.25, eye.Y), radius * 0.5, radius * 0.65);
            dc.DrawEllipse(Brushes.White, null, new(eye.X + radius * 0.38, eye.Y - radius * 0.22), radius * 0.18, radius * 0.18);
        }
    }
}
