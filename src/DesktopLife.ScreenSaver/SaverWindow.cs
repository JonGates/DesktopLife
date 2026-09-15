using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using DesktopLife.Engine.World;
namespace DesktopLife.ScreenSaver;

internal sealed class SaverWindow : Window
{
    private readonly WorldBounds _bounds;
    private bool _closed;
    public SaverWindow(WorldBounds bounds, SaverSurface surface)
    {
        _bounds = bounds;
        Title = "DesktopLife Screen Saver";
        WindowStyle = WindowStyle.None; ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false; Topmost = true; ShowActivated = false;
        Cursor = Cursors.None; Content = surface;
        SourceInitialized += (_, _) => Place();
        Loaded += (_, _) => Place();
        Closed += (_, _) => _closed = true;
    }
    private void Place()
    {
        if (_closed) return;
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != 0 && !SaverNative.SetWindowPos(hwnd, -1, (int)_bounds.Left, (int)_bounds.Top, (int)_bounds.Width, (int)_bounds.Height, 0x0010 | 0x0040))
            throw new Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error());
    }
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(Place));
    }
}
