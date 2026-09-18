using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.ScreenSaver;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Diagnostics;

internal static class SaverSettingsUiProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        var store = new SaverSettingsStore(Path.GetFullPath(Path.Combine(output, "settings.json")));
        File.WriteAllText(store.Path, "{\"Cockroaches\":17}");
        Require(store.Load(out var warning).Cockroaches == 17 && warning == null, "Legacy configuration failed");
        File.WriteAllText(store.Path, "{\"Language\":\"invalid\"}");
        store.Load(out warning); Require(warning != null, "Invalid language accepted");
        store.Save(new(Language: "zh-CN"));
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var sourceImage = Path.GetFullPath(Path.Combine(output, "background-source.png"));
        var pixels = BitmapSource.Create(2, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 20, 40, 180, 255, 160, 80, 20, 255 }, 8);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(pixels));
        using (var stream = File.Create(sourceImage)) png.Save(stream);
        var imported = SaverBackground.Import(sourceImage, store.Path);
        Require(imported != sourceImage && SaverBackground.Load(imported) != null, "Background import failed");
        File.Delete(sourceImage);
        Require(SaverBackground.Load(imported) != null && SaverBackground.Load(sourceImage) == null, "Background copy or missing-file fallback failed");
        var surface = new SaverSurface(new DesktopLife.Creatures.Displays.DisplaySimulation(42), new(0, 0, 300, 150), false, backgroundImage: SaverBackground.Load(imported));
        surface.Measure(new Size(300, 150)); surface.Arrange(new Rect(0, 0, 300, 150));
        var backgroundRender = new RenderTargetBitmap(300, 150, 96, 96, PixelFormats.Pbgra32); backgroundRender.Render(surface);
        var sample = new byte[4]; backgroundRender.CopyPixels(new Int32Rect(10, 75, 1, 1), sample, 4, 0);
        Require(sample[2] > sample[0] && sample[3] == 255, "Background image not rendered behind creatures");
        store.Save(new(Language: "zh-CN", BackgroundImage: imported));
        var window = new ConfigurationWindow(store) { WindowStartupLocation = WindowStartupLocation.Manual, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false };
        window.Show();
        TextBox Field(string name) => (TextBox)window.FindName(name);
        var language = (ComboBox)window.FindName("LanguagePicker");
        var tabs = (TabControl)window.FindName("HabitatTabs");
        var save = (Button)window.FindName("SaveButton");
        Field("CockroachCount").Text = "41"; Field("ClownfishCount").Text = "9";
        foreach (var lang in Enumerable.Range(0, 5))
        foreach (var scene in new[] { 0, 1, 2 })
        foreach (var size in new[] { new Size(620, 800), new Size(470, 600) })
        {
            language.SelectedIndex = lang; tabs.SelectedIndex = scene; window.Width = size.Width; window.Height = size.Height;
            window.UpdateLayout();
            var scroll = ((DockPanel)window.Content).Children.OfType<ScrollViewer>().Single(); scroll.ScrollToTop(); window.UpdateLayout();
            Require(Field("CockroachCount").Text == "41" && Field("ClownfishCount").Text == "9", "Switch lost unsaved input");
            Require(scene == 2 || Field(scene == 0 ? "CockroachCount" : "ClownfishCount").ActualWidth >= 48, "Narrow input clipped");
            var savePosition = save.TranslatePoint(new Point(), window);
            Require(save.ActualHeight >= 36 && savePosition.Y + save.ActualHeight <= window.ActualHeight, "Footer clipped");
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(output, $"saver-{lang}-{scene}-{size.Width}.png")); encoder.Save(stream);
        }
        ((ComboBox)window.FindName("ThemePicker")).SelectedIndex = 1;
        var before = File.ReadAllText(store.Path);
        Field("AntMin").Text = "200";
        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Require(File.ReadAllText(store.Path) == before && tabs.SelectedIndex == 0, "Hidden invalid field was saved or not revealed");
        Field("AntMin").Text = "60";
        Field("CockroachCount").Text = "-1"; save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        language.SelectedIndex = 3;
        var errorText = ((DockPanel)window.Content).Children.OfType<StackPanel>().Single().Children.OfType<TextBlock>().Single().Text;
        Require(errorText.Contains("500") && errorText.Contains("整数"), "Validation error did not switch language with its argument");
        language.SelectedIndex = 4;
        Field("CockroachCount").Text = "41"; tabs.SelectedIndex = 1;
        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var loaded = store.Load(out warning);
        Require(warning == null && loaded.Language == "ko-KR" && loaded.Light && loaded.Cockroaches == 41 && loaded.Population.GetOcean(CreatureKind.Clownfish).Count == 9, "Save round-trip failed");
        Require(loaded.BackgroundImage == imported, "Background path was lost on save");
        var reopened = new ConfigurationWindow(store);
        Require(((ComboBox)reopened.FindName("LanguagePicker")).SelectedIndex == 4, "Language did not persist");
        before = File.ReadAllText(store.Path);
        ((TextBox)reopened.FindName("CockroachCount")).Text = "22"; reopened.Close();
        Require(File.ReadAllText(store.Path) == before, "Closing without save mutated settings");
        var clearWindow = new ConfigurationWindow(store);
        ((Button)clearWindow.FindName("ClearBackgroundButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        ((Button)clearWindow.FindName("SaveButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Require(store.Load(out _).BackgroundImage == null, "Restore solid color failed");
        app.Shutdown();
        Console.WriteLine("PASS: legacy/invalid language, 30 multilingual/resizable UI captures, unsaved edits, hidden validation, language/theme/population persistence, cancel.");
    }
}
