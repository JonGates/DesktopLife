# Icon and capture exclusion verification

- Reproducible WPF vector drawing exported to PNG and a seven-size ICO (16, 24, 32, 48, 64, 128, 256). Applied to executable resource, settings window and tray icon.
- Optional ExcludeFromCapture preference defaults false; older JSON uses that default. Applies WDA_EXCLUDEFROMCAPTURE (0x11) to own top-level creature/settings windows only. Windows 10 build 19041 or later required for exclusion.
- Native diagnostic checked GetWindowDisplayAffinity=0x11 on both actual monitor overlays and settings; disabling restored 0. A newly created tracked window inherited 0x11 before display. Injected persistence failure restored prior affinity and enabled state.
- Settings/tray resources, language and shortcut diagnostics passed. Existing 93 unit tests passed; Release build succeeded.
- Reviewed rendered icon and Chinese settings layout. The diagnostic verified Windows affinity flags, not specific third-party recording/monitoring products. No guarantee is made for capture software that ignores this API. Taskbar/tray icons and the process remain visible.
- Capture failures on new windows notify the controller and open settings asynchronously; capture warnings remain separate from hotkey warnings.

Reference: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity
