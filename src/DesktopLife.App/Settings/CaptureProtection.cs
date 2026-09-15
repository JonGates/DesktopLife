using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
namespace DesktopLife.App.Settings;

/// <summary>Uses the public Windows capture exclusion API for this process's windows only.</summary>
public sealed class CaptureProtection : IDisposable
{
    private readonly HashSet<Window> _windows = [];
    public bool Enabled { get; private set; }
    public event Action? Failed;
    public void Track(Window window)
    {
        if (!_windows.Add(window)) return;
        window.SourceInitialized += Initialized;
        window.Closed += Closed;
        if (new WindowInteropHelper(window).Handle != IntPtr.Zero) Initialize(window);
    }
    private void Initialized(object? sender, EventArgs e) => Initialize((Window)sender!);
    private void Initialize(Window window)
    {
        try { Apply(window, Enabled); }
        catch (Win32Exception) { Failed?.Invoke(); }
    }
    private void Closed(object? sender, EventArgs e)
    {
        var window = (Window)sender!;
        window.SourceInitialized -= Initialized; window.Closed -= Closed; _windows.Remove(window);
    }
    public void Configure(bool enabled, Action persist)
    {
        if (enabled && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041)) throw new Win32Exception(50);
        try
        {
            foreach (var window in _windows) Apply(window, enabled);
            persist();
            Enabled = enabled;
        }
        catch
        {
            foreach (var window in _windows) { try { Apply(window, Enabled); } catch (Win32Exception) { Failed?.Invoke(); } }
            throw;
        }
    }
    private static void Apply(Window window, bool enabled)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;
        if (!SetWindowDisplayAffinity(handle, enabled ? 0x11u : 0u)) throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    public void Dispose()
    {
        foreach (var window in _windows) { window.SourceInitialized -= Initialized; window.Closed -= Closed; }
        _windows.Clear();
    }
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint affinity);
}
