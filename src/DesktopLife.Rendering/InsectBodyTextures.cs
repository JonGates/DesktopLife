using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace DesktopLife.Rendering;

/// <summary>Read the three rows of the original RGBA atlas, preserving its alpha.</summary>
internal static class InsectBodyTextures
{
    private static readonly BitmapSource[] Bodies = Load();
    public static BitmapSource Roach => Bodies[0];
    public static BitmapSource Ant => Bodies[1];
    public static BitmapSource Caterpillar => Bodies[2];

    private static BitmapSource[] Load()
    {
        using var stream = typeof(InsectBodyTextures).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Assets.insect-bodies.png")
            ?? throw new InvalidOperationException("Embedded insect body atlas is missing.");
        var decoded = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        var atlas = new FormatConvertedBitmap(decoded, PixelFormats.Bgra32, null, 0);
        var stride = atlas.PixelWidth * 4;
        var pixels = new byte[stride * atlas.PixelHeight];
        atlas.CopyPixels(pixels, stride, 0);
        var result = new BitmapSource[3];
        for (var row = 0; row < 3; row++)
        {
            var top = row * atlas.PixelHeight / 3;
            var bottom = (row + 1) * atlas.PixelHeight / 3;
            var left = atlas.PixelWidth; var right = -1; var first = bottom; var last = -1;
            for (var y = top; y < bottom; y++)
            for (var x = 0; x < atlas.PixelWidth; x++)
                if (pixels[y * stride + x * 4 + 3] > 16)
                {
                    left = Math.Min(left, x); right = Math.Max(right, x);
                    first = Math.Min(first, y); last = Math.Max(last, y);
                }
            if (right < left || last < first || left == 0 || first == top || last == bottom - 1)
                throw new InvalidOperationException("Insect atlas requires three isolated transparent rows.");
            // Keep a small alpha margin to avoid clipping soft hairs.
            left = Math.Max(0, left - 3); right = Math.Min(atlas.PixelWidth - 1, right + 3);
            first = Math.Max(top, first - 3); last = Math.Min(bottom - 1, last + 3);
            var body = new CroppedBitmap(atlas, new Int32Rect(left, first, right - left + 1, last - first + 1));
            body.Freeze(); result[row] = body;
        }
        return result;
    }
}
