using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;
using DesktopLife.Rendering;

namespace DesktopLife.Diagnostics;

internal static class OceanRenderProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var renderer = new WpfCreatureRenderer();
        var definitions = OceanCatalog.Fish.Prepend(OceanCatalog.Turtle).ToArray();
        foreach (var style in new[] { CreatureStyle.Realistic, CreatureStyle.Cute })
        {
            renderer.Style = style;
            foreach (var d in definitions)
            foreach (var dpi in new[] { 1d, 1.25, 1.5 })
            {
                byte[]? first = null;
                var changed = false;
                for (var frame = 0; frame < 8; frame++)
                {
                    var visual = new DrawingVisual();
                    using (var dc = visual.RenderOpen()) renderer.Render(dc, [new Sample(d.Kind, new(100, 100), frame / 8f)], new(0, 0, 200, 200), 0, dpi, dpi);
                    var bitmap = new RenderTargetBitmap(200, 200, 96 * dpi, 96 * dpi, PixelFormats.Pbgra32);
                    bitmap.Render(visual);
                    var pixels = new byte[200 * 200 * 4]; bitmap.CopyPixels(pixels, 800, 0);
                    if (pixels[3] != 0 || !pixels.Where((_, i) => i % 4 == 3).Any(a => a > 0)) throw new Exception($"Invalid sprite {d.Kind}");
                    if (first == null) first = pixels; else changed |= !first.SequenceEqual(pixels);
                }
                if (!changed) throw new Exception($"No animation: {style} {d.Kind}");
            }
            for (var frame = 0; frame < 8; frame++)
            {
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(17, 42, 56)), null, new Rect(0, 0, 1000, 850));
                    for (var i = 0; i < definitions.Length; i++)
                    {
                        var x = 125 + i % 4 * 250; var y = 100 + i / 4 * 200;
                        renderer.Render(dc, [new Sample(definitions[i].Kind, new(x, y), frame / 8f, 3, frame >= 4 ? MathF.PI - .3f : 0)], new(0, 0, 1000, 850), 0, 1, 1);
                        dc.DrawText(new FormattedText(definitions[i].EnglishName, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 15, Brushes.White, 1), new Point(x - 80, y + 70));
                    }
                }
                var bitmap = new RenderTargetBitmap(1000, 850, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var stream = File.Create(Path.Combine(output, $"{style}-{frame}.png")); encoder.Save(stream);
            }
        }
        Console.WriteLine("PASS: 13 ocean species, both styles, animated transparent sprites at 100/125/150% DPI");
    }
    private sealed class Sample : Creature
    {
        public override CreatureKind Kind { get; }
        public Sample(CreatureKind kind, Vector2 position, float phase, float scale = 1, float rotation = 0) { Kind = kind; Position = position; AnimationPhase = phase; Scale = scale; Rotation = rotation; }
        public override void Update(float deltaTime, in CreatureContext context) { }
    }
}
