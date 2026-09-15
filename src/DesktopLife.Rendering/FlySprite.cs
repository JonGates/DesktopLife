using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace DesktopLife.Rendering;

/// <summary>Photographic body and wings, articulated at the thorax; +X faces the head.</summary>
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
    public static DrawingGroup Create(bool resting, int phase = 0)
    {
        var frame = new CroppedBitmap(Atlas, new Int32Rect(resting ? 0 : Atlas.PixelWidth / 2, 0, Atlas.PixelWidth / 2, Atlas.PixelHeight));
        frame.Freeze();
        var drawing = new DrawingGroup();
        using (var dc = drawing.Open())
        {
            // 0.036 rather than 0.06: 40% smaller, with a fixed thorax anchor in all frames.
            dc.PushTransform(new MatrixTransform(0.036, 0, 0, 0.036, -18, -18.432));
            if (!resting)
            {
                for (var side = -1; side <= 1; side += 2)
                {
                    // Integrate several wing positions into each displayed frame, like a
                    // camera exposure. No single rigid wing swings back and forth slowly.
                    var hingeY = side < 0 ? 420 : 595;
                    for (var exposure = -2; exposure <= 2; exposure++)
                    {
                        dc.PushOpacity(0.13 + 0.015 * Math.Sin(phase * Math.PI / 4 + exposure));
                        dc.PushTransform(new ScaleTransform(1, 0.88 + 0.06 * Math.Sin(phase * Math.PI / 4), 440, hingeY));
                        dc.PushTransform(new RotateTransform(side * exposure * 10, 440, hingeY));
                        dc.PushClip(new RectangleGeometry(new Rect(0, side < 0 ? 0 : 595, 440, side < 0 ? 420 : 429)));
                        dc.DrawImage(frame, new Rect(0, 0, 768, 1024));
                        dc.Pop(); dc.Pop(); dc.Pop(); dc.Pop();
                    }                }
            }
            // Exclude the original front feet so the articulated feet replace them.
            var body = new GeometryGroup();
            body.Children.Add(new RectangleGeometry(new Rect(0, resting ? 0 : 420, 580, resting ? 1024 : 175)));
            body.Children.Add(new RectangleGeometry(new Rect(580, 442, 188, 150)));
            dc.PushClip(body);
            dc.DrawImage(frame, new Rect(0, 0, 768, 1024));
            dc.Pop();
            var leg = new Pen(new SolidColorBrush(Color.FromRgb(52, 43, 32)), 12) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            for (var side = -1; side <= 1; side += 2)
            {
                var rub = resting ? Math.Sin(phase * Math.PI / 4) : 0;
                var root = new Point(560, 512 + side * 80);
                var knee = new Point(660 + rub * 20, 512 + side * (resting ? 85 : 115));
                var foot = new Point(725 + rub * 14, 512 + side * (resting ? 12 + (1 + rub) * 15 : 100));
                dc.DrawLine(leg, root, knee); dc.DrawLine(leg, knee, foot);
            }
            dc.Pop();
        }
        RenderOptions.SetBitmapScalingMode(drawing, BitmapScalingMode.HighQuality);
        drawing.Freeze();
        return drawing;
    }
}
