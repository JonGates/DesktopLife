using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopLife.ScreenSaver;

namespace DesktopLife.Diagnostics;

internal static class ScreenSaverProbe
{
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern nint GetParent(nint hwnd);
    [DllImport("user32.dll")] private static extern bool GetClientRect(nint hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint hwnd, out Rect rect);
    [DllImport("user32.dll")] private static extern bool IsWindow(nint hwnd);
    private delegate bool EnumWindow(nint hwnd, nint data);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(nint parent, EnumWindow callback, nint data);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint hwnd, out uint process);
    private static void Require(bool condition, string text) { if (!condition) throw new Exception(text); }

    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var store = new SaverSettingsStore(Path.GetFullPath(Path.Combine(output, "settings.json")));
        store.Save(new(true, 12, 13, 4));
        Require(store.Load(out _) == new SaverSettings(true, 12, 13, 4), "Settings round trip failed");
        File.WriteAllText(store.Path, "{\"Ants\":-1}");
        Require(store.Load(out var warning) == new SaverSettings() && warning != null, "Invalid settings did not fall back");
        File.WriteAllText(store.Path, "broken");
        Require(store.Load(out warning) == new SaverSettings() && warning != null, "Corrupt settings did not fall back");
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var host = new Window { Width = 500, Height = 320, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, Title = "DesktopLife Preview Probe" };
        SaverSession? session = null;
        Exception? failure = null;
        var phase = 0;
        nint parent = 0, child = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        app.Startup += (_, _) =>
        {
            try
            {
                var config = new ConfigurationWindow(store) { Height = 512, Left = -10000, Top = -10000, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false, ShowInTaskbar = false };
                config.Show(); config.UpdateLayout();
                var root = (DockPanel)config.Content;
                var footer = (StackPanel)root.Children[0];
                var save = (Button)footer.Children[1];
                var location = save.TranslatePoint(new Point(0, save.ActualHeight), root);
                Require(location.Y <= root.ActualHeight && save.ActualHeight == 36, "Save button was clipped on a short display");
                Require(((ScrollViewer)root.Children[1]).ScrollableHeight > 0, "Short configuration cannot scroll");
                config.Close();
                host.Show(); parent = new WindowInteropHelper(host).Handle;
                session = new SaverSession(app, new(false, 12, 13, 4), parent); child = session.PreviewHandle;
                timer.Start();
            }
            catch (Exception e) { failure = e; app.Shutdown(1); }
        };
        timer.Tick += (_, _) =>
        {
            try
            {
                Require(session != null && session.Simulation.World.TotalTime > 0, "Preview animation did not advance");
                Require(GetParent(child) == parent, "Preview HWND was not parented");
                Require(session!.Simulation.World.Manager.Creatures.Count == 30, "Population mismatch");
                Require(GetClientRect(parent, out var client) && GetWindowRect(child, out var bounds) && bounds.Right - bounds.Left == client.Right && bounds.Bottom - bounds.Top == client.Bottom, "Preview did not fit parent client area");
                if (phase == 0)
                {
                    Capture(child, Path.Combine(output, "preview-dark.png"));
                    session.Dispose(); Require(!IsWindow(child), "Disposal left a child window");
                    session = new SaverSession(app, new(true, 12, 13, 4), parent); child = session.PreviewHandle;
                    phase++;
                }
                else if (phase == 1)
                {
                    Capture(child, Path.Combine(output, "preview-light.png"));
                    host.Width = 700; host.Height = 420; phase++;
                }
                else
                {
                    Capture(child, Path.Combine(output, "preview-resized.png"));
                    phase++; timer.Stop(); host.Close(); // Session must detect a destroyed parent and shut down.
                }
            }
            catch (Exception e) { failure = e; timer.Stop(); app.Shutdown(1); }
        };
        var watchdog = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        watchdog.Tick += (_, _) => { failure = new Exception("Preview failed to exit after parent destruction"); app.Shutdown(1); };
        watchdog.Start(); app.Run(); watchdog.Stop(); timer.Stop(); session?.Dispose();
        if (failure != null) throw failure;
        Require(phase == 3 && !IsWindow(child), "Preview cleanup did not finish");
        File.WriteAllText(Path.Combine(output, "result.txt"), "PASS: isolated settings; invalid settings fallback; animated dark/light preview; child HWND; resizing; parent destruction; disposal.");
        Console.WriteLine(File.ReadAllText(Path.Combine(output, "result.txt")));
    }

    private static void Capture(nint child, string path)
    {
        var surface = (FrameworkElement)HwndSource.FromHwnd(child)!.RootVisual;
        surface.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)surface.ActualWidth, (int)surface.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(surface);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }

    public static void RunFullScreen(string output, bool synthetic = false)
    {
        Directory.CreateDirectory(output);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        SaverSession? session = null;
        Exception? failure = null;
        var verified = false;
        IReadOnlyList<DesktopLife.Engine.World.DisplayArea> displays = synthetic
            ? [new("left", new(-1280, 120, 1280, 720)), new("primary", new(0, 0, 1920, 1080), true)]
            : DesktopLife.Windows.MonitorService.GetDisplays();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        app.Startup += (_, _) =>
        {
            try { session = new SaverSession(app, new(false, 20, 20, 3), getDisplays: () => displays); timer.Start(); }
            catch (Exception e) { failure = e; app.Shutdown(1); }
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                var windows = app.Windows.Cast<Window>().ToArray();
                Require(windows.Length == displays.Count, "Not every monitor has a screen saver window");
                foreach (var display in displays)
                {
                    Require(windows.Any(w => GetWindowRect(new WindowInteropHelper(w).Handle, out var r) && r.Left == display.Bounds.Left && r.Top == display.Bounds.Top && r.Right - r.Left == display.Bounds.Width && r.Bottom - r.Top == display.Bounds.Height), "Physical monitor bounds mismatch");
                }
                Require(session!.Simulation.TotalCockroachCount == 20 && session.Simulation.World.Manager.Creatures.Count == 44, "Population multiplied across screens");
                verified = true;
                File.WriteAllText(Path.Combine(output, "result.txt"), $"PASS: {windows.Length} full-screen windows match physical monitor bounds; one shared population; animated world; cleanup.");
            }
            catch (Exception e) { failure = e; }
            finally { app.Shutdown(); }
        };
        app.Exit += (_, _) => session?.Dispose();
        app.Run(); session?.Dispose();
        if (failure != null) throw failure;
        Require(verified, "Full-screen check exited prematurely");
        Console.WriteLine(File.ReadAllText(Path.Combine(output, "result.txt")));
    }

    public static void RunExternal(string executable)
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var host = new Window { Width = 500, Height = 320, Left = -10000, Top = -10000, ShowActivated = false, ShowInTaskbar = false, Title = "DesktopLife External Preview Probe" };
        Process? process = null;
        Exception? failure = null;
        nint parent = 0;
        var phase = 0;
        var ticks = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        app.Startup += (_, _) =>
        {
            try
            {
                host.Show(); parent = new WindowInteropHelper(host).Handle;
                var info = new ProcessStartInfo(Path.GetFullPath(executable)) { UseShellExecute = false };
                info.ArgumentList.Add("/p"); info.ArgumentList.Add(parent.ToInt64().ToString(System.Globalization.CultureInfo.InvariantCulture));
                process = Process.Start(info) ?? throw new Exception("Could not start published screen saver");
                timer.Start();
            }
            catch (Exception e) { failure = e; app.Shutdown(1); }
        };
        timer.Tick += (_, _) =>
        {
            try
            {
                if (++ticks > 40) throw new Exception("External screen saver preview timed out");
                if (phase == 0)
                {
                    Require(!process!.HasExited, "External preview exited early");
                    var children = new List<nint>();
                    EnumChildWindows(parent, (hwnd, _) => { GetWindowThreadProcessId(hwnd, out var pid); if (pid == process.Id) children.Add(hwnd); return true; }, 0);
                    if (children.Count == 0) return;
                    Require(children.Count == 1 && GetParent(children[0]) == parent, "External preview parent mismatch");
                    Require(GetClientRect(parent, out var client) && GetWindowRect(children[0], out _), "External preview rectangles unavailable");
                    GetWindowRect(children[0], out var bounds);
                    if (bounds.Right - bounds.Left <= 1 || bounds.Bottom - bounds.Top <= 1) return; // HwndSource starts at 1×1 while sprite resources load.
                    Require(bounds.Right - bounds.Left == client.Right && bounds.Bottom - bounds.Top == client.Bottom, $"External preview size mismatch: child {bounds.Right - bounds.Left}×{bounds.Bottom - bounds.Top}; parent {client.Right}×{client.Bottom}");
                    host.Close(); phase = 1;
                }
                else if (process!.HasExited)
                {
                    Require(process.ExitCode == 0, "Published preview failed during parent cleanup"); phase = 2; app.Shutdown();
                }
            }
            catch (Exception e) { failure = e; app.Shutdown(1); }
        };
        app.Run(); timer.Stop();
        if (process != null) { if (!process.HasExited) process.Kill(); process.Dispose(); }
        if (failure != null) throw failure;
        Require(phase == 2, "External preview did not complete");
        Console.WriteLine("PASS: published executable starts with /p; cross-process preview parenting and size; parent destruction exits cleanly.");
    }
}
