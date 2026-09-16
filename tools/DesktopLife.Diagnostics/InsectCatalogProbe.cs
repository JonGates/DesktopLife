using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;
using DesktopLife.Rendering;

namespace DesktopLife.Diagnostics;

internal static class InsectCatalogProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var renderer = new WpfCreatureRenderer();
        foreach (var insect in InsectCatalog.Additional)
        {
            byte[]? previous = null;
            for (var frame = 0; frame < 8; frame++)
            {
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen()) renderer.Render(dc, [new Pose(insect.Kind, new(128, 128), frame / 8f)], new(0, 0, 256, 256), 0, 1, 1);
                var bitmap = Render(visual, 256, 256);
                var pixels = new byte[256 * 256 * 4]; bitmap.CopyPixels(pixels, 1024, 0);
                if (pixels[3] != 0 || pixels[(128 * 256 + 128) * 4 + 3] == 0) throw new Exception("Body/alpha failure: " + insect.Kind);
                if (previous != null && previous.SequenceEqual(pixels)) throw new Exception("No animated appendages: " + insect.Kind);
                previous = pixels;
            }
            // Pausing walkers must retain their gait pose instead of using fly grooming frames.
            var paused = new Pose(insect.Kind, new(128, 128), 0.625f, true);
            if (WpfCreatureRenderer.Frame(paused) != 5) throw new Exception("Walker pose changed while paused");
            Console.WriteLine("PASS: " + insect.Kind + " body, transparent background, eight moving poses and stable pause");
        }
        for (var dark = 0; dark < 2; dark++)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var foreground = dark == 1 ? Brushes.Gainsboro : Brushes.DarkSlateGray;
                dc.DrawRectangle(dark == 1 ? new SolidColorBrush(Color.FromRgb(24, 30, 27)) : new SolidColorBrush(Color.FromRgb(241, 245, 240)), null, new Rect(0, 0, 1440, 800));
                Text(dc, "DesktopLife · 12 种昆虫", 28, 20, 26, foreground);
                Text(dc, "WPF 渲染示意 · 上方 3 倍细节，下方原始尺寸 · 默认 100% 身体比例", 28, 60, 14, foreground);
                var kinds = new[] { CreatureKind.Fly, CreatureKind.Cockroach, CreatureKind.Ant, CreatureKind.Caterpillar }.Concat(InsectCatalog.Additional.Select(x => x.Kind)).ToArray();
                var names = new[] { "苍蝇 / Fly", "蟑螂 / Cockroach", "蚂蚁 / Ant", "毛毛虫 / Caterpillar" }.Concat(InsectCatalog.Additional.Select(x => x.ChineseName + " / " + x.EnglishName)).ToArray();
                for (var i = 0; i < kinds.Length; i++)
                {
                    var x = 180 + i % 4 * 360;
                    var y = 155 + i / 4 * 218;
                    dc.PushTransform(new ScaleTransform(3, 3));
                    var detailX = kinds[i] == CreatureKind.StickInsect ? x - 60 : x;
                    renderer.Render(dc, [new Pose(kinds[i], new(detailX / 3f, y / 3f), 0.25f)], new(0, 0, 1440, 800), 0, 1, 1);
                    dc.Pop();
                    renderer.Render(dc, [new Pose(kinds[i], new(x, y + 77), 0.25f)], new(0, 0, 1440, 800), 0, 1, 1);
                    Text(dc, names[i], x - 100, y + 100, 14, foreground);
                }
            }
            Save(Render(visual, 1440, 800), Path.Combine(output, dark == 1 ? "insects-dark.png" : "insects-light.png"));
        }
    }
    private static void Text(DrawingContext dc, string value, double x, double y, double size, Brush brush) =>
        dc.DrawText(new FormattedText(value, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei UI"), size, brush, 1), new(x, y));
    private static RenderTargetBitmap Render(Visual visual, int width, int height)
    { var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual); return bitmap; }
    private static void Save(BitmapSource bitmap, string path)
    { var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var file = File.Create(path); encoder.Save(file); }
    private sealed class Pose(CreatureKind kind, Vector2 position, float phase, bool resting = false) : ICreature
    {
        public CreatureKind Kind => kind;
        public Guid Id { get; } = Guid.NewGuid();
        public Vector2 Position => position;
        public Vector2 Velocity => Vector2.Zero;
        public float Rotation => 0;
        public float Scale => 1;
        public bool IsVisible => true;
        public bool IsResting => resting;
        public float AnimationPhase => phase;
        public float RestingSeconds => 1;
        public void Update(float deltaTime, in CreatureContext context) { }
    }
}
