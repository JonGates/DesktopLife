using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Math;
using DesktopLife.Rendering;

namespace DesktopLife.Diagnostics;

internal static class SpiderSilkProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var renderer = new WpfCreatureRenderer();
        foreach (var style in new[] { CreatureStyle.Realistic, CreatureStyle.Cute })
        foreach (var dpi in new[] { 1.0, 1.25, 1.5 })
        {
            renderer.Style = style;
            var pose = new SilkPose();
            byte[] RenderThread()
            {
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen()) renderer.Render(dc, [pose], new(0, 0, 300, 200), 0, dpi, dpi);
                var bitmap = new RenderTargetBitmap(300, 200, dpi * 96, dpi * 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                var pixels = new byte[300 * 200 * 4]; bitmap.CopyPixels(pixels, 1200, 0); return pixels;
            }
            var pixels = RenderThread();
            if (pixels[(100 * 300 + 30) * 4 + 3] == 0) throw new Exception("Silk disappeared when spider was outside viewport");
            if (pixels[3] != 0) throw new Exception("Silk background lost transparency");
            if (!pixels.SequenceEqual(RenderThread())) throw new Exception("Paused silk changed");
            pose.Clear();
            if (RenderThread().Any(b => b != 0)) throw new Exception("Silk remains after clearing anchor");
            Console.WriteLine($"PASS: {style}, {dpi * 100}% DPI: cross-viewport thread, transparency, stable pause and cleanup");
        }
        foreach (var dark in new[] { false, true })
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var foreground = dark ? Brushes.Gainsboro : Brushes.DarkSlateGray;
                dc.DrawRectangle(dark ? new SolidColorBrush(Color.FromRgb(24, 30, 27)) : new SolidColorBrush(Color.FromRgb(241, 245, 240)), null, new Rect(0, 0, 1120, 400));
                void Text(string text, double x, double y, double size) => dc.DrawText(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei UI"), size, foreground, 1), new(x, y));
                Text("蜘蛛 · 射丝 → 牵引 → 收丝 → 爬行", 24, 18, 24);
                Text("实际模拟状态的 WPF 渲染示意，非桌面实录 · 100% 尺寸", 24, 54, 13);
                var times = new[] { 0.15f, 0.45f, 0.75f, 1.0f };
                var labels = new[] { "射丝", "沿丝牵引", "到达 / 丝网淡出", "恢复爬行" };
                for (var row = 0; row < 2; row++)
                {
                    renderer.Style = (CreatureStyle)row;
                    Text(row == 0 ? "写实" : "可爱", 24, 93 + row * 155, 13);
                    for (var column = 0; column < 4; column++)
                    {
                        var spider = new CrawlingInsect(new(30 + column * 280, 155 + row * 155), CreatureKind.Spider);
                        for (var t = 0f; t < times[column]; t += 0.01f)
                            spider.Update(0.01f, new(new(spider.Position - new Vector2(20, 0), Vector2.Zero, 0, false, TimeSpan.Zero), new(0, 0, 1120, 400), t, new MidpointRandom()));
                        renderer.Render(dc, [spider], new(0, 0, 1120, 400), times[column], 1, 1);
                        Text(labels[column], 30 + column * 280, 200 + row * 155, 14);
                    }
                }
            }
            var bitmap = new RenderTargetBitmap(1120, 400, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(output, dark ? "silk-dark.png" : "silk-light.png")); encoder.Save(file);
        }
    }
    private sealed class MidpointRandom : IRandomSource { public float NextFloat(float min, float max) => (min + max) / 2; }
    private sealed class SilkPose : Creature
    {
        public SilkPose() { Position = new(-180, 100); SilkAnchor = new(180, 100); MotionState = LocomotionState.SilkPulling; }
        public override CreatureKind Kind => CreatureKind.Spider;
        public void Clear() => SilkAnchor = null;
        public override void Update(float deltaTime, in CreatureContext context) { }
    }
}
