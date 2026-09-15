using System.ComponentModel;
using System.Runtime.InteropServices;
using DesktopLife.Engine.Input;
namespace DesktopLife.Windows;

/// <summary>Read-only left-button observation on the UI message-loop thread; never consumes input.</summary>
public sealed class GlobalMouseClickSource : IDisposable
{
    private readonly MouseClickBuffer _buffer;
    private readonly HookProc _callback;
    private nint _hook;
    public bool IsInstalled => _hook != 0;

    public GlobalMouseClickSource(MouseClickBuffer buffer)
    {
        _buffer = buffer;
        _callback = OnMouse;
        _hook = SetWindowsHookEx(14, _callback, GetModuleHandle(null), 0); // WH_MOUSE_LL
        if (_hook == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    private nint OnMouse(int code, nint message, nint data)
    {
        if (code >= 0 && message == 0x0201 && _buffer.Enabled) // WM_LBUTTONDOWN
        {
            var input = Marshal.PtrToStructure<MouseHookData>(data);
            // Ignore our settings/menu windows. A transparent overlay is never an input control.
            var target = WindowFromPoint(input.Point);
            GetWindowThreadProcessId(target, out var process);
            var transparentOverlay = (NativeMethods.GetWindowLongPtr(target, WindowStyles.ExtendedStyleIndex).ToInt64() & WindowStyles.Transparent) != 0;
            if (process != (uint)Environment.ProcessId || transparentOverlay)
                _buffer.Record(new(input.Point.X, input.Point.Y));
        }
        return CallNextHookEx(_hook, code, message, data);
    }
    public void Dispose()
    {
        if (_hook == 0) return;
        UnhookWindowsHookEx(_hook);
        _hook = 0;
        GC.KeepAlive(_callback);
    }
    private delegate nint HookProc(int code, nint message, nint data);
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] private struct MouseHookData
    {
        public Point Point;
        public uint MouseData, Flags, Time;
        public nuint ExtraInfo;
    }
    [DllImport("user32.dll", EntryPoint = "SetWindowsHookExW", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int id, HookProc callback, nint module, uint thread);
    [DllImport("user32.dll")] private static extern nint CallNextHookEx(nint hook, int code, nint message, nint data);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
    private static extern nint GetModuleHandle(string? name);
    [DllImport("user32.dll")] private static extern nint WindowFromPoint(Point point);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint process);
}
