using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.App;
using DesktopLife.App.Settings;
using DesktopLife.Creatures.Fly;
using DesktopLife.Creatures.Cockroach;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.Math;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;

namespace DesktopLife.Diagnostics;

/// <summary>Deterministic documentation frames, using production simulation and controls.</summary>
internal static class ReadmeDemo
{
    private static readonly WpfCreatureRenderer Renderer = new();
    public static void Run(string output)
    {
        output = Path.GetFullPath(output);
        foreach (var name in new[] { "fly", "multiscreen", "settings" }) Directory.CreateDirectory(Path.Combine(output, name));
        var bounds = new WorldBounds(0, 0, 320, 150);
        var fly = new FlyCreature(new(130, 75));
        var world = new SimulationWorld(bounds, new RandomSource(17), [fly]);
        var landedFrames = 0;
        for (var frame = 0; frame < 200; frame++)
        {
            var t = frame * .05f;
            var cursor = t < 2 ? new Vector2(150 + 25 * MathF.Sin(t * 2), 75) : t < 6 ? new Vector2(210, 95) : new Vector2(160 + 45 * MathF.Sin(t), 65);
            world.Update(.05f, cursor, frame == 40 ? new MouseClick(1, new(210, 95)) : null);
            if (fly.IsResting) landedFrames++;
            var phase = fly.IsResting ? "停落 3 秒 · 搓动前足" : t >= 2 && t < 3 ? "单击指定落点" : "跟随鼠标 · 窜飞与停悬";
            Draw(output, "fly", frame, "01 / 苍蝇互动", phase, dc =>
            {
                Renderer.Render(dc, world.Manager.Creatures, bounds, world.TotalTime, 1, 1);
                dc.DrawGeometry(Brushes.White, new Pen(Brushes.SlateGray, .7), Geometry.Parse($"M {cursor.X.ToString(CultureInfo.InvariantCulture)},{cursor.Y.ToString(CultureInfo.InvariantCulture)} l 0,12 3,-3 3,5 2,-1 -3,-5 4,0 Z"));
                if (frame is >= 40 and < 50) dc.DrawEllipse(null, new Pen(Brushes.CornflowerBlue, 1), new Point(210, 95), 12, 12);
            });
        }
        if (landedFrames < 59 || landedFrames > 61) throw new Exception($"Unexpected landing duration: {landedFrames} frames");

        ICreature[] insects = [new CockroachCreature(new(90, 45)), new CrawlingInsect(new(100, 85), CreatureKind.Ant), new CrawlingInsect(new(130, 120), CreatureKind.Caterpillar)];
        var layout = new DesktopLayout([new("left", new(0, 0, 160, 150), true), new("right", new(160, 0, 160, 150))]);
        world = new SimulationWorld(bounds, new StraightRandom(), insects) { Layout = layout };
        var crossed = new HashSet<Guid>();
        for (var frame = 0; frame < 100; frame++)
        {
            world.Update(.05f, new(-1000, -1000));
            foreach (var insect in insects) if (insect.Position.X > 160) crossed.Add(insect.Id);
            Draw(output, "multiscreen", frame, "02 / 共享桌面 · 跨屏移动", "同一批昆虫，自由穿过相接的屏幕边缘", dc =>
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(224, 237, 250)), null, new Rect(0, 0, 160, 150));
                dc.DrawLine(new Pen(Brushes.SlateGray, .6), new(160, 0), new(160, 150));
                Label(dc, "屏幕 1", 10, 8, 7); Label(dc, "屏幕 2", 170, 8, 7);
                Renderer.Render(dc, insects, bounds, world.TotalTime, 1, 1);
            });
        }
        if (crossed.Count != 3) throw new Exception("Not every insect crossed the screen seam");
        Settings(output);
        Console.WriteLine($"PASS: 3 demo sequences; fly landed for {landedFrames * .05:F2}s; all 3 crawling species crossed the seam.");
    }

    private static void Draw(string output, string name, int frame, string title, string subtitle, Action<DrawingContext> draw)
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(244, 247, 251)), null, new Rect(0, 0, 680, 410));
            Label(dc, title, 22, 15, 23); Label(dc, subtitle, 22, 49, 14);
            dc.PushTransform(new TranslateTransform(20, 85)); dc.PushTransform(new ScaleTransform(2, 2));
            draw(dc); dc.Pop(); dc.Pop();
            Label(dc, "DesktopLife · 实际引擎演示 / 2× 放大", 22, 388, 11);
        }
        Save(visual, 680, 410, Path.Combine(output, name, $"{frame:D4}.png"));
    }

    private static void Settings(string output)
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        using var host = new DesktopHost(app.Dispatcher, observeMouseClicks: false);
        host.Simulation.Synchronize([new("left", new(0, 0, 1920, 1080), true), new("right", new(1920, 0, 1920, 1080))]);
        var store = new SettingsStore(Path.Combine(output, "demo-settings.json"));
        store.SavePreferences(new("zh-CN", "", ""));
        using var preferences = new PreferencesController(host, store);
        var window = new SettingsWindow(host, store, preferences: preferences);
        // Render the actual content without opening a desktop window or registering shortcuts.
        var content = (FrameworkElement)window.Content;
        content.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        for (var frame = 0; frame < 6; frame++)
        {
            if (frame == 1)
            {
                ((TextBox)window.FindName("AntCount")).Text = "50";
                ((Button)window.FindName("ApplyButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                if (host.Simulation.TotalAntCount != 50) throw new Exception("Demo quantity was not applied");
            }
            if (frame == 2) ((ComboBox)window.FindName("LanguagePicker")).SelectedIndex = 1;
            if (frame == 3) FindScroll(content)?.ScrollToEnd();
            if (frame == 4) ((ComboBox)window.FindName("LanguagePicker")).SelectedIndex = 0;
            if (frame == 5) FindScroll(content)?.ScrollToTop();
            content.Measure(new Size(540, 820)); content.Arrange(new Rect(0, 0, 540, 820)); content.UpdateLayout();
            Save(content, 540, 820, Path.Combine(output, "settings", $"{frame:D4}.png"), window.Background);
        }
        window.Close(); app.Shutdown();
    }
    private static ScrollViewer? FindScroll(DependencyObject parent)
    {
        if (parent is ScrollViewer scroll) return scroll;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) if (FindScroll(VisualTreeHelper.GetChild(parent, i)) is { } found) return found;
        return null;
    }
    private static void Label(DrawingContext dc, string text, double x, double y, double size) => dc.DrawText(new FormattedText(text, CultureInfo.GetCultureInfo("zh-CN"), FlowDirection.LeftToRight, new Typeface("Microsoft YaHei UI"), size, new SolidColorBrush(Color.FromRgb(30, 55, 82)), 1), new(x, y));
    private static void Save(Visual visual, int width, int height, string path, Brush? background = null)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        if (background != null)
        {
            var backdrop = new DrawingVisual();
            using (var dc = backdrop.RenderOpen()) dc.DrawRectangle(background, null, new Rect(0, 0, width, height));
            bitmap.Render(backdrop);
        }
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }
    private sealed class StraightRandom : IRandomSource { public float NextFloat(float min, float max) => max == MathF.Tau ? 0 : (min + max) / 2; }
}
