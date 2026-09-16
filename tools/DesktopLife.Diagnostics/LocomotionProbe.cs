using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;
using DesktopLife.Rendering;

namespace DesktopLife.Diagnostics;

internal static class LocomotionProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var renderer = new WpfCreatureRenderer();
        var kinds = new[] { CreatureKind.Cricket, CreatureKind.Grasshopper, CreatureKind.Ladybug };
        // Verify each motion stage differs from walking in each style, at three physical display scales.
        foreach (var kind in kinds)
        foreach (var style in new[] { CreatureStyle.Realistic, CreatureStyle.Cute })
        foreach (var dpi in new[] { 1.0, 1.25, 1.5 })
        {
            renderer.Style = style;
            var walking = Pixels(RenderPose(renderer, new Pose(kind, new(128, 128)), dpi, 0));
            foreach (var t in kind == CreatureKind.Ladybug ? new[] { 0.5f, 1.5f, 2.7f } : new[] { 0.45f, 0.8f, 1.31f })
            {
                var pose = At(kind, t, new(128, 128), false);
                var pixels = Pixels(RenderPose(renderer, pose, dpi, t));
                if (walking.SequenceEqual(pixels)) throw new Exception($"Missing {kind} {pose.MotionState} {style} pose");
                if (pixels[3] != 0) throw new Exception("Motion background lost transparency");
                var occupied = Enumerable.Range(0, 256 * 256).Count(i => pixels[i * 4 + 3] > 0);
                if (occupied < 30) throw new Exception("Motion body missing");
            }
            Console.WriteLine($"PASS: {kind}, {style}, {dpi * 100}% DPI, distinct motion stages and transparency");
        }
        renderer.Style = CreatureStyle.Realistic;
        var flying = At(CreatureKind.Ladybug, 1.5f, new(128, 128), false);
        if (Pixels(RenderPose(renderer, flying, 1, 1.5f)).SequenceEqual(Pixels(RenderPose(renderer, flying, 1, 1.55f))))
            throw new Exception("Ladybug wings do not beat in flight");
        // A frozen simulation time must render a byte-identical flight pose.
        if (!Pixels(RenderPose(renderer, flying, 1, 1.5f)).SequenceEqual(Pixels(RenderPose(renderer, flying, 1, 1.5f))))
            throw new Exception("Paused flight render changed");
        for (var frame = 0; frame < 64; frame++)
        {
            var time = frame / 16f;
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(241, 245, 240)), null, new Rect(0, 0, 1200, 580));
                Text(dc, "DesktopLife · 跳跃与飞行", 24, 18, 26);
                Text(dc, "动作渲染示意（非桌面实录） · 3 倍细节 · 写实 / 可爱", 24, 57, 14);
                for (var row = 0; row < 2; row++)
                {
                    renderer.Style = (CreatureStyle)row;
                    Text(dc, row == 0 ? "写实" : "可爱", 24, 98 + row * 235, 14);
                    for (var column = 0; column < kinds.Length; column++)
                    {
                        var center = new Vector2((110 + column * 400) / 3f, (205 + row * 235) / 3f);
                        var pose = At(kinds[column], time, center, true);
                        dc.PushTransform(new ScaleTransform(3, 3));
                        renderer.Render(dc, [pose], new(0, 0, 1200, 580), time, 1, 1);
                        dc.Pop();
                        Text(dc, InsectCatalog.Get(kinds[column]).ChineseName + " · " + Label(pose.MotionState), 65 + column * 400, 257 + row * 235, 16);
                    }
                }
            }
            var bitmap = new RenderTargetBitmap(1200, 580, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual);
            Save(bitmap, Path.Combine(output, $"motion-{frame:00}.png"));
        }
    }
    private static string Label(LocomotionState state) => state switch
    {
        LocomotionState.JumpPreparing => "蓄力", LocomotionState.Jumping => "腾空", LocomotionState.JumpLanding => "落地缓冲",
        LocomotionState.TakingOff => "展翅起飞", LocomotionState.Flying => "飞行", LocomotionState.Landing => "降落收翅", _ => "行走"
    };
    private static Pose At(CreatureKind kind, float t, Vector2 origin, bool move)
    {
        var pose = new Pose(kind, origin);
        if (kind == CreatureKind.Ladybug)
        {
            if (t is >= 0.3f and < 0.7f) { pose.State = LocomotionState.TakingOff; pose.Progress = (t - 0.3f) / 0.4f; pose.Height = 12 * Math.Clamp((pose.Progress - 0.4f) / 0.6f, 0, 1); pose.Wings = Math.Clamp(pose.Progress / 0.4f, 0, 1); }
            else if (t is >= 0.7f and < 2.5f) { pose.State = LocomotionState.Flying; pose.Progress = (t - 0.7f) / 1.8f; pose.Height = 12; pose.Wings = 1; }
            else if (t is >= 2.5f and < 2.95f) { pose.State = LocomotionState.Landing; pose.Progress = (t - 2.5f) / 0.45f; pose.Height = 12 * (1 - Math.Clamp(pose.Progress / 0.65f, 0, 1)); pose.Wings = 1 - Math.Clamp((pose.Progress - 0.65f) / 0.35f, 0, 1); }
            if (move) pose.Offset = new(Math.Clamp((t - 0.3f) / 2.65f, 0, 1) * 50, 0);
        }
        else
        {
            if (t is >= 0.3f and < 0.55f) { pose.State = LocomotionState.JumpPreparing; pose.Progress = (t - 0.3f) / 0.25f; }
            else if (t is >= 0.55f and < 1.2f) { pose.State = LocomotionState.Jumping; pose.Progress = (t - 0.55f) / 0.65f; pose.Height = 4 * (kind == CreatureKind.Cricket ? 16 : 20) * pose.Progress * (1 - pose.Progress); }
            else if (t is >= 1.2f and < 1.42f) { pose.State = LocomotionState.JumpLanding; pose.Progress = (t - 1.2f) / 0.22f; }
            if (move) pose.Offset = new(Math.Clamp((t - 0.55f) / 0.65f, 0, 1) * 50, 0);
        }
        pose.Gait = t % 1;
        return pose;
    }
    private static RenderTargetBitmap RenderPose(WpfCreatureRenderer renderer, Pose pose, double dpi, float time)
    {
        var visual = new DrawingVisual(); using (var dc = visual.RenderOpen()) renderer.Render(dc, [pose], new(0, 0, 256, 256), time, dpi, dpi);
        var bitmap = new RenderTargetBitmap(256, 256, dpi * 96, dpi * 96, PixelFormats.Pbgra32); bitmap.Render(visual); return bitmap;
    }
    private static byte[] Pixels(BitmapSource bitmap) { var pixels = new byte[256 * 256 * 4]; bitmap.CopyPixels(pixels, 1024, 0); return pixels; }
    private static void Save(BitmapSource bitmap, string path) { var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var file = File.Create(path); encoder.Save(file); }
    private static void Text(DrawingContext dc, string value, double x, double y, double size) => dc.DrawText(new FormattedText(value, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Microsoft YaHei UI"), size, Brushes.DarkSlateGray, 1), new(x, y));
    private sealed class Pose(CreatureKind kind, Vector2 origin) : ICreature
    {
        public LocomotionState State; public float Progress, Height, Wings, Gait; public Vector2 Offset;
        public CreatureKind Kind => kind; public Guid Id { get; } = Guid.NewGuid(); public Vector2 Position => origin + Offset;
        public Vector2 Velocity => Vector2.Zero; public float Rotation => 0; public float Scale => 1; public bool IsVisible => true;
        public float AnimationPhase => Gait; public LocomotionState MotionState => State; public float MotionProgress => Progress;
        public float Elevation => Height; public float WingSpread => Wings;
        public void Update(float deltaTime, in CreatureContext context) { }
    }
}
