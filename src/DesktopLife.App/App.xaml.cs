using System.Windows;
using DesktopLife.App.Overlay;
using DesktopLife.App.Tray;
using DesktopLife.App.Settings;

namespace DesktopLife.App;

public partial class App : Application
{
    private TrayService? _tray;
    private Mutex? _instance;
    private bool _ownsInstance;
    public DesktopHost? Desktop { get; private set; }
    private readonly SettingsStore _settingsStore = new();
    private SettingsWindow? _settingsWindow;
    private string? _settingsWarning;
    private EventWaitHandle? _showSettingsSignal;
    private RegisteredWaitHandle? _showSettingsRegistration;

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
        _showSettingsSignal = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\DesktopLife.ShowSettings");
        _instance = new Mutex(true, @"Local\DesktopLife.FlyPrototype", out _ownsInstance);
        if (!_ownsInstance)
        {
            if (e.Args.Contains("--settings")) _showSettingsSignal.Set();
            Shutdown();
            return;
        }
        _showSettingsRegistration = ThreadPool.RegisterWaitForSingleObject(_showSettingsSignal, (_, _) =>
        {
            if (!Dispatcher.HasShutdownStarted) Dispatcher.BeginInvoke(new Action(ShowSettings));
        }, null, Timeout.Infinite, executeOnlyOnce: false);
        Desktop = new DesktopHost(Dispatcher);
        _tray = new TrayService(ShowSettings, Desktop.TogglePause, () => Shutdown());
        Desktop.ExitRequested += () => Shutdown();
        void RefreshTray()
        {
            _tray.SetPopulationSummary(Desktop.Simulation.Worlds.Count,
                Desktop.Simulation.TotalFlyCount, Desktop.Simulation.TotalCockroachCount, Desktop.Simulation.TotalAntCount, Desktop.Simulation.TotalCaterpillarCount);
            _tray.SetPaused(Desktop.IsPaused);
        }
        Desktop.LayoutChanged += RefreshTray;
        Desktop.StateChanged += RefreshTray;
        Desktop.Start();
        var settings = _settingsStore.Load(out _settingsWarning);
        Desktop.SetPopulation(settings);
        if (e.Args.Contains("--settings")) ShowSettings();
    }

    public void ShowSettings()
    {
        if (Desktop == null) return;
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(Desktop, _settingsStore, _settingsWarning);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }
        _settingsWindow.Show();
        if (_settingsWindow.WindowState == WindowState.Minimized) _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _showSettingsRegistration?.Unregister(null);
        _showSettingsSignal?.Dispose();
        Desktop?.Dispose();
        _tray?.Dispose();
        if (_ownsInstance) _instance?.ReleaseMutex();
        _instance?.Dispose();
        base.OnExit(e);
    }
}
