using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.App;
using DesktopLife.App.Settings;

namespace DesktopLife.Diagnostics;

internal static class ControlCenterProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var store = new SettingsStore(Path.GetFullPath(Path.Combine(output, "settings.json")));
        store.SavePreferences(new(StartHotkey: "", PauseHotkey: ""));
        using var host = new DesktopHost(app.Dispatcher, () => [new("probe", new(0, 0, 1280, 800), true)], false);
        var window = new SettingsWindow(host, store) { Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.Manual };
        window.Show();
        var pages = (TabControl)window.FindName("SettingsPages");
        foreach (var language in new[] { "zh-CN", "en-US" })
        foreach (var size in new[] { (620, 800), (470, 600) })
        for (var page = 0; page < 3; page++)
        {
            ((ComboBox)window.FindName("LanguagePicker")).SelectedIndex = language == "en-US" ? 1 : 0;
            LanguageService.Apply(language); window.Width = size.Item1; window.Height = size.Item2;
            pages.SelectedIndex = page; window.UpdateLayout();
            var scroll = ((DockPanel)window.Content).Children.OfType<ScrollViewer>().Single(); scroll.ScrollToTop(); window.UpdateLayout();
            foreach (var name in new[] { "PauseButton", "ApplyButton" })
            {
                var button = (Button)window.FindName(name);
                if (name == "ApplyButton" && page != 0)
                {
                    if (button.Visibility != Visibility.Collapsed) throw new Exception("Population save shown outside creature settings");
                    continue;
                }
                var end = button.TranslatePoint(new Point(button.ActualWidth, button.ActualHeight), window);
                if (end.Y > window.ActualHeight || end.X > window.ActualWidth || button.ActualHeight < 30) throw new Exception("Footer clipped");
            }
            if (page == 1 && !((Button)window.FindName("SaverWindowsButton")).IsVisible) throw new Exception("Saver controls missing");
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(output, $"page-{page}-{language}-{size.Item1}.png")); encoder.Save(file);
        }
        window.Close(); app.Shutdown();
        var source = Path.GetFullPath(Path.Combine(output, "source.exe")); File.WriteAllBytes(source, [1, 2, 3, 4]);
        var target = ScreenSaverLauncher.PrepareInstall(source, Path.Combine(output, "install path with spaces"));
        if (!File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(target))) throw new Exception("Install copy mismatch");
        if (target != ScreenSaverLauncher.PrepareInstall(source, Path.Combine(output, "install path with spaces"))) throw new Exception("Installation is not stable");
        var command = ScreenSaverLauncher.WindowsSettingsCommand(target);
        if (command.ArgumentList.Count != 2 || command.ArgumentList[1] != target || command.UseShellExecute) throw new Exception("Windows installer arguments unsafe");
        foreach (var mode in new[] { "/s", "/c" })
            if (ScreenSaverLauncher.SelfCommand(mode).ArgumentList.Single() != mode) throw new Exception("Wrong saver command");
        Console.WriteLine("PASS: three settings pages, both languages at 470/620px, visible footer, stable installed copy, safe process arguments; system registration unchanged.");
    }
}
