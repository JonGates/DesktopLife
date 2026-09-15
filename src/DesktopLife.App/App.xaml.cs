using System.Windows;
using DesktopLife.App.Overlay;
using DesktopLife.App.Tray;

namespace DesktopLife.App;

public partial class App : Application
{
    private TrayService? _tray;
    private Mutex? _instance;
    private bool _ownsInstance;
    public DesktopHost? Desktop { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            CrashLog.Write(args.Exception);
            args.Handled = true;
            Shutdown(1);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) => CrashLog.Write(args.ExceptionObject);
        _instance = new Mutex(true, @"Local\DesktopLife.FlyPrototype", out _ownsInstance);
        if (!_ownsInstance) { Shutdown(); return; }
        Desktop = new DesktopHost(Dispatcher);
        _tray = new TrayService(Desktop.TogglePause, () => Shutdown());
        Desktop.ExitRequested += () => Shutdown();
        Desktop.LayoutChanged += () =>
        {
            MainWindow = Desktop.Overlays.FirstOrDefault();
            _tray.SetPopulationSummary(Desktop.Simulation.Worlds.Count,
                Desktop.Simulation.TotalFlyCount, Desktop.Simulation.TotalCockroachCount);
        };
        Desktop.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Desktop?.Dispose();
        _tray?.Dispose();
        if (_ownsInstance) _instance?.ReleaseMutex();
        _instance?.Dispose();
        base.OnExit(e);
    }
}
