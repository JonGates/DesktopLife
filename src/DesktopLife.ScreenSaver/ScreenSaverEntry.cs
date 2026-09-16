using System.IO;
using System.Windows;
using System.Windows.Interop;
using DesktopLife.Engine.ScreenSaving;

namespace DesktopLife.ScreenSaver;

/// <summary>Shared Windows entry point for the desktop bundle and standalone SCR.</summary>
public static class ScreenSaverEntry
{
    public static void Start(Application app, string[] args)
    {
        var options = ScreenSaverArguments.Parse(args);
        if (options.Mode == ScreenSaverMode.Invalid) { app.Shutdown(1); return; }
        Mutex? instance = null;
        SaverSession? session = null;
        var ownsMutex = false;
        void Cleanup()
        {
            session?.Dispose(); session = null;
            if (ownsMutex) { instance!.ReleaseMutex(); ownsMutex = false; }
            instance?.Dispose(); instance = null;
        }
        try
        {
            if (options.Mode == ScreenSaverMode.Run)
            {
                instance = new Mutex(true, @"Local\DesktopLife.ScreenSaver.FullScreen", out ownsMutex);
                if (!ownsMutex) { instance.Dispose(); app.Shutdown(); return; }
            }
            app.Exit += (_, _) => Cleanup();
            app.DispatcherUnhandledException += (_, e) => { Log(e.Exception); e.Handled = true; app.Shutdown(1); };
            var store = new SaverSettingsStore();
            if (options.Mode == ScreenSaverMode.Configure)
            {
                var window = new ConfigurationWindow(store);
                if (options.Parent != 0 && SaverNative.IsWindow((nint)options.Parent)) new WindowInteropHelper(window).Owner = (nint)options.Parent;
                window.Closed += (_, _) => app.Shutdown();
                window.Show();
            }
            else session = new SaverSession(app, store.Load(out _), options.Mode == ScreenSaverMode.Preview ? (nint)options.Parent : 0);
        }
        catch (Exception e) { Log(e); Cleanup(); app.Shutdown(1); }
    }
    private static void Log(Exception e)
    {
        try
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopLife");
            Directory.CreateDirectory(directory); File.WriteAllText(Path.Combine(directory, "screensaver-error.log"), e.ToString());
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
    }
}
