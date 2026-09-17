using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;

namespace DesktopLife.Diagnostics;
internal static class RainPerformanceProbe
{
    public static void Run()
    {
        var layout = new DesktopLayout([new("left", new(0, 0, 1920, 1080), true), new("right", new(1920, 0, 1920, 1080), false)]);
        var rain = new RainGlass { Enabled = true };
        rain.SetLevel(5);
        for (var i = 0; i < 300; i++) rain.Update(1f / 30, layout, new(-999, -999), null);
        Console.WriteLine($"Prepared {rain.Drops.Count} drops, {rain.Trails.Count} trails");
        var target = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
        var samples = new List<double>(); var update = new List<double>(); long bytes = 0;
        for (var i = 0; i < 9; i++)
        {
            var before = GC.GetAllocatedBytesForCurrentThread(); var watch = Stopwatch.StartNew();
            rain.Update(1f / 30, layout, new(-999, -999), null); var ms = watch.Elapsed.TotalMilliseconds;
            var visual = new DrawingVisual(); using (var dc = visual.RenderOpen()) RainGlassRenderer.Render(dc, rain, layout.Displays[0].Bounds, 1, 1);
            target.Clear(); target.Render(visual);
            if (i >= 1) { samples.Add(watch.Elapsed.TotalMilliseconds); update.Add(ms); bytes += GC.GetAllocatedBytesForCurrentThread() - before; }
        }
        samples.Sort();
        Console.WriteLine($"Rain benchmark: drops={rain.Drops.Count}, trails={rain.Trails.Count}; 1080p software raster median={samples[4]:F2}ms, max={samples[^1]:F2}ms, simulation mean={update.Average():F2}ms, allocations/frame={bytes / 8 / 1024}KB. Includes only one of two viewports, not live desktop FPS.");
    }
}
