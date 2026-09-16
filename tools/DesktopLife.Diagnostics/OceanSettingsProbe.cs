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

internal static class OceanSettingsProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        var store = new SettingsStore(Path.GetFullPath(Path.Combine(output, "ocean-population.json")));
        var saverStore = new SaverSettingsStore(Path.GetFullPath(Path.Combine(output, "ocean-saver.json")));
        store.SavePreferences(new(StartHotkey: "", PauseHotkey: ""));
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        LanguageService.Apply("zh-CN");
        using var host = new DesktopHost(app.Dispatcher);
        host.SetPopulation(new(Cockroaches: 7));
        var window = new SettingsWindow(host, store);
        TextBox Field(string name) => (TextBox)window.FindName(name);
        var tabs = (TabControl)window.FindName("HabitatTabs");
        void Apply() => ((Button)window.FindName("ApplyButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Field("RoachCount").Text = "9";
        Field("ClownfishCount").Text = "5";
        Field("ClownfishMin").Text = "65";
        Field("ClownfishMax").Text = "130";
        tabs.SelectedIndex = 1;
        Require(host.Simulation.Settings.Habitat == Habitat.Ocean, "Tab did not immediately change habitat");
        Require(store.Load(out _).Cockroaches == 9, "Switch lost unsaved forest changes");
        Require(store.Load(out _).GetOcean(CreatureKind.Clownfish) == new SpeciesPopulation(5, 65, 130), "Ocean population not saved");
        var saved = File.ReadAllText(store.FilePath);
        foreach (var invalid in new[] { "-1", "101", "1.5", "abc", "" })
        {
            Field("ClownfishCount").Text = invalid;
            tabs.SelectedIndex = 0;
            Require(tabs.SelectedIndex == 1 && host.Simulation.Settings.Habitat == Habitat.Ocean, "Failed validation changed active scene");
            Require(Field("ClownfishCount").Text == invalid && File.ReadAllText(store.FilePath) == saved, "Failed validation lost edit or mutated file");
        }
        Field("ClownfishCount").Text = "5"; Field("ClownfishMin").Text = "200"; Apply();
        Require(File.ReadAllText(store.FilePath) == saved, "Invalid ocean size saved");
        LanguageService.Apply("en-US");
        Require(AutomationProperties.GetName(Field("ClownfishCount")).Contains("Clownfish"), "Ocean live translation failed");
        Require(Field("ClownfishMin").Text == "200", "Translation lost invalid edit");
        Field("ClownfishMin").Text = "65";
        tabs.SelectedIndex = 0;
        Require(host.Simulation.Settings.Habitat == Habitat.Forest && Field("RoachCount").Text == "9", "Forest restoration failed");
        Field("AntCount").Text = "11";
        tabs.SelectedIndex = 1;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = -10000; window.Top = -10000; window.ShowActivated = false; window.ShowInTaskbar = false;
        window.Show();
        foreach (var language in new[] { "zh-CN", "en-US" })
        foreach (var size in new[] { new Size(470, 600), new Size(540, 760) })
        {
            LanguageService.Apply(language); window.Width = size.Width; window.Height = size.Height; window.UpdateLayout();
            var scroll = ((DockPanel)window.Content).Children.OfType<ScrollViewer>().Single();
            var position = tabs.TranslatePoint(new Point(), (UIElement)scroll.Content);
            scroll.ScrollToVerticalOffset(Math.Max(0, position.Y - 10)); window.UpdateLayout();
            foreach (var definition in OceanCatalog.Fish)
            foreach (var suffix in new[] { "Count", "Min", "Max" })
                Require(Field(definition.Kind + suffix).ActualWidth >= 48, "Ocean field clipped");
            SaveWindow(window, Path.Combine(output, $"ocean-{language}-{size.Width}x{size.Height}.png"));
        }
        window.Close();
        var reopened = new SettingsWindow(host, store);
        Require(((TabControl)reopened.FindName("HabitatTabs")).SelectedIndex == 1, "Reopened tab not restored");
        Require(((TextBox)reopened.FindName("RoachCount")).Text == "9" && ((TextBox)reopened.FindName("AntCount")).Text == "11", "Opening ocean erased saved forest counts");
        reopened.Close();

        var blockedPath = Path.GetFullPath(Path.Combine(output, "blocked-population"));
        Directory.CreateDirectory(blockedPath);
        var blockedStore = new SettingsStore(blockedPath);
        blockedStore.SavePreferences(new(StartHotkey: "", PauseHotkey: ""));
        var blockedWindow = new SettingsWindow(host, blockedStore);
        var blockedTabs = (TabControl)blockedWindow.FindName("HabitatTabs");
        blockedTabs.SelectedIndex = 0;
        Require(blockedTabs.SelectedIndex == 1 && host.Simulation.Settings.Habitat == Habitat.Ocean, "Persistence failure changed active scene");
        blockedWindow.Close();

        saverStore.Save(new(Cockroaches: 13));
        var saver = new ConfigurationWindow(saverStore);
        ((TabControl)saver.FindName("HabitatTabs")).SelectedIndex = 1;
        ((TextBox)saver.FindName("BlueTangCount")).Text = "8";
        ((TextBox)saver.FindName("BlueTangMin")).Text = "75";
        ((TextBox)saver.FindName("BlueTangMax")).Text = "150";
        var footer = ((DockPanel)saver.Content).Children.OfType<StackPanel>().Single();
        var save = footer.Children.OfType<Button>().Single();
        saved = File.ReadAllText(saverStore.Path);
        ((TextBox)saver.FindName("LadybugCount")).Text = "101";
        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Require(File.ReadAllText(saverStore.Path) == saved && ((TabControl)saver.FindName("HabitatTabs")).SelectedIndex == 0, "Hidden saver validation did not reveal invalid forest field");
        ((TextBox)saver.FindName("LadybugCount")).Text = "0";
        ((TabControl)saver.FindName("HabitatTabs")).SelectedIndex = 1;
        ((TextBox)saver.FindName("BlueTangCount")).Text = "101";
        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Require(File.ReadAllText(saverStore.Path) == saved, "Invalid saver ocean count saved");
        ((TextBox)saver.FindName("BlueTangCount")).Text = "8";
        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var loaded = saverStore.Load(out var warning);
        Require(warning == null && loaded.Habitat == Habitat.Ocean && loaded.Cockroaches == 13 && loaded.Population.GetOcean(CreatureKind.BlueTang) == new SpeciesPopulation(8, 75, 150), "Saver ocean round-trip failed");
        app.Shutdown();
        var result = "PASS: immediate habitat switching; independent forest/ocean persistence; invalid edit and tab rollback; live language changes; ocean startup retains forest counts; saver ocean save/validation; 470/540px UI screenshots.";
        File.WriteAllText(Path.Combine(output, "ocean-settings-result.txt"), result); Console.WriteLine(result);
    }

    private static void SaveWindow(Window window, string path)
    {
        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }
}
