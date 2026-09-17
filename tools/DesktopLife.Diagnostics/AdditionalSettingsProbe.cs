using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.App;
using DesktopLife.App.Settings;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.ScreenSaver;

namespace DesktopLife.Diagnostics;

internal static class AdditionalSettingsProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var store = new SettingsStore(Path.GetFullPath(Path.Combine(output, "population.json")));
        var saverStore = new SaverSettingsStore(Path.GetFullPath(Path.Combine(output, "saver.json")));
        var additional = InsectCatalog.Additional.ToDictionary(d => d.Kind, _ => new SpeciesPopulation(2, 70, 140));
        void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        store.Save(new(Additional: additional));
        saverStore.Save(new(Additional: additional));
        foreach (var definition in InsectCatalog.Additional)
        {
            Require(store.Load(out _).GetAdditional(definition.Kind) == additional[definition.Kind], "Desktop round-trip failed");
            Require(saverStore.Load(out _).Population.GetAdditional(definition.Kind) == additional[definition.Kind], "Saver round-trip failed");
        }
        foreach (var malformed in new[] { "{\"Additional\":{\"Ladybug\":null}}", "{\"Additional\":{\"Ladybug\":{\"Count\":101}}}", "{\"Additional\":{\"Ladybug\":{\"MinPercent\":200,\"MaxPercent\":100}}}" })
        {
            File.WriteAllText(store.FilePath, malformed); File.WriteAllText(saverStore.Path, malformed);
            Require(store.Load(out var warning).Additional == null && warning != null, "Desktop malformed fallback failed");
            Require(saverStore.Load(out warning).Additional == null && warning != null, "Saver malformed fallback failed");
        }
        File.WriteAllText(store.FilePath, "{\"Cockroaches\":7}");
        File.WriteAllText(saverStore.Path, "{\"Cockroaches\":7}");
        Require(store.Load(out _).Cockroaches == 7 && saverStore.Load(out _).Cockroaches == 7, "Legacy migration failed");
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        store.SavePreferences(new(StartHotkey: "", PauseHotkey: ""));
        LanguageService.Apply("zh-CN");
        using var host = new DesktopHost(app.Dispatcher);
        var window = new SettingsWindow(host, store);
        TextBox Input(string suffix) => (TextBox)window.FindName("Ladybug" + suffix);
        void Apply() => ((Button)window.FindName("ApplyButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        ((TextBox)window.FindName("SpiderCount")).Text = "4";
        ((TextBox)window.FindName("SpiderMin")).Text = "60";
        ((TextBox)window.FindName("SpiderMax")).Text = "140";
        Input("Count").Text = "2"; Input("Min").Text = "70"; Input("Max").Text = "140"; Apply();
        Require(host.Simulation.Settings.GetAdditional(CreatureKind.Spider) == new SpeciesPopulation(4, 60, 140), "Spider desktop UI apply failed");
        Require(host.Simulation.Settings.GetAdditional(CreatureKind.Ladybug) == new SpeciesPopulation(2, 70, 140), "Desktop UI apply failed");
        var saved = File.ReadAllText(store.FilePath);
        foreach (var invalid in new[] { "-1", "101", "1.5", "abc", "" })
        {
            Input("Count").Text = invalid; Apply();
            Require(File.ReadAllText(store.FilePath) == saved && host.Simulation.Settings.GetAdditional(CreatureKind.Ladybug).Count == 2, "Invalid desktop count mutated settings");
        }
        Input("Count").Text = "2"; Input("Min").Text = "200"; Apply();
        Require(File.ReadAllText(store.FilePath) == saved, "Invalid desktop size saved");
        LanguageService.Apply("en-US");
        Require(AutomationProperties.GetName(Input("Count")).Contains("Ladybug"), "Live translation failed");
        Require(AutomationProperties.GetName((TextBox)window.FindName("SpiderCount")).Contains("Spider"), "Spider live translation failed");
        Require(Input("Min").Text == "200", "Translation lost unsaved values");
        Input("Min").Text = "70"; Apply();
        ShowOffscreen(window);
        foreach (var language in new[] { "zh-CN", "en-US" })
        foreach (var size in new[] { new Size(470, 600), new Size(540, 760) })
        {
            LanguageService.Apply(language); window.Width = size.Width; window.Height = size.Height; window.UpdateLayout();
            var scroll = ((DockPanel)window.Content).Children.OfType<ScrollViewer>().Single();
            var section = (Expander)window.FindName("AdditionalSpeciesExpander");
            var position = section.TranslatePoint(new Point(), (UIElement)scroll.Content);
            scroll.ScrollToVerticalOffset(Math.Max(0, position.Y - 10)); window.UpdateLayout();
            SaveWindow(window, Path.Combine(output, $"additional-{language}-{size.Width}x{size.Height}.png"));
            foreach (var definition in InsectCatalog.Additional)
            foreach (var suffix in new[] { "Count", "Min", "Max" })
            {
                var field = (TextBox)window.FindName(definition.Kind + suffix);
                Require(field.ActualWidth >= 48 && field.ActualHeight >= 26, "Additional field clipped or undersized");
            }
        }
        window.Close();

        saverStore.Save(new(Additional: additional));
        var saver = new ConfigurationWindow(saverStore);
        // Enumerate logical children without showing a window or taking over the screen.
        IEnumerable<DependencyObject> Descendants(DependencyObject parent)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
            { yield return child; foreach (var descendant in Descendants(child)) yield return descendant; }
        }
        var fields = Descendants(saver).OfType<TextBox>().ToArray();
        var button = (Button)saver.FindName("SaveButton");
        var ladybug = fields.Single(field => field.Name == "LadybugCount");
        fields.Single(field => field.Name == "SpiderCount").Text = "5";
        fields.Single(field => field.Name == "SpiderMin").Text = "60";
        fields.Single(field => field.Name == "SpiderMax").Text = "140";
        saved = File.ReadAllText(saverStore.Path);
        ladybug.Text = "101"; button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Require(File.ReadAllText(saverStore.Path) == saved, "Invalid saver count saved");
        ladybug.Text = "3";
        fields.Single(field => field.Name == "LadybugMin").Text = "200";
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Require(File.ReadAllText(saverStore.Path) == saved, "Invalid saver size saved");
        fields.Single(field => field.Name == "LadybugMin").Text = "70";
        ShowOffscreen(saver);
        var saverScroll = ((DockPanel)saver.Content).Children.OfType<ScrollViewer>().Single();
        var saverPosition = ladybug.TranslatePoint(new Point(), (UIElement)saverScroll.Content);
        saverScroll.ScrollToVerticalOffset(Math.Max(0, saverPosition.Y - 150)); saver.UpdateLayout();
        SaveWindow(saver, Path.Combine(output, "additional-screensaver.png"));
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Require(saverStore.Load(out _).Population.GetAdditional(CreatureKind.Ladybug) == new SpeciesPopulation(3, 70, 140), "Saver UI apply failed");
        Require(saverStore.Load(out _).Population.GetAdditional(CreatureKind.Spider) == new SpeciesPopulation(5, 60, 140), "Spider saver UI apply failed");
        app.Shutdown();
        File.WriteAllText(Path.Combine(output, "result.txt"), $"PASS: all {InsectCatalog.Additional.Count} additional species round-trip, old JSON migration, invalid JSON fallback, desktop/saver UI validation without mutation, save/apply, and live translation.");
        Console.WriteLine(File.ReadAllText(Path.Combine(output, "result.txt")));
    }

    private static void ShowOffscreen(Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = -10000; window.Top = -10000;
        window.ShowActivated = false; window.ShowInTaskbar = false;
        window.Show(); window.UpdateLayout();
    }

    private static void SaveWindow(Window window, string path)
    {
        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }
}
