using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.World;

namespace DesktopLife.Rendering;

/// <summary>Bounded image sampling and per-drop lenses; no desktop capture or per-frame full-screen filter.</summary>
public static class RainImageOptics
{
    private const int Size = 32;
    private sealed class Source
    {
        public required byte[] Pixels;
        public required int Width;
        public required int Height;
        public required BitmapSource Soft;
        public readonly ConditionalWeakTable<GlassDrop, Lens> Lenses = new();
    }
    private sealed class Lens
    {
        public BitmapSource? Bitmap;
        public Geometry? Shape;
        public Vector2 Position;
        public float Radius;
        public WorldBounds Viewport;
    }
    private static readonly ConditionalWeakTable<ImageSource, Source> Sources = new();
    private static readonly Dictionary<Geometry, byte[]> Masks = new();
    public static ImageSource SoftBackground(ImageSource image) => Sources.GetValue(image, Prepare).Soft;
    private static Source Prepare(ImageSource image)
    {
        var scale = Math.Min(1, 1024 / Math.Max(1, Math.Max(image.Width, image.Height)));
        var width = Math.Max(1, (int)(image.Width * scale));
        var height = Math.Max(1, (int)(image.Height * scale));
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen()) dc.DrawImage(image, new Rect(0, 0, width, height));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var pixels = new byte[width * height * 4]; bitmap.CopyPixels(pixels, width * 4, 0);
        // Bake soft focus only once, preserving the sharp source for the droplets.
        visual.Effect = new BlurEffect { Radius = 7, RenderingBias = RenderingBias.Performance };
        var soft = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        soft.Render(visual); soft.Freeze();
        return new Source { Pixels = pixels, Width = width, Height = height, Soft = soft };
    }
    internal static BitmapSource LensImage(ImageSource image, GlassDrop drop, WorldBounds viewport, Geometry shape)
    {
        var source = Sources.GetValue(image, Prepare);
        var lens = source.Lenses.GetOrCreateValue(drop);
        if (lens.Bitmap != null && ReferenceEquals(lens.Shape, shape) && lens.Viewport == viewport && Vector2.DistanceSquared(lens.Position, drop.Position) < 16 && Math.Abs(lens.Radius - drop.Radius) < .35f)
            return lens.Bitmap;
        if (!Masks.TryGetValue(shape, out var mask))
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.PushTransform(new TranslateTransform(Size / 2, Size / 2));
                dc.PushTransform(new ScaleTransform(Size / 2, Size / 2));
                dc.DrawGeometry(Brushes.White, null, shape);
            }
            var bitmap = new RenderTargetBitmap(Size, Size, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual);
            mask = new byte[Size * Size * 4]; bitmap.CopyPixels(mask, Size * 4, 0); Masks.Add(shape, mask);
        }
        var cover = Math.Max(viewport.Width / image.Width, viewport.Height / image.Height);
        var u = .5 + (drop.Position.X - viewport.Center.X) / (cover * image.Width);
        var v = .5 + (drop.Position.Y - viewport.Center.Y) / (cover * image.Height);
        var spanX = drop.Radius * 5 / (cover * image.Width);
        var spanY = drop.Radius * 5 / (cover * image.Height);
        var pixels = new byte[Size * Size * 4];
        for (var y = 0; y < Size; y++) for (var x = 0; x < Size; x++)
        {
            var offset = (y * Size + x) * 4;
            var alpha = mask[offset + 3]; if (alpha == 0) continue;
            var nx = (x + .5) * 2 / Size - 1; var ny = (y + .5) * 2 / Size - 1;
            // A curved lens compresses the surrounding image progressively at its rim.
            var radial = .48 + .68 * Math.Min(1, nx * nx + ny * ny);
            var sx = Math.Clamp((u - nx * radial * spanX) * (source.Width - 1), 0, source.Width - 1);
            var sy = Math.Clamp((v - ny * radial * spanY) * (source.Height - 1), 0, source.Height - 1);
            var ix = (int)sx; var iy = (int)sy; var fx = sx - ix; var fy = sy - iy;
            var right = Math.Min(ix + 1, source.Width - 1); var bottom = Math.Min(iy + 1, source.Height - 1);
            for (var c = 0; c < 4; c++)
            {
                double Sample(int px, int py) => source.Pixels[(py * source.Width + px) * 4 + c];
                var top = Sample(ix, iy) * (1 - fx) + Sample(right, iy) * fx;
                var low = Sample(ix, bottom) * (1 - fx) + Sample(right, bottom) * fx;
                pixels[offset + c] = (byte)Math.Clamp((top * (1 - fy) + low * fy) * alpha / 255, 0, 255);
            }
        }
        lens.Bitmap = BitmapSource.Create(Size, Size, 96, 96, PixelFormats.Pbgra32, null, pixels, Size * 4);
        lens.Bitmap.Freeze(); lens.Shape = shape; lens.Position = drop.Position; lens.Radius = drop.Radius; lens.Viewport = viewport;
        return lens.Bitmap;
    }
}
