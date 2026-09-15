# DesktopLife · Windows Desktop Pets & Interactive Insects

[简体中文](README.md) · **English** · [Download for Windows](https://github.com/JonGates/DesktopLife/releases) · [Report a bug](https://github.com/JonGates/DesktopLife/issues)

**Bring your Windows desktop to life with interactive insect companions.** DesktopLife adds a mouse-following fly and roaming ants, cockroaches and caterpillars to your desktop. Creatures move between adjacent monitors, with configurable populations and global pause/resume hotkeys.

Built with **C# / .NET 10 / WPF / Win32** for **Windows 10/11 x64**. The portable download includes the runtime; extract it and run.

Also available as a **Windows `.scr` screen saver** with dark/light backgrounds, autonomous insects, multi-monitor support and separate population settings.

![DesktopLife Windows desktop pets: insects roaming on the primary monitor](docs/images/live-desktop.gif)

## Download and run

**[Download DesktopLife v0.1.0 for Windows x64 — portable ZIP](https://github.com/JonGates/DesktopLife/releases/download/v0.1.0/DesktopLife-Portable-win-x64-v0.1.0.zip)**

Version **v0.1.0 is a preview release**. [Release notes and checksums](https://github.com/JonGates/DesktopLife/releases/tag/v0.1.0).

### Screen saver v0.2.0

**[Download the self-contained screen saver ZIP](https://github.com/JonGates/DesktopLife/releases/download/v0.2.0/DesktopLife-ScreenSaver-win-x64-v0.2.0.zip)** · [Release notes](https://github.com/JonGates/DesktopLife/releases/tag/v0.2.0)

Extract it, run `Configure-ScreenSaver.cmd` to choose **Dark / Light** and populations, and try `Preview-FullScreen.cmd`. Run `Install-ScreenSaver.cmd` to select it in Windows Screen Saver Settings and set the idle timeout. Move the mouse or press a key to exit. Keep the extracted folder in place. [Full bilingual instructions](docs/SCREENSAVER.md).

### Desktop companion v0.1.0

1. Open the release page and download **`DesktopLife-Portable-win-x64-v0.1.0.zip`** from **Assets**.
2. Extract the entire ZIP into a folder.
3. Double-click **`Start-DesktopLife.cmd`** to launch the app and open settings. You can also run **`DesktopLife.exe`** and double-click its system tray icon to open settings.
4. Choose **English** in the language selector, adjust insect counts, and click **Save population**.

No separate .NET runtime, SDK or Visual Studio installation is required for the portable package. GitHub's **Source code (zip/tar.gz)** downloads contain source files; choose the portable ZIP to run the app.

## Features

| Feature | What it does |
| --- | --- |
| Mouse-following fly | One fly darts and hovers near your cursor. Left-click to choose a landing point; it lands, grooms for 3 seconds, then resumes flying. |
| Configurable insects | Set cockroaches and ants to 0–500 each, and caterpillars to 0–100. Set a species to 0 to disable it. The fly stays fixed at one. |
| Multi-monitor desktop | Creatures cross touching screen edges according to your Windows display layout, including stacked and offset monitors. All monitors share one population. |
| Global hotkeys | **Ctrl+Alt+P** pauses and **Ctrl+Alt+S** resumes. Customize both shortcuts in settings. The app must be running for shortcuts to work. |
| Chinese and English | Settings and tray menus switch language immediately. |
| System tray controls | Close settings to keep insects running. Right-click the tray icon and choose **Exit** to stop the app. |
| Portable Windows app | A self-contained x64 ZIP with the runtime included. Preferences are stored in `%AppData%\DesktopLife`. |

## Live recordings

These GIFs were captured from the **primary monitor** while DesktopLife was running, against a static presentation background. The recordings use higher insect counts than the defaults. Close-ups are cropped and resized; motion plays at the captured speed.

### Click, land, groom, and fly again

![Mouse-following desktop fly landing after a click and resuming flight](docs/images/live-fly.gif)

### Live insects and bilingual settings

![DesktopLife settings with live insects and Chinese-to-English language switching](docs/images/live-settings.gif)

## Frequently asked questions

### Does adding a monitor add more insects?

All screens share the configured total. Adding or removing a monitor preserves the population. Crawling insects cross edges where screens actually touch; gaps and corner-only connections do not form a crossing route.

### How do I pause or quit?

Use **Ctrl+Alt+P**, the settings button, or the tray menu to pause. Use **Ctrl+Alt+S** to resume. Closing the settings window keeps the app running; choose **Exit** in its tray menu to quit.

### Can I hide the insects during screen sharing?

Settings includes an optional capture-exclusion switch, disabled by default. It uses Windows `WDA_EXCLUDEFROMCAPTURE` and requires Windows 10 version 2004 or later. It works only with compatible capture tools and cannot guarantee exclusion from every recorder. Tray icons and the process remain visible. [Windows API limitations](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity).

## Build from source

Install the **.NET 10 SDK** compatible with [`global.json`](global.json), then run on Windows:

```powershell
git clone https://github.com/JonGates/DesktopLife.git
cd DesktopLife
dotnet build DesktopLife.sln
dotnet test DesktopLife.sln
dotnet run --project src/DesktopLife.App -- --settings
```

Create a self-contained Windows x64 portable package:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-Portable.ps1 -Version 0.1.0
```

Output: `artifacts/DesktopLife-Portable-win-x64-v0.1.0.zip` and its SHA256 file. The `artifacts/` directory is generated locally and excluded from Git.

See the [development and packaging guide (Chinese)](docs/DEVELOPMENT_AND_PACKAGING.md), [recording guide (Chinese)](docs/images/README.md), and [sprite asset notes (Chinese)](src/DesktopLife.Rendering/Assets/README.md).

## Feedback

[Open an issue](https://github.com/JonGates/DesktopLife/issues) with your Windows version, DesktopLife version, monitor arrangement and steps to reproduce the problem. Animation feedback and compatibility reports are welcome.
