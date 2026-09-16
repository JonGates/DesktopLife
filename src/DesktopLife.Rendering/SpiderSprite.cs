using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Rendering;

/// <summary>Four pairs of articulated walking legs anchored to the cephalothorax.</summary>
internal static class SpiderSprite
{
    private static readonly BitmapSource Body = LoadBody();

    public static DrawingGroup Create(InsectDefinition definition, int frame, bool cute)
    {
        double length = definition.BodyLength, width = definition.BodyWidth;
        var drawing = new DrawingGroup();
        using (var dc = drawing.Open())
        {
            var leg = new SolidColorBrush(cute ? Color.FromRgb(112, 91, 134) : Color.FromRgb(82, 62, 44));
            Pen Stroke(double thickness) => new(leg, thickness)
                { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
            for (var side = -1; side <= 1; side += 2)
            for (var pair = 0; pair < 4; pair++)
            {
                // Alternating groups of four: opposing legs are out of phase. Movement follows distance.
                var phase = (frame / 8.0 + pair * 0.5 + (side > 0 ? 0.5 : 0)) % 1;
                var stroke = phase < 0.65 ? 1 - 2 * phase / 0.65 : -Math.Cos((phase - 0.65) / 0.35 * Math.PI);
                var lift = phase < 0.65 ? 0 : Math.Sin((phase - 0.65) / 0.35 * Math.PI);
                var root = new Point(length * (0.31 - pair * 0.07), side * width * 0.21);
                var reach = length * (pair switch { 0 => 0.83, 1 => 0.36, 2 => -0.3, _ => -0.84 });
                var span = length * (pair is 1 or 2 ? 0.79 : 0.63);
                var knee = new Point(root.X * 0.35 + reach * 0.65 + stroke * 0.65, side * span * 0.6);
                var ankle = new Point(reach + stroke * 1.8, side * (span * 0.9 - lift * length * 0.065));
                var toe = new Point(ankle.X - length * 0.06, ankle.Y + side * length * 0.09);
                dc.DrawLine(Stroke(cute ? 1.15 : 0.95), root, knee);
                dc.DrawLine(Stroke(cute ? 0.85 : 0.65), knee, ankle);
                dc.DrawLine(Stroke(0.38), ankle, toe);
                if (!cute)
                {
                    for (var hair = 1; hair <= 4; hair++)
                    {
                        var t = hair / 5.0;
                        var p = new Point(knee.X + (ankle.X - knee.X) * t, knee.Y + (ankle.Y - knee.Y) * t);
                        dc.DrawLine(Stroke(0.15), p, new Point(p.X - 0.6, p.Y + side * 0.48));
                    }
                }
            }
            // Short pedipalps sit beside the mouth; these are not antennae or walking legs.
            for (var side = -1; side <= 1; side += 2)
                dc.DrawLine(Stroke(0.65), new(length * 0.39, side * width * 0.17), new(length * 0.56, side * width * 0.23));
            if (!cute) dc.DrawImage(Body, new Rect(-length / 2, -width / 2, length, width));
            else
            {
                var outline = Stroke(0.45);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(184, 157, 203)), outline, new(-length * 0.2, 0), length * 0.3, width * 0.49);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(158, 132, 184)), outline, new(length * 0.25, 0), length * 0.25, width * 0.4);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(220, 203, 231)), null, new(-length * 0.25, -width * 0.15), length * 0.11, width * 0.12);
                for (var side = -1; side <= 1; side += 2)
                {
                    dc.DrawEllipse(Brushes.Ivory, null, new(length * 0.37, side * width * 0.18), 1.25, 1.25);
                    dc.DrawEllipse(Brushes.Black, null, new(length * 0.405, side * width * 0.18), 0.6, 0.7);
                }
            }
        }
        drawing.Freeze();
        return drawing;
    }

    private static BitmapSource LoadBody()
    {
        using var stream = typeof(SpiderSprite).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Assets.spider-body.png")
            ?? throw new InvalidOperationException("Missing spider body texture");
        var decoded = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        var bitmap = new FormatConvertedBitmap(decoded, PixelFormats.Bgra32, null, 0);
        var stride = bitmap.PixelWidth * 4;
        var pixels = new byte[stride * bitmap.PixelHeight];
        bitmap.CopyPixels(pixels, stride, 0);
        var left = bitmap.PixelWidth; var right = -1; var top = bitmap.PixelHeight; var bottom = -1;
        for (var y = 0; y < bitmap.PixelHeight; y++)
        for (var x = 0; x < bitmap.PixelWidth; x++)
            if (pixels[y * stride + x * 4 + 3] > 16)
            { left = Math.Min(left, x); right = Math.Max(right, x); top = Math.Min(top, y); bottom = Math.Max(bottom, y); }
        if (left <= 0 || top <= 0 || right >= bitmap.PixelWidth - 1 || bottom >= bitmap.PixelHeight - 1 || right < left)
            throw new InvalidOperationException("Spider texture must be isolated on transparent margins");
        var body = new CroppedBitmap(bitmap, new Int32Rect(left, top, right - left + 1, bottom - top + 1));
        body.Freeze();
        return body;
    }
}
