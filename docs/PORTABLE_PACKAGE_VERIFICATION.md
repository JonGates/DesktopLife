# Portable package verification

2026-09-15

- Application revision: 54bfb17 (localization and configurable global shortcuts).
- Package: DesktopLife-Portable-win-x64-54bfb17.zip, 71,290,641 bytes (about 68 MiB).
- SHA256: D305ACB44235604DD634C6C54744D606C6EBE929F19233C203764A34F49D2B02.
- Release self-contained win-x64 single-file publish, with Microsoft.NETCore.App and Microsoft.WindowsDesktop.App 10.0.12 included. Runtime configuration uses includedFrameworks instead of shared framework dependencies.
- Official runtime/host NuGet package downloads were verified against SHA512 values supplied by the NuGet CDN. Runtime redistribution notices are included.
- ZIP contains the app executable, Start-DesktopLife.cmd, Chinese/English README, build metadata and runtime notices; no developer preferences or debug symbols.
- Extracted the actual ZIP into a separate directory. Launched with DOTNET_ROOT / DOTNET_ROOT_X64 pointing to a nonexistent installation and DOTNET_MULTILEVEL_LOOKUP=0, with bundle extraction redirected to a test directory.
- Checked native modules: WPF graphics and presentation libraries loaded from the extracted bundle. No modules loaded from an installed dotnet/shared directory. The single-file host does not expose a separate coreclr.dll module.
- Native two-monitor bounds, click-through, no activation and single-instance settings opening passed using the extracted portable executable.
- This machine has .NET installed; a fresh Windows VM was not used. Self-contained metadata and observed module paths establish independence from that shared installation.
- Confirmed the UTF-8 Chinese README renders correctly when packaging via Windows PowerShell 5.1; script is stored with UTF-8 BOM for that shell.

Build again with scripts/Package-Portable.ps1. The script uses a fresh staging directory and refuses to overwrite an existing archive. The optional NuGetSource parameter supports a previously verified local package feed.
