using System.ComponentModel;
using System.Runtime.InteropServices;
namespace DesktopLife.Windows;
public static class OverlayWindowHelper
{
    public static void Configure(nint window, MonitorBounds bounds)
    {
        var styles = NativeMethods.GetWindowLongPtr(window, WindowStyles.ExtendedStyleIndex).ToInt64();
        styles = (styles | WindowStyles.Layered | WindowStyles.Transparent | WindowStyles.NoActivate | WindowStyles.ToolWindow) & ~WindowStyles.AppWindow;
        Marshal.SetLastPInvokeError(0);
        var previous = NativeMethods.SetWindowLongPtr(window, WindowStyles.ExtendedStyleIndex, (nint)styles);
        if (previous == 0 && Marshal.GetLastWin32Error() != 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        PlaceOnMonitor(window, bounds);
    }

    public static void PlaceOnMonitor(nint window, MonitorBounds bounds)
    {
        // HWND_TOPMOST; SWP_NOACTIVATE | SWP_FRAMECHANGED. Physical pixels.
        if (!NativeMethods.SetWindowPos(window, -1, bounds.Left, bounds.Top, bounds.Width, bounds.Height, 0x0010 | 0x0020))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }
}
