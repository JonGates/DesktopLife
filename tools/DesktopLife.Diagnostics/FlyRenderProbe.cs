using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;
namespace DesktopLife.Diagnostics;

internal static class FlyRenderProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var renderer = new WpfCreatureRenderer();
        foreach (var scale in new[] { 1.0, 1.25, 1.5 })
        {
            var flight = Render(false, scale);
            var landed = Render(true, scale);
            if (Pixels(flight).SequenceEqual(Pixels(Render(false, scale, 0.2f)))) throw new Exception("Wing animation is static");
            if (Pixels(landed).SequenceEqual(Pixels(Render(true, scale, 0.25f)))) throw new Exception("Grooming animation is static");
            var a = Pixels(flight);
            var b = Pixels(landed);
            if (a.SequenceEqual(b)) throw new Exception("Flying and resting poses are identical");
            if (a[3] != 0 || b[3] != 0) throw new Exception("Fly texture contains a solid background");
            var center = (80 * 160 + 80) * 4 + 3;
            if (a[center] < 150 || b[center] < 150) throw new Exception("Fly thorax is not at the landing anchor");
            if (a.Where((v, i) => i % 4 == 3 && v > 10).Count() > 900) throw new Exception("Sprite unexpectedly fills its texture rectangle");
            Save(flight, Path.Combine(output, $"fly-flight-{scale * 100:F0}.png"));
            Save(landed, Path.Combine(output, $"fly-landed-{scale * 100:F0}.png"));
            Console.WriteLine($"PASS: {scale * 100:F0}% realistic flying/resting poses differ, transparent corners, opaque landing anchor.");
        }
        var sheet = new DrawingVisual();
        using (var dc = sheet.RenderOpen())
        {
            dc.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, 640, 320));
            for (var row = 0; row < 4; row++)
            for (var phase = 0; phase < 8; phase++)
            {
                var kind = row < 2 ? CreatureKind.Fly : row == 2 ? CreatureKind.Ant : CreatureKind.Caterpillar;
                renderer.Render(dc, [new PoseCreature(new(40 + phase * 80, 40 + row * 80), row == 1, kind)], new(0, 0, 640, 320), phase / (row == 0 ? 53f : row == 1 ? 18f : 12f), 1, 1);
            }
        }
        var sheetBitmap = new RenderTargetBitmap(1280, 640, 192, 192, PixelFormats.Pbgra32);
        sheetBitmap.Render(sheet);
        Save(sheetBitmap, Path.Combine(output, "animation-phases.png"));
        var preview = new DrawingVisual();
        using (var dc = preview.RenderOpen())
        {
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(243, 246, 250)), null, new Rect(0, 0, 400, 180));
            renderer.Render(dc, [new PoseCreature(new(100, 90), true), new PoseCreature(new(300, 90), false)], new(0, 0, 400, 180), 0, 1, 1);
        }
        var bitmap = new RenderTargetBitmap(400, 180, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(preview);
        Save(bitmap, Path.Combine(output, "fly-poses-on-light.png"));
        return;
        RenderTargetBitmap Render(bool resting, double scale, float time = 0.1f)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
                renderer.Render(dc, [new PoseCreature(new(-920, 80), resting)], new(-1000, 0, 160, 160), time, scale, scale);
            var bitmap = new RenderTargetBitmap(160, 160, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }
    }
    private static byte[] Pixels(BitmapSource bitmap)
    {
        var pixels = new byte[160 * 160 * 4];
        bitmap.CopyPixels(pixels, 160 * 4, 0);
        return pixels;
    }
    private static void Save(BitmapSource bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
    private sealed class PoseCreature : Creature
    {
        public override CreatureKind Kind { get; }
        private readonly bool _resting;
        public override bool IsResting => _resting;
        public PoseCreature(Vector2 position, bool resting, CreatureKind kind = CreatureKind.Fly) { Position = position; _resting = resting; Kind = kind; }
        public override void Update(float deltaTime, in CreatureContext context) { }
    }
}
