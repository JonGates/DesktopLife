using System.IO;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;
namespace DesktopLife.Diagnostics;

internal static class SeamProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var renderer = new WpfCreatureRenderer();
        foreach (var rightScale in new[] { 1.0, 1.25, 1.5 })
        {
            var creature = new SeamCreature(new(399, 200));
            ICreature[] creatures = [creature];
            var left = Render(new(0, 0, 400, 400), 400, 1, creatures);
            var right = Render(new(400, 0, 400, 400), 400, rightScale, creatures);
            var whole = Render(new(0, 0, 800, 400), 800, 1, creatures);
            var lp = Pixels(left, 400);
            var rp = Pixels(right, 400);
            var wp = Pixels(whole, 800);
            if (lp[(200 * 400 + 399) * 4 + 3] == 0 || rp[(200 * 400) * 4 + 3] == 0)
                throw new Exception("The same creature was not rendered on both sides of the seam");
            var difference = 0L;
            var alpha = 0L;
            for (var y = 0; y < 400; y++)
                for (var x = 0; x < 800; x++)
                {
                    var expected = wp[(y * 800 + x) * 4 + 3];
                    var actual = x < 400 ? lp[(y * 400 + x) * 4 + 3] : rp[(y * 400 + x - 400) * 4 + 3];
                    difference += Math.Abs(expected - actual);
                    alpha += expected;
                }
            if (difference > alpha * 0.04) throw new Exception($"Seam fragments do not reconstruct the full sprite at {rightScale}: {difference}/{alpha}");
            Save(right, Path.Combine(output, $"right-{rightScale * 100:F0}.png"));
            Console.WriteLine($"PASS: one creature across 100%/{rightScale * 100:F0}% viewports, no missing seam pixels; alpha difference {difference}/{alpha}.");
        }
        var offscreen = Render(new(400, 0, 400, 400), 400, 1, [new SeamCreature(new(200, 200))]);
        if (Pixels(offscreen, 400).Any(p => p != 0)) throw new Exception("Out-of-viewport creature painted the other screen");
        return;

        RenderTargetBitmap Render(WorldBounds bounds, int width, double dpiScale, IReadOnlyList<ICreature> creatures)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen()) renderer.Render(dc, creatures, bounds, 0.1f, dpiScale, dpiScale);
            var bitmap = new RenderTargetBitmap(width, 400, 96 * dpiScale, 96 * dpiScale, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }
    }
    private static byte[] Pixels(BitmapSource bitmap, int width)
    {
        var pixels = new byte[width * 400 * 4];
        bitmap.CopyPixels(pixels, width * 4, 0);
        return pixels;
    }
    private static void Save(BitmapSource bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(path);
        encoder.Save(file);
    }
    private sealed class SeamCreature : Creature
    {
        public override CreatureKind Kind => CreatureKind.Cockroach;
        public SeamCreature(Vector2 position) => Position = position;
        public override void Update(float deltaTime, in CreatureContext context) { }
    }
}
