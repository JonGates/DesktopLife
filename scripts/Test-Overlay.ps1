param([string]$Executable = "$PSScriptRoot/../src/DesktopLife.App/bin/Debug/net10.0-windows/DesktopLife.exe")
$ErrorActionPreference = 'Stop'
Add-Type @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class OverlayProbe {
    [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", EntryPoint="GetWindowLongPtrW")] public static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int index);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern IntPtr WindowFromPoint(Point point);
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);
    [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);
    delegate bool EnumWindowProc(IntPtr hwnd, IntPtr data);
    delegate bool EnumMonitorProc(IntPtr monitor, IntPtr dc, ref Rect rect, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowProc callback, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumDisplayMonitors(IntPtr dc, IntPtr clip, EnumMonitorProc callback, IntPtr data);
    public static IntPtr[] Windows(uint processId) {
        var result = new List<IntPtr>();
        EnumWindows((hwnd, data) => {
            uint owner; GetWindowThreadProcessId(hwnd, out owner);
            var title = new StringBuilder(256); GetWindowText(hwnd, title, 256);
            if(owner == processId && title.ToString().StartsWith("DesktopLife Overlay [")) result.Add(hwnd);
            return true;
        }, IntPtr.Zero);
        return result.ToArray();
    }
    public static Rect[] Displays() {
        var result = new List<Rect>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr monitor, IntPtr dc, ref Rect rect, IntPtr data) => { result.Add(rect); return true; }, IntPtr.Zero);
        return result.ToArray();
    }
    [StructLayout(LayoutKind.Sequential)] public struct Rect {
        public int Left,Top,Right,Bottom;
        public override string ToString() { return Left + "," + Top + "," + Right + "," + Bottom; }
    }
    [StructLayout(LayoutKind.Sequential)] public struct Point { public int X,Y; public Point(int x,int y){X=x;Y=y;} }
}
'@
$previousDpi = [OverlayProbe]::SetThreadDpiAwarenessContext([IntPtr]::new(-4))
$foreground = [OverlayProbe]::GetForegroundWindow()
$appProcess = Start-Process -FilePath (Resolve-Path -LiteralPath $Executable) -WindowStyle Hidden -PassThru
$windows = @()
try {
    $monitors = @([OverlayProbe]::Displays())
    for ($attempt=0; $attempt -lt 150; $attempt++) {
        Start-Sleep -Milliseconds 100
        if ($appProcess.HasExited) { throw "App exited early: $($appProcess.ExitCode). Exit other instances before testing." }
        $windows = @([OverlayProbe]::Windows($appProcess.Id))
        if ($windows.Count -eq $monitors.Count) { break }
    }
    if ($windows.Count -ne $monitors.Count) { throw "Expected $($monitors.Count) overlays, got $($windows.Count)" }
    Start-Sleep -Milliseconds 500
    $rows = @()
    foreach ($window in $windows) {
        $style = [OverlayProbe]::GetWindowLongPtr($window, -20).ToInt64()
        $required = 0x00080000 -bor 0x00000020 -bor 0x08000000 -bor 0x00000080 -bor 0x00000008
        if (($style -band $required) -ne $required) { throw ('Incorrect extended style: 0x{0:X}' -f $style) }
        if (($style -band 0x00040000) -ne 0) { throw 'WS_EX_APPWINDOW must be absent' }
        if (-not [OverlayProbe]::IsWindowVisible($window)) { throw 'Overlay is hidden' }
        $rect = New-Object OverlayProbe+Rect
        if (-not [OverlayProbe]::GetWindowRect($window, [ref]$rect)) { throw 'GetWindowRect failed' }
        foreach ($point in @([OverlayProbe+Point]::new($rect.Left+100,$rect.Top+100), [OverlayProbe+Point]::new($rect.Right-100,$rect.Top+80))) {
            if ($windows -contains [OverlayProbe]::WindowFromPoint($point)) { throw 'An overlay intercepted hit testing' }
        }
        $rows += [pscustomobject]@{ Styles=('0x{0:X}' -f $style); ClickThrough=$true; Bounds=$rect.ToString(); Dpi=[OverlayProbe]::GetDpiForWindow($window) }
    }
    $actual = @($rows.Bounds | Sort-Object)
    $expected = @($monitors | ForEach-Object { $_.ToString() } | Sort-Object)
    if (Compare-Object $expected $actual) { throw 'Overlay rectangles do not exactly match monitor rectangles' }
    if ([OverlayProbe]::GetForegroundWindow() -ne $foreground) { throw 'Foreground changed while showing overlays' }
    [pscustomobject]@{ MonitorCount=$monitors.Count; OverlayCount=$windows.Count; NoActivation=$true; Windows=$rows } | ConvertTo-Json -Depth 4
} finally {
    if (-not $appProcess.HasExited) {
        if ($windows.Count -gt 0) { [void][OverlayProbe]::SendMessage($windows[0], 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) }
        if (-not $appProcess.WaitForExit(3000)) { $appProcess.Kill() }
    }
    [void][OverlayProbe]::SetThreadDpiAwarenessContext($previousDpi)
}
