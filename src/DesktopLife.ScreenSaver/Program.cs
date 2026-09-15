using System.IO;
using System.Windows;
using System.Windows.Interop;
using DesktopLife.Engine.ScreenSaving;
namespace DesktopLife.ScreenSaver;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var options = ScreenSaverArguments.Parse(args);
        if (options.Mode == ScreenSaverMode.Invalid) return 1;
        var created = false;
        using var instance = options.Mode == ScreenSaverMode.Run ? new Mutex(true, @"Local\DesktopLife.ScreenSaver.FullScreen", out created) : null;
        if (instance != null && !created) return 0;
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        SaverSession? session = null;
        void Log(Exception exception)
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopLife");
                Directory.CreateDirectory(folder); File.WriteAllText(Path.Combine(folder, "screensaver-error.log"), exception.ToString());
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }
        app.DispatcherUnhandledException += (_, e) => { Log(e.Exception); e.Handled = true; app.Shutdown(1); };
        app.Startup += (_, _) =>
        {
            try
            {
                var store = new SaverSettingsStore();
                if (options.Mode == ScreenSaverMode.Configure)
                {
                    var window = new ConfigurationWindow(store);
                    if (options.Parent != 0 && SaverNative.IsWindow((nint)options.Parent)) new WindowInteropHelper(window).Owner = (nint)options.Parent;
                    window.Closed += (_, _) => app.Shutdown(); window.Show();
                }
                else session = new SaverSession(app, store.Load(out _), options.Mode == ScreenSaverMode.Preview ? (nint)options.Parent : 0);
            }
            catch (Exception e) { Log(e); app.Shutdown(1); }
        };
        app.Exit += (_, _) => session?.Dispose();
        try { return app.Run(); }
        finally { session?.Dispose(); if (instance != null) instance.ReleaseMutex(); }
    }
}
