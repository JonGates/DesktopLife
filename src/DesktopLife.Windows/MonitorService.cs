using System.ComponentModel;
using System.Runtime.InteropServices;
using DesktopLife.Engine.World;
namespace DesktopLife.Windows;
public readonly record struct MonitorBounds(int Left, int Top, int Width, int Height);
public static class MonitorService
{
    public static IReadOnlyList<DisplayArea> GetDisplays()
    {
        var displays = new List<DisplayArea>();
        var error = 0;
        bool Visit(nint monitor, nint hdc, ref NativeMethods.Rect rect, nint data)
        {
            var info = new NativeMethods.MonitorInfoEx { Size = Marshal.SizeOf<NativeMethods.MonitorInfoEx>(), DeviceName = "" };
            if (!NativeMethods.GetMonitorInfo(monitor, ref info))
            {
                error = Marshal.GetLastWin32Error();
                return false;
            }
            var r = info.Monitor;
            displays.Add(new(info.DeviceName, new(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top), (info.Flags & 1) != 0));
            return true;
        }
        if (!NativeMethods.EnumDisplayMonitors(0, 0, Visit, 0))
            throw new Win32Exception(error != 0 ? error : Marshal.GetLastWin32Error());
        return Array.AsReadOnly(displays.OrderByDescending(d => d.IsPrimary).ThenBy(d => d.Id, StringComparer.Ordinal).ToArray());
    }

    public static MonitorBounds GetPrimaryBounds()
    {
        var monitor = NativeMethods.MonitorFromPoint(default, 1);
        var info = new NativeMethods.MonitorInfo { Size = Marshal.SizeOf<NativeMethods.MonitorInfo>() };
        if (!NativeMethods.GetMonitorInfo(monitor, ref info)) throw new Win32Exception(Marshal.GetLastWin32Error());
        var rect = info.Monitor;
        return new(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }
}
