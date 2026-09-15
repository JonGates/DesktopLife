using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopLife.App.Overlay;

namespace DesktopLife.Diagnostics;

/// <summary>Runs the real App lifecycle and verifies pause/resume without injecting desktop input.</summary>
internal static class LiveProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var app = new App.App();
        app.InitializeComponent();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        var clock = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
        var rows = new List<object>();
        double[] pauseTimes = [];
        var phase = 0;
        string? failure = null;
        timer.Tick += (_, _) =>
        {
            try
            {
                if (app.Desktop is not { } desktop || desktop.Overlays.Count == 0)
                    throw new InvalidOperationException("Production overlay was not created (another instance may be running)");
                var overlays = desktop.Overlays;
                var surfaces = overlays.Select(w => (RenderSurface)w.Content).ToArray();
                var surface = surfaces[0];
                process.Refresh();
                rows.Add(new { Seconds = clock.Elapsed.TotalSeconds, Phase = phase, Screens = overlays.Count, Flies = desktop.Simulation.TotalFlyCount, Cockroaches = desktop.Simulation.TotalCockroachCount, PerScreen = surfaces.Select(s => new { s.Fps, s.UpdateMs, s.VisibleCount }).ToArray(),
                    CpuSeconds = process.TotalProcessorTime.TotalSeconds, PrivateMB = process.PrivateMemorySize64 / 1048576.0,
                    Gen0 = GC.CollectionCount(0), Gen1 = GC.CollectionCount(1), Gen2 = GC.CollectionCount(2) });
                if (phase == 0 && clock.Elapsed.TotalSeconds >= 8)
                {
                    if (surfaces.Any(s => s.VisibleCount == 0 || s.Fps < 10)) throw new Exception("No visible creatures or update loop is below 10 Hz");
                    var dpi = VisualTreeHelper.GetDpi(surface);
                    var bitmap = new RenderTargetBitmap((int)Math.Ceiling(surface.ActualWidth * dpi.DpiScaleX), (int)Math.Ceiling(surface.ActualHeight * dpi.DpiScaleY), 96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);
                    bitmap.Render(surface);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using (var file = File.Create(Path.Combine(output, "live-overlay.png"))) encoder.Save(file);
                    pauseTimes = surfaces.Select(s => s.Seconds).ToArray();
                    desktop.TogglePause();
                    phase = 1;
                }
                else if (phase == 1 && clock.Elapsed.TotalSeconds >= 13)
                {
                    if (overlays.Any(w => w.IsVisible) || surfaces.Where((s, i) => s.Seconds != pauseTimes[i]).Any()) throw new Exception("Pause did not hide the overlay and stop simulation ticks");
                    desktop.TogglePause();
                    phase = 2;
                }
                else if (phase == 2 && clock.Elapsed.TotalSeconds >= 20)
                {
                    if (overlays.Any(w => !w.IsVisible) || surfaces.Where((s, i) => s.Seconds <= pauseTimes[i] + 5 || s.VisibleCount == 0).Any()) throw new Exception("Resume did not restart visible simulation");
                    phase = 3;
                    timer.Stop();
                    app.Shutdown();
                }
            }
            catch (Exception error)
            {
                failure = error.ToString();
                timer.Stop();
                app.Shutdown(1);
            }
        };
        timer.Start();
        var exitCode = app.Run();
        timer.Stop();
        File.WriteAllText(Path.Combine(output, "samples.json"), JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
        if (failure is not null || exitCode != 0 || phase != 3) throw new Exception(failure ?? "Live probe exited before all phases completed");
        Console.WriteLine("PASS: real app runs with visible creatures; pause hides overlay and stops ticks; resume restarts updates; app exits cleanly.");
    }
}
