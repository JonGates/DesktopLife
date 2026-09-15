using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using DesktopLife.App;
using DesktopLife.App.Overlay;
using DesktopLife.Engine.World;
using DesktopLife.Windows;

namespace DesktopLife.Diagnostics;

/// <summary>Real HWNDs and production host with an injected monitor topology; no OS display settings are changed.</summary>
internal static class DisplayProbe
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint window, out Rect rect);
    [DllImport("user32.dll")] private static extern bool IsWindow(nint window);

    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var physical = MonitorService.GetDisplays();
        var b = physical[0].Bounds;
        var a = new DisplayArea("synthetic-a", new(b.Left, b.Top, b.Width / 2, b.Height), true);
        var second = new DisplayArea("synthetic-b", new(b.Left + b.Width / 2, b.Top, b.Width / 2, b.Height));
        var c = new DisplayArea("synthetic-negative", new(b.Left - b.Width / 2, b.Top - 100, b.Width / 2, b.Height));
        IReadOnlyList<DisplayArea> layout = [a, second];
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        using var host = new DesktopHost(app.Dispatcher, () => layout);
        var unexpectedExit = false;
        host.ExitRequested += () => unexpectedExit = true;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        var phase = 0;
        string? failure = null;
        OverlayWindow? retained = null;
        nint removedHandle = 0;
        float[] pausedTimes = [];
        var rows = new List<object>();
        void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        void CheckWindows(int count, bool visible)
        {
            Require(host.Overlays.Count == count && host.Simulation.TotalFlyCount == count &&
                host.Simulation.TotalCockroachCount == count * 20, "Incorrect per-screen population or window count");
            Require(!unexpectedExit, "Topology update requested application exit");
            foreach (var window in host.Overlays)
            {
                Require(window.IsVisible == visible, "Incorrect global pause visibility");
                if (!visible) continue;
                var hwnd = new WindowInteropHelper(window).Handle;
                var bounds = window.Session.Display.Bounds;
                Require(GetWindowRect(hwnd, out var rect) && rect.Left == bounds.Left && rect.Top == bounds.Top &&
                    rect.Right == bounds.Right && rect.Bottom == bounds.Bottom, "Overlay does not match physical display bounds");
            }
        }
        timer.Tick += (_, _) =>
        {
            try
            {
                phase++;
                switch (phase)
                {
                    case 1:
                        CheckWindows(2, true);
                        Require(host.Simulation.Worlds.All(w => w.World.TotalTime > 0), "Not all worlds ticked");
                        Require(Math.Abs(host.Simulation.Worlds[0].World.TotalTime - host.Simulation.Worlds[1].World.TotalTime) < 0.001, "Worlds did not advance together");
                        retained = host.Overlays[0];
                        removedHandle = new WindowInteropHelper(host.Overlays[1]).Handle;
                        break;
                    case 2:
                        layout = [a with { IsPrimary = false }, second with { IsPrimary = true }];
                        host.SynchronizeDisplays();
                        host.SynchronizeDisplays();
                        Require(ReferenceEquals(retained, host.Overlays[0]) && !host.Overlays[0].Session.Display.IsPrimary, "Repeated events reset a window or left stale metadata");
                        host.TogglePause();
                        pausedTimes = host.Simulation.Worlds.Select(w => w.World.TotalTime).ToArray();
                        CheckWindows(2, false);
                        break;
                    case 3:
                        Require(host.Simulation.Worlds.Select(w => w.World.TotalTime).SequenceEqual(pausedTimes), "Paused worlds advanced");
                        layout = [a, second with { Bounds = second.Bounds with { Height = second.Bounds.Height - 80 } }, c];
                        host.SynchronizeDisplays();
                        CheckWindows(3, false);
                        Require(ReferenceEquals(retained, host.Overlays[0]), "Unaffected window recreated");
                        Require(!IsWindow(removedHandle), "Resized screen leaked its previous HWND");
                        pausedTimes = host.Simulation.Worlds.Select(w => w.World.TotalTime).ToArray();
                        break;
                    case 4:
                        Require(host.Simulation.Worlds.Select(w => w.World.TotalTime).SequenceEqual(pausedTimes), "Paused hotplug started updates");
                        host.TogglePause();
                        CheckWindows(3, true);
                        break;
                    case 5:
                        CheckWindows(3, true);
                        Require(host.Simulation.Worlds.Where((w, i) => w.World.TotalTime <= pausedTimes[i]).Count() == 0, "Resume did not restart every world");
                        Require(host.Overlays.All(w => ((RenderSurface)w.Content).Seconds > 0), "A screen did not receive rendered frames");
                        layout = [second, c];
                        host.SynchronizeDisplays();
                        CheckWindows(2, true);
                        break;
                    case 6:
                        layout = [];
                        host.SynchronizeDisplays();
                        CheckWindows(0, true);
                        break;
                    case 7:
                        layout = [a];
                        host.SynchronizeDisplays();
                        CheckWindows(1, true);
                        break;
                    case 8:
                        Require(host.Simulation.Worlds[0].World.TotalTime > 0, "Reconnected display never updated");
                        host.Dispose();
                        Require(host.Overlays.Count == 0, "Dispose retained windows");
                        timer.Stop();
                        app.Shutdown();
                        break;
                }
                rows.Add(new { Phase = phase, Displays = host.Overlays.Count, host.IsPaused, host.Simulation.TotalFlyCount, host.Simulation.TotalCockroachCount });
            }
            catch (Exception error)
            {
                failure = error.ToString();
                timer.Stop();
                app.Shutdown(1);
            }
        };
        app.Startup += (_, _) => { host.Start(); timer.Start(); };
        var result = app.Run();
        timer.Stop();
        File.WriteAllText(Path.Combine(output, "topology.json"), JsonSerializer.Serialize(new { PhysicalDisplays = physical, SyntheticLifecycle = rows, Failure = failure }, new JsonSerializerOptions { WriteIndented = true }));
        if (result != 0 || phase != 8 || failure != null) throw new Exception(failure ?? "Probe ended early");
        Console.WriteLine($"PASS: {physical.Count} physical monitor(s) enumerated; synthetic 2/3/0/1-screen HWND lifecycle, negative coordinates, resize, primary change, repeated events, pause/hotplug/resume, shared loop and disposal.");
    }
}
