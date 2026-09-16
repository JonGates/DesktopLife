using System.IO;
using System.Numerics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.App;
using DesktopLife.App.Settings;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;
using DesktopLife.ScreenSaver;
namespace DesktopLife.Diagnostics;

internal static class StyleProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var store = new SettingsStore(Path.Combine(output, "settings.json"));
        File.WriteAllText(store.PreferencesPath, "{}");
        if (store.LoadPreferences(out var invalid).Style != CreatureStyle.Realistic || invalid) throw new Exception("Legacy preferences");
        store.SavePreferences(new(StartHotkey: "", PauseHotkey: "", Style: CreatureStyle.Cute));
        if (store.LoadPreferences(out invalid).Style != CreatureStyle.Cute || invalid) throw new Exception("Desktop style round trip");
        var saver = new SaverSettingsStore(Path.Combine(output, "screensaver.json"));
        File.WriteAllText(saver.Path, "{}");
        if (saver.Load(out _).Style != CreatureStyle.Realistic) throw new Exception("Legacy saver preferences");
        saver.Save(new(Style: CreatureStyle.Cute));
        if (saver.Load(out _).Style != CreatureStyle.Cute) throw new Exception("Saver style round trip");
        File.WriteAllText(saver.Path, "{\"Style\":99}");
        if (saver.Load(out var warning).Style != CreatureStyle.Realistic || warning == null) throw new Exception("Invalid saver style");
        using (var host = new DesktopHost(app.Dispatcher, () => [new("probe", new(0, 0, 800, 600), true)], false))
        using (var preferences = new PreferencesController(host, store))
        {
            host.TogglePause();
            host.SynchronizeDisplays();
            if (host.Overlays.Any(w => w.InsectStyle != CreatureStyle.Cute)) throw new Exception("New overlay style inheritance");
            host.SetPopulation(new DesktopLife.Creatures.Displays.PopulationSettings(Additional:
                InsectCatalog.Additional.ToDictionary(x => x.Kind, _ => new DesktopLife.Creatures.Displays.SpeciesPopulation(2))));
            var positions = host.Simulation.World.Manager.Creatures.Select(c => (c.Id, c.Position)).ToArray();
            if (!preferences.SaveStyle(CreatureStyle.Realistic, out _)) throw new Exception("Style save");
            if (!positions.SequenceEqual(host.Simulation.World.Manager.Creatures.Select(c => (c.Id, c.Position)))) throw new Exception("Style reset population");
            var window = new SettingsWindow(host, store, preferences: preferences);
            var picker = (ComboBox)window.FindName("StylePicker");
            picker.SelectedIndex = 1;
            if (host.Style != CreatureStyle.Cute || host.Overlays.Any(w => w.InsectStyle != CreatureStyle.Cute) || store.LoadPreferences(out _).Style != CreatureStyle.Cute) throw new Exception("Style UI not applied");
            ((TextBox)window.FindName("RoachMin")).Text = "80";
            ((TextBox)window.FindName("RoachMax")).Text = "160";
            ((Button)window.FindName("ApplyButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if (store.Load(out _).RoachMin != 80 || host.Simulation.Settings.RoachMax != 160) throw new Exception("Size UI save failed");
            window.Left = -10000; window.Top = -10000; window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.ShowActivated = false; window.ShowInTaskbar = false; window.Show(); window.UpdateLayout();
            var ui = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            ui.Render(window);
            var uiEncoder = new PngBitmapEncoder(); uiEncoder.Frames.Add(BitmapFrame.Create(ui));
            using (var uiStream = File.Create(Path.Combine(output, "settings-compact.png"))) uiEncoder.Save(uiStream);
            var screenInfo = (Expander)window.FindName("ScreenInfoExpander");
            ((TabControl)window.FindName("SettingsPages")).SelectedIndex = 2;
            window.UpdateLayout();
            if (screenInfo.IsExpanded) throw new Exception("Screen information should start collapsed");
            screenInfo.IsExpanded = true; window.UpdateLayout();
            if (((System.Windows.Controls.Canvas)window.FindName("DisplayMap")).Children.Count == 0) throw new Exception("Expanded display map missing");
            screenInfo.IsExpanded = false;
            LanguageService.Apply("en-US"); window.Width = 470; window.Height = 600; window.UpdateLayout();
            var narrow = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            narrow.Render(window);
            var narrowEncoder = new PngBitmapEncoder(); narrowEncoder.Frames.Add(BitmapFrame.Create(narrow));
            using (var narrowStream = File.Create(Path.Combine(output, "settings-narrow-en.png"))) narrowEncoder.Save(narrowStream);
            window.Close();
        }
        saver.Save(new(Style: CreatureStyle.Cute));
        var config = new ConfigurationWindow(saver) { Left = -10000, Top = -10000, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false, ShowInTaskbar = false };
        config.Show(); config.UpdateLayout();
        var root = (DockPanel)config.Content;
        var content = (StackPanel)((ScrollViewer)root.Children[1]).Content;
        var styles = (ComboBox)config.FindName("StylePicker");
        if (styles.SelectedIndex != 1) throw new Exception("Saver style UI load");
        styles.SelectedIndex = 0;
        ((Button)((StackPanel)root.Children[0]).Children[1]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (saver.Load(out _).Style != CreatureStyle.Realistic) throw new Exception("Saver style UI save");
        var renderer = new WpfCreatureRenderer();
        var kinds = new[] { CreatureKind.Fly, CreatureKind.Cockroach, CreatureKind.Ant, CreatureKind.Caterpillar };
        for (var frame = 0; frame < 24; frame++)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(239, 244, 239)), null, new Rect(0, 0, 800, 390));
                Text(dc, "DesktopLife · Realistic / Cute", 24, 24, 23);
                Text(dc, "Rendered animation samples · 3x detail", 24, 60, 14);
                for (var row = 0; row < 2; row++)
                {
                    Text(dc, row == 0 ? "Realistic" : "Cute", 24, 130 + row * 150, 16);
                    renderer.Style = (CreatureStyle)row;
                    dc.PushTransform(new ScaleTransform(3, 3));
                    for (var k = 0; k < kinds.Length; k++)
                    {
                        var creature = new Sample(kinds[k], new(75 + k * 51, 48 + row * 50), frame / 8f % 1);
                        renderer.Render(dc, [creature], new(0, 0, 800, 400), 0, 1, 1);
                    }
                    dc.Pop();
                    for (var k = 0; k < kinds.Length; k++) Text(dc, kinds[k].ToString(), 195 + k * 153, 188 + row * 150, 13);
                }
            }
            var bitmap = new RenderTargetBitmap(800, 390, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(output, $"styles-{frame:00}.png")); encoder.Save(stream);
        }
        var realistic = new DrawingVisual();
        renderer.Style = CreatureStyle.Realistic;
        using (var dc = realistic.RenderOpen())
        {
            dc.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, 800, 420));
            Text(dc, "DesktopLife · Realistic insects", 24, 20, 23);
            Text(dc, "Generated body textures + articulated legs · WPF render samples", 24, 57, 14);
            for (var row = 0; row < 2; row++)
            {
                dc.DrawRectangle(row == 0 ? new SolidColorBrush(Color.FromRgb(29, 42, 49)) : new SolidColorBrush(Color.FromRgb(231, 240, 233)), null, new Rect(16, 88 + row * 155, 768, 145));
                for (var k = 0; k < kinds.Length; k++)
                {
                    dc.PushTransform(new ScaleTransform(3, 3));
                    renderer.Render(dc, [new Sample(kinds[k], new(48 + k * 59, 47 + row * 155 / 3f), 0.25f)], new(0, 0, 800, 420), 0, 1, 1);
                    dc.Pop();
                    renderer.Render(dc, [new Sample(kinds[k], new(144 + k * 177, 205 + row * 155), 0.25f)], new(0, 0, 800, 420), 0, 1, 1);
                }
            }
        }
        var sampleBitmap = new RenderTargetBitmap(800, 420, 96, 96, PixelFormats.Pbgra32);
        sampleBitmap.Render(realistic);
        var sampleEncoder = new PngBitmapEncoder(); sampleEncoder.Frames.Add(BitmapFrame.Create(sampleBitmap));
        using (var sampleStream = File.Create(Path.Combine(output, "realistic-insects.png"))) sampleEncoder.Save(sampleStream);
        var walker = new Sample(CreatureKind.Ant, Vector2.Zero, 0.3f);
        if (WpfCreatureRenderer.Frame(walker) != 2) throw new Exception("Distance-based gait frame");
        Console.WriteLine("PASS: legacy settings, style persistence, invalid fallback, desktop UI switch preserves creatures; 24 WPF style frames rendered.");
        app.Shutdown();
    }
    private static void Text(DrawingContext dc, string text, double x, double y, double size) => dc.DrawText(
        new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), size, Brushes.DarkSlateGray, 1), new(x, y));
    private sealed class Sample : Creature
    {
        public override CreatureKind Kind { get; }
        public Sample(CreatureKind kind, Vector2 position, float phase) { Kind = kind; Position = position; AnimationPhase = phase; }
        public override void Update(float deltaTime, in CreatureContext context) { }
    }
}
