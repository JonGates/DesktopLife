param([int]$DurationSeconds = 60, [string]$Executable = "$PSScriptRoot/../artifacts/publish/DesktopLife.exe")
$ErrorActionPreference = 'Stop'
$fullExecutable = (Resolve-Path -LiteralPath $Executable).Path
if (Get-Process -Name DesktopLife -ErrorAction SilentlyContinue) { throw 'Exit DesktopLife from the tray before measuring.' }
$appProcess = Start-Process -FilePath $fullExecutable -WindowStyle Hidden -PassThru
try {
    Start-Sleep -Seconds 3
    $samples = @()
    for ($seconds = 0; $seconds -le $DurationSeconds; $seconds += 5) {
        $appProcess.Refresh()
        if ($appProcess.HasExited) { throw "App exited during measurement: $($appProcess.ExitCode)" }
        $samples += [pscustomobject]@{ Seconds=$seconds; CpuSeconds=$appProcess.TotalProcessorTime.TotalSeconds; WorkingSetMB=[math]::Round($appProcess.WorkingSet64/1MB,2); PrivateMB=[math]::Round($appProcess.PrivateMemorySize64/1MB,2); Handles=$appProcess.HandleCount }
        if ($seconds + 5 -le $DurationSeconds) { Start-Sleep -Seconds 5 }
    }
    $output = Join-Path $PSScriptRoot '../artifacts/runtime-release.json'
    $samples | ConvertTo-Json | Set-Content -LiteralPath $output -Encoding utf8
    $samples | Format-Table
} finally {
    if (-not $appProcess.HasExited) { $appProcess.Kill(); $appProcess.WaitForExit() }
}
