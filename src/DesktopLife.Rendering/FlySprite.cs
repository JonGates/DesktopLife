using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopLife.Rendering;

/// <summary>Generated photographic atlas, decoded once and cached; local +X points toward the head.</summary>
internal static class FlySprite
{
    private static readonly BitmapSource Atlas = LoadAtlas();
    private static BitmapSource LoadAtlas()
    {
        using var stream = typeof(FlySprite).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Assets.fly-atlas.png")
            ?? throw new InvalidOperationException("Embedded fly atlas is missing.");
        var bitmap = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        bitmap.Freeze();
        return bitmap;
    }
    public static DrawingGroup Create(bool resting)
    {
        var cellWidth = Atlas.PixelWidth / 2;
        var frame = new CroppedBitmap(Atlas, new Int32Rect(resting ? 0 : cellWidth, 0, cellWidth, Atlas.PixelHeight));
        frame.Freeze();
        var drawing = new DrawingGroup();
        // Both poses share a thorax anchor near (500,512) in each source cell.
        // About 40px leg span; the PNG's transparent padding is not visible desktop area.
        using (var dc = drawing.Open()) dc.DrawImage(frame, new Rect(-30, -30.72, cellWidth * 0.06, Atlas.PixelHeight * 0.06));
        RenderOptions.SetBitmapScalingMode(drawing, BitmapScalingMode.HighQuality);
        drawing.Freeze();
        return drawing;
    }
}
