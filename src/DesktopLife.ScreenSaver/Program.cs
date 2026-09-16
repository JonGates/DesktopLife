using System.Windows;
namespace DesktopLife.ScreenSaver;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Startup += (_, _) => ScreenSaverEntry.Start(app, args);
        return app.Run();
    }
}
