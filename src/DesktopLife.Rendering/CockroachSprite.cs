using System.Windows;
using System.Windows.Media;
namespace DesktopLife.Rendering;

/// <summary>Generated macro texture over a six-leg articulated gait; +X faces forward.</summary>
internal static class CockroachSprite
{
    public static DrawingGroup Create(bool alternateStep) => Create(alternateStep ? 4 : 0);
    public static DrawingGroup Create(int phase)
    {
        var drawing = new DrawingGroup();
        using (var dc = drawing.Open())
        {
            WalkingAppendages.Draw(dc, phase, false);
            dc.DrawImage(InsectBodyTextures.Roach, new Rect(-15, -4.9, 28, 9.8));
        }
        RenderOptions.SetBitmapScalingMode(drawing, BitmapScalingMode.HighQuality);
        drawing.Freeze();
        return drawing;
    }
}
