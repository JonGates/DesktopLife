using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Threading;
using DesktopLife.App;
using DesktopLife.Creatures.Fly;
using DesktopLife.Engine.Input;
using DesktopLife.Windows;
namespace DesktopLife.Diagnostics;

/// <summary>Real production frame loop, deterministic click mailbox input; no desktop input injection.</summary>
internal static class FlyLandingProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        // Register and release the real read-only native hook, without capturing the user's input during the test.
        var input = new MouseClickBuffer { Enabled = false };
        using (var hook = new GlobalMouseClickSource(input))
        {
            if (!hook.IsInstalled) throw new Exception("Mouse hook was not installed");
            hook.Dispose();
            if (hook.IsInstalled) throw new Exception("Mouse hook was not released");
        }
        using var host = new DesktopHost(app.Dispatcher, observeMouseClicks: false);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        var clock = Stopwatch.StartNew();
        var phaseClock = new Stopwatch();
        var phase = 0;
        string? failure = null;
        FlyCreature? fly = null;
        Vector2 first = default, second = default, previous = default;
        void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        timer.Tick += (_, _) =>
        {
            try
            {
                Require(clock.Elapsed.TotalSeconds < 30, "Landing lifecycle timed out");
                switch (phase)
                {
                    case 0:
                        fly = (FlyCreature)host.Simulation.World.Manager.Creatures[0];
                        first = host.Simulation.Worlds[0].Display.Bounds.Center;
                        second = host.Simulation.Worlds[^1].Display.Bounds.Center + new Vector2(100, 0);
                        fly.Relocate(first - new Vector2(100, 0));
                        host.Clicks.Record(first);
                        phase = 1;
                        break;
                    case 1:
                        if (fly!.State != FlyState.Landed) break;
                        Require(fly.Position == first && fly.IsResting, "Initial landing anchor or pose incorrect");
                        host.TogglePause();
                        host.Clicks.Record(second);
                        phaseClock.Restart();
                        phase = 2;
                        break;
                    case 2:
                        Require(fly!.State == FlyState.Landed && fly.Position == first, "Pause advanced landed fly");
                        if (phaseClock.Elapsed.TotalSeconds < 1) break;
                        host.TogglePause();
                        phaseClock.Restart();
                        phase = 3;
                        break;
                    case 3:
                        if (phaseClock.Elapsed.TotalSeconds < 2.8)
                            Require(fly!.State == FlyState.Landed && fly.Position == first, "Stay ended early or paused click was replayed");
                        if (phaseClock.Elapsed.TotalSeconds < 3.4) break;
                        Require(fly!.State is FlyState.Approach or FlyState.Orbit or FlyState.Panic, "Fly did not resume following after three seconds");
                        Require(!fly.IsResting, "Flying pose did not resume");
                        previous = fly.Position;
                        host.Clicks.Record(second);
                        phase = 4;
                        break;
                    case 4:
                        // Timer samples can span more than one frame; state-machine unit tests bound each frame.
                        Require(Vector2.Distance(previous, fly!.Position) < 100, "Cross-screen landing teleported");
                        previous = fly.Position;
                        if (fly.State != FlyState.Landed) break;
                        Require(fly.Position == second, "Cross-screen click landed at the current cursor instead of recorded point");
                        host.Dispose();
                        timer.Stop();
                        phase = 5;
                        app.Shutdown();
                        break;
                }
            }
            catch (Exception error) { failure = error.ToString(); timer.Stop(); app.Shutdown(1); }
        };
        app.Startup += (_, _) => { host.Start(); host.SetPopulation(new(0, 0, 0)); timer.Start(); };
        var result = app.Run();
        timer.Stop();
        File.WriteAllText(Path.Combine(output, "result.txt"), failure ?? "PASS: native hook install/dispose; real shared frame loop lands at original click, rests through pause, drops paused click, resumes after 3s, and lands on another display.");
        if (result != 0 || failure != null || phase != 5) throw new Exception(failure ?? "Incomplete landing probe");
        Console.WriteLine(File.ReadAllText(Path.Combine(output, "result.txt")));
    }
}
