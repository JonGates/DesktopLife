using System.IO;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;

namespace DesktopLife.Diagnostics;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--localization") { LocalizationProbe.Run(args.Length > 1 ? args[1] : "artifacts/localization"); return; }
        if (args.Length > 0 && args[0] == "--rain-performance") { RainPerformanceProbe.Run(); RainPerformanceProbe.Run(true); return; }
        if (args.Length > 0 && args[0] == "--rain") { RainProbe.Run(args.Length > 1 ? args[1] : "artifacts/rain-check"); return; }
        if (args.Length > 0 && args[0] == "--saver-settings-ui") { SaverSettingsUiProbe.Run(args.Length > 1 ? args[1] : "artifacts/saver-ui/check"); return; }
        if (args.Length > 1 && args[0] == "--screensaver-run-check") { ScreenSaverProbe.RunExternalFullScreen(args[1]); return; }
        if (args.Length > 0 && args[0] == "--control-center") { ControlCenterProbe.Run(args.Length > 1 ? args[1] : "artifacts/control-center"); return; }
        if (args.Length > 0 && args[0] == "--ocean-render") { OceanRenderProbe.Run(args.Length > 1 ? args[1] : "artifacts/ocean-render"); return; }
        if (args.Length > 0 && args[0] == "--ocean-settings-probe") { OceanSettingsProbe.Run(args.Length > 1 ? args[1] : "artifacts/ocean-settings"); return; }
        if (args.Length > 0 && args[0] == "--spider-silk") { SpiderSilkProbe.Run(args.Length > 1 ? args[1] : "artifacts/spider-silk"); return; }
        if (args.Length > 0 && args[0] == "--locomotion") { LocomotionProbe.Run(args.Length > 1 ? args[1] : "artifacts/locomotion"); return; }
        if (args.Length > 0 && args[0] == "--insect-catalog") { InsectCatalogProbe.Run(args.Length > 1 ? args[1] : "artifacts/insect-catalog"); return; }
        if (args.Length > 0 && args[0] == "--additional-settings") { AdditionalSettingsProbe.Run(args.Length > 1 ? args[1] : "artifacts/additional-settings"); return; }
        if (args.Length > 0 && args[0] == "--styles") { StyleProbe.Run(args.Length > 1 ? args[1] : "artifacts/style-check"); return; }
        if (args.Length > 1 && args[0] == "--screensaver-external-check") { ScreenSaverProbe.RunExternal(args[1]); return; }
        if (args.Length > 0 && args[0] == "--screensaver-layout-check") { ScreenSaverProbe.RunFullScreen(args.Length > 1 ? args[1] : "artifacts/screensaver-layout-check", synthetic: true); return; }
        if (args.Length > 0 && args[0] == "--screensaver-fullscreen-check") { ScreenSaverProbe.RunFullScreen(args.Length > 1 ? args[1] : "artifacts/screensaver-fullscreen-check"); return; }
        if (args.Length > 0 && args[0] == "--screensaver-check") { ScreenSaverProbe.Run(args.Length > 1 ? args[1] : "artifacts/screensaver-check"); return; }
        if (args.Length > 0 && args[0] == "--recording-backdrop") { RecordingBackdrop.Run(); return; }
        if (args.Length > 0 && args[0] == "--readme-demo") { ReadmeDemo.Run(args.Length > 1 ? args[1] : "artifacts/readme-demo"); return; }
        if (args.Length > 0 && args[0] == "--preferences") { PreferencesProbe.Run(args.Length > 1 ? args[1] : "artifacts/preferences-probe"); return; }
        if (args.Length > 0 && args[0] == "--fly-landing")
        {
            FlyLandingProbe.Run(args.Length > 1 ? args[1] : "artifacts/fly-landing-probe");
            return;
        }
        if (args.Length > 0 && args[0] == "--fly-art")
        {
            FlyRenderProbe.Run(args.Length > 1 ? args[1] : "artifacts/fly-art-probe");
            return;
        }
        if (args.Length > 0 && args[0] == "--seams")
        {
            SeamProbe.Run(args.Length > 1 ? args[1] : "artifacts/seam-probe");
            return;
        }
        if (args.Length > 0 && args[0] == "--controls")
        {
            ControlsProbe.Run(args.Length > 1 ? args[1] : "artifacts/controls-probe");
            return;
        }
        if (args.Length > 0 && args[0] == "--displays")
        {
            DisplayProbe.Run(args.Length > 1 ? args[1] : "artifacts/display-probe");
            return;
        }
        if (args.Length > 0 && args[0] == "--live")
        {
            LiveProbe.Run(args.Length > 1 ? args[1] : "artifacts/live-probe");
            return;
        }
        var output = Path.GetFullPath(args.Length > 0 ? args[0] : "artifacts/render-check");
        Directory.CreateDirectory(output);
        var renderer = new WpfCreatureRenderer();
        var bounds = new WorldBounds(-1920, 0, 1920, 1080);
        ICreature[] creatures = [new RenderCreature(new(-1720, 200), CreatureKind.Fly), new RenderCreature(new(-1620, 200), CreatureKind.Cockroach)];
        foreach (var scale in new[] { 1.0, 1.25, 1.5 })
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen()) renderer.Render(dc, creatures, bounds, 0.1f, scale, scale);
            var bitmap = new RenderTargetBitmap(400, 400, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var pixels = new byte[400 * 400 * 4];
            bitmap.CopyPixels(pixels, 1600, 0);
            if (pixels[(200 * 400 + 200) * 4 + 3] == 0) throw new Exception($"Fly missed expected physical position at {scale}");
            var roachCenter = (200 * 400 + 300) * 4;
            if (pixels[roachCenter + 3] == 0) throw new Exception($"Cockroach missed expected physical position at {scale}");
            if (pixels[roachCenter + 2] <= pixels[roachCenter + 1] * 1.15) throw new Exception("Cockroach must render its brown sprite, not the green fly sprite");
            if (pixels[(100 * 400 + 100) * 4 + 3] != 0) throw new Exception("Background is not transparent");
            var occupied = 0;
            for (var i = 3; i < pixels.Length; i += 4) if (pixels[i] != 0) occupied++;
            if (occupied is < 500 or > 1600) throw new Exception($"Unexpected combined sprite size: {occupied} pixels");
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(output, $"population-{scale * 100:F0}.png"));
            encoder.Save(stream);
            Console.WriteLine($"PASS: {scale * 100:F0}% render, fly (200,200), cockroach (300,200), brown sprite, transparent background, {occupied} painted pixels");
        }
    }

    private sealed class RenderCreature : Creature
    {
        public override CreatureKind Kind { get; }
        public RenderCreature(Vector2 position, CreatureKind kind) { Position = position; Kind = kind; }
        public override void Update(float deltaTime, in CreatureContext context) { }
    }
}
