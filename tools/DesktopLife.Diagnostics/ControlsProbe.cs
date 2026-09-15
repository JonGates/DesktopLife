using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopLife.App;
using DesktopLife.App.Overlay;
using DesktopLife.App.Settings;
using DesktopLife.Creatures.Displays;
namespace DesktopLife.Diagnostics;

internal static class ControlsProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var store = new SettingsStore(Path.Combine(Path.GetFullPath(output), "settings.json"));
        store.Save(new(1, 20));
        if (store.Load(out _) != new PopulationSettings(1, 20)) throw new Exception("Settings round-trip failed");
        File.WriteAllText(store.FilePath, "{bad-json");
        if (store.Load(out var warning) != new PopulationSettings() || warning == null) throw new Exception("Corrupt settings fallback failed");
        store.Save(new(1, 20));
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        using var host = new DesktopHost(app.Dispatcher);
        var window = new SettingsWindow(host, store);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        var phase = 0;
        string? failure = null;
        Guid[] original = [];
        void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        void Apply(string flies, string roaches)
        {
            ((TextBox)window.FindName("FlyCount")).Text = flies;
            ((TextBox)window.FindName("RoachCount")).Text = roaches;
            ((Button)window.FindName("ApplyButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        timer.Tick += (_, _) =>
        {
            try
            {
                phase++;
                switch (phase)
                {
                    case 1:
                        original = host.Simulation.World.Manager.Creatures.Select(c => c.Id).ToArray();
                        Apply("2", "37");
                        Require(host.Simulation.TotalFlyCount == 2 && host.Simulation.TotalCockroachCount == 37, "Apply did not change global counts");
                        Require(store.Load(out _) == new PopulationSettings(2, 37), "Apply did not persist counts");
                        Require(original.All(id => host.Simulation.World.Manager.Creatures.Any(c => c.Id == id)), "Increasing counts replaced existing creatures");
                        break;
                    case 2:
                        foreach (var invalid in new[] { "1.5", "-1", "501", "abc", "" })
                        {
                            Apply("2", invalid);
                            Require(host.Simulation.TotalCockroachCount == 37 && store.Load(out _) == new PopulationSettings(2, 37), "Invalid input changed settings");
                        }
                        Apply("0", "0");
                        Require(host.Simulation.World.Manager.Creatures.Count == 0, "Zero population failed");
                        break;
                    case 3:
                        Require(host.Overlays.All(w => ((RenderSurface)w.Content).VisibleCount == 0), "Removed creatures left visible counts");
                        ((Button)window.FindName("PauseButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Apply("3", "12");
                        Require(host.IsPaused && host.Overlays.All(w => !w.IsVisible), "Applying while paused resumed overlays");
                        var blocked = Path.Combine(Path.GetFullPath(output), "blocked-parent");
                        File.WriteAllText(blocked, "This is a file, not a directory.");
                        var badWindow = new SettingsWindow(host, new SettingsStore(Path.Combine(blocked, "settings.json")));
                        ((TextBox)badWindow.FindName("FlyCount")).Text = "4";
                        ((Button)badWindow.FindName("ApplyButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Require(host.Simulation.TotalFlyCount == 3 && ((TextBlock)badWindow.FindName("Status")).Text.Contains("保存失败"), "Save failure changed live population or gave no error");
                        badWindow.Close();
                        break;
                    case 4:
                        ((Button)window.FindName("PauseButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Require(!host.IsPaused && host.Overlays.All(w => w.IsVisible), "Control window resume failed");
                        window.UpdateLayout();
                        var dpi = VisualTreeHelper.GetDpi(window);
                        var bitmap = new RenderTargetBitmap((int)(window.ActualWidth * dpi.DpiScaleX), (int)(window.ActualHeight * dpi.DpiScaleY),
                            96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);
                        bitmap.Render(window);
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using (var stream = File.Create(Path.Combine(output, "controls.png"))) encoder.Save(stream);
                        window.Close();
                        break;
                    case 5:
                        Require(host.Overlays.Count > 0 && host.Simulation.World.TotalTime > 0, "Closing settings stopped the desktop");
                        var reopened = new SettingsWindow(host, store);
                        Require(((TextBox)reopened.FindName("FlyCount")).Text == "3" && ((TextBox)reopened.FindName("RoachCount")).Text == "12", "Reopening lost configured values");
                        reopened.Close();
                        host.Dispose();
                        timer.Stop();
                        app.Shutdown();
                        break;
                }
            }
            catch (Exception error) { failure = error.ToString(); timer.Stop(); app.Shutdown(1); }
        };
        app.Startup += (_, _) => { host.Start(); window.Show(); timer.Start(); };
        var result = app.Run();
        timer.Stop();
        File.WriteAllText(Path.Combine(output, "result.txt"), failure ?? "PASS: actual controls apply, validate, persist, pause/resume, support zero, handle failed writes, close/reopen, and preserve IDs.");
        if (result != 0 || failure != null || phase != 5) throw new Exception(failure ?? "Controls probe ended early");
        Console.WriteLine(File.ReadAllText(Path.Combine(output, "result.txt")));
    }
}
