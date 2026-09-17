using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.App;
using DesktopLife.App.Settings;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.Input;
using DesktopLife.Rendering;
using DesktopLife.ScreenSaver;

namespace DesktopLife.Diagnostics;
internal static class RainProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var simulation = new DisplaySimulation(43);
        simulation.Synchronize([new("rain", new(0, 0, 960, 600), true)]);
        simulation.SetPopulation(new(Habitat: Habitat.Rain));
        if (simulation.World.Manager.Creatures.Count != 0 || !simulation.World.Rain.Enabled) throw new Exception("Rain contains creatures");
        for (var i = 0; i < 650; i++) simulation.Update(.05f, new(-500, -500));
        for (var frame = 0; frame < 3; frame++)
        {
            simulation.Update(.05f, new(480, 260), new MouseClick(frame + 1, new(480, 260)));
            for (var i = 0; i < 8; i++) simulation.Update(.05f, new(-500, -500));
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new LinearGradientBrush(Color.FromRgb(24, 43, 59), Color.FromRgb(75, 113, 119), 60), null, new Rect(0, 0, 960, 600));
                RainGlassRenderer.Render(dc, simulation.World.Rain, simulation.Layout.Bounds, 1, 1);
            }
            Save(visual, 960, 600, Path.Combine(output, $"rain-{frame}.png"));
        }
        simulation.SetPopulation(new(Habitat: Habitat.Forest));
        if (simulation.World.Rain.Enabled || simulation.World.Rain.Drops.Count != 0) throw new Exception("Rain state survived switching away");
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        // Exercise the production window's redraw decision, not only the renderer.
        var live = new DisplaySimulation(74);
        live.Synchronize([new("offscreen", new(-10000, -10000, 960, 600), true)]);
        live.SetPopulation(new(Habitat: Habitat.Rain));
        var overlay = new DesktopLife.App.Overlay.OverlayWindow(live.Worlds[0]);
        overlay.Show(); overlay.UpdateLayout();
        var surface = (FrameworkElement)overlay.Content;
        live.World.Rain.AddDrop(new(-9950, -9950), 8);
        overlay.PresentFrame(new(.016f, .016f, 1), 0);
        if (surface.IsArrangeValid) throw new Exception("Rain-only frame did not invalidate the production overlay");
        surface.UpdateLayout();
        if (VisualTreeHelper.GetDrawing(surface)?.Bounds.IsEmpty != false) throw new Exception("Rain overlay drawing is empty");
        overlay.Close();
        var store = new SettingsStore(Path.GetFullPath(Path.Combine(output, "desktop.json")));
        store.SavePreferences(new(StartHotkey: "", PauseHotkey: ""));
        using var host = new DesktopHost(app.Dispatcher, () => [new("probe", new(0, 0, 960, 600), true)], false);
        var window = new SettingsWindow(host, store) { Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.Manual };
        window.Show(); ((TabControl)window.FindName("HabitatTabs")).SelectedIndex = 2; window.UpdateLayout();
        if (store.Load(out _).Habitat != Habitat.Rain || !host.Simulation.World.Rain.Enabled) throw new Exception("Desktop rain tab did not persist/apply");
        Save(window, (int)window.ActualWidth, (int)window.ActualHeight, Path.Combine(output, "rain-settings.png")); window.Close();
        var saverStore = new SaverSettingsStore(Path.GetFullPath(Path.Combine(output, "saver.json")));
        var saver = new ConfigurationWindow(saverStore);
        ((TabControl)saver.FindName("HabitatTabs")).SelectedIndex = 2;
        ((Button)saver.FindName("SaveButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (saverStore.Load(out _).Habitat != Habitat.Rain) throw new Exception("Screen saver rain selection did not persist");
        app.Shutdown();
        Console.WriteLine("PASS: rain render, creature isolation, scene cleanup, desktop switching/persistence, screen saver third scene.");
    }
    private static void Save(Visual visual, int width, int height, string path)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(visual);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var file = File.Create(path); encoder.Save(file);
    }
}
