param(
    [string]$Executable = "$PSScriptRoot/../artifacts/publish/DesktopLife.exe",
    [switch]$KeepRunning
)
$ErrorActionPreference = 'Stop'
Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class SettingsInstanceProbe {
    delegate bool EnumProc(IntPtr hwnd, IntPtr data);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc callback, IntPtr data);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr hwnd, StringBuilder title, int count);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
    public static int CountSettings(uint processId) {
        int result = 0;
        EnumWindows((hwnd, data) => {
            uint owner; GetWindowThreadProcessId(hwnd, out owner);
            var title = new StringBuilder(256); GetWindowText(hwnd, title, 256);
            if(owner == processId && IsWindowVisible(hwnd) && title.ToString().StartsWith("DesktopLife ") &&
                !title.ToString().StartsWith("DesktopLife Overlay")) result++;
            return true;
        }, IntPtr.Zero);
        return result;
    }
}
'@
$resolvedExecutable = (Resolve-Path -LiteralPath $Executable).Path
$firstInstance = Start-Process -FilePath $resolvedExecutable -WindowStyle Hidden -PassThru
$secondInstance = $null
$passed = $false
try {
    Start-Sleep -Seconds 2
    $firstInstance.Refresh()
    if ($firstInstance.HasExited) { throw 'Background instance exited. Exit other running instances before this test.' }
    if ([SettingsInstanceProbe]::CountSettings($firstInstance.Id) -ne 0) { throw 'Background launch unexpectedly showed settings' }
    $secondInstance = Start-Process -FilePath $resolvedExecutable -ArgumentList '--settings' -WindowStyle Hidden -PassThru
    if (-not $secondInstance.WaitForExit(5000)) { throw 'Second instance did not exit' }
    if ($secondInstance.ExitCode -ne 0) { throw 'Second instance failed' }
    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        if ([SettingsInstanceProbe]::CountSettings($firstInstance.Id) -eq 1) { break }
        Start-Sleep -Milliseconds 100
    }
    if ([SettingsInstanceProbe]::CountSettings($firstInstance.Id) -ne 1) { throw 'Existing process did not show one settings window' }
    $passed = $true
    [pscustomobject]@{ Passed=$true; ProcessId=$firstInstance.Id; SettingsWindows=1; SecondaryExited=$true; KeptRunning=[bool]$KeepRunning } | ConvertTo-Json
} finally {
    if ($secondInstance -and -not $secondInstance.HasExited) { $secondInstance.Kill() }
    if (-not ($passed -and $KeepRunning) -and -not $firstInstance.HasExited) { $firstInstance.Kill() }
}
