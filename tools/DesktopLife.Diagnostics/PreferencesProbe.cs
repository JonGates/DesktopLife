using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLife.App;
using DesktopLife.App.Settings;
namespace DesktopLife.Diagnostics;

internal static class PreferencesProbe
{
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        foreach (var invalid in new[] { "S", "Ctrl+Ctrl+S", "Win+A", "Alt+F12", "Ctrl+", "Ctrl+Space", "Shift+F99" })
            Require(!Hotkey.TryParse(invalid, out _), "Accepted invalid shortcut: " + invalid);
        foreach (var valid in new[] { "", "Ctrl+Alt+S", "Shift+F11", "Alt+3", "ctrl + shift + p" })
        {
            Require(Hotkey.TryParse(valid, out var key), "Rejected valid shortcut");
            Require(Hotkey.TryParse(key.ToString(), out var roundTrip) && roundTrip == key, "Shortcut roundtrip failed");
        }
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        string? failure = null;
        app.Startup += (_, _) =>
        {
            try
            {
                NativeChecks();
                var store = new SettingsStore(Path.Combine(Path.GetFullPath(output), "settings.json"));
                store.SavePreferences(new("zh-CN", "", ""));
                using var host = new DesktopHost(app.Dispatcher, observeMouseClicks: false);
                host.Start();
                using var preferences = new PreferencesController(host, store);
                using var tray = new DesktopLife.App.Tray.TrayService(() => { }, () => { }, () => { });
                tray.SetPopulationSummary(2, 1, 20, 20, 3);
                var window = new SettingsWindow(host, store, preferences: preferences);
                window.Show(); window.UpdateLayout();
                ((TextBox)window.FindName("AntCount")).Text = "49";
                ((ComboBox)window.FindName("LanguagePicker")).SelectedIndex = 1;
                var traySettings = (System.Windows.Forms.ToolStripItem)typeof(DesktopLife.App.Tray.TrayService).GetField("_settings", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(tray)!;
                Require(traySettings.Text == "Settings…", "Tray language did not update");
                Require(window.Title == "DesktopLife · Settings", "Window title was not translated");
                Require(store.LoadPreferences(out _) is { Language: "en-US" }, "Language was not persisted");
                Require(((TextBox)window.FindName("AntCount")).Text == "49", "Language switch lost pending population edit");
                Require(((Button)window.FindName("ApplyButton")).Content.ToString() == "Save population", "Static resources were not translated");
                var start = (TextBox)window.FindName("StartKey");
                var pause = (TextBox)window.FindName("PauseKey");
                start.Text = "Ctrl+Shift+F8"; pause.Text = "Ctrl+Shift+F9";
                ((Button)window.FindName("SaveKeysButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Require(preferences.Current.StartHotkey == start.Text && store.LoadPreferences(out _) == preferences.Current, "Shortcut controls did not persist");
                Dispatch(preferences.Hotkeys, "Ctrl+Shift+F9"); Require(host.IsPaused, "Pause did not pause");
                Dispatch(preferences.Hotkeys, "Ctrl+Shift+F9"); Require(host.IsPaused, "Repeated pause toggled state");
                Dispatch(preferences.Hotkeys, "Ctrl+Shift+F8"); Require(!host.IsPaused, "Start did not resume");
                Dispatch(preferences.Hotkeys, "Ctrl+Shift+F8"); Require(!host.IsPaused, "Repeated start toggled state");
                Capture(window, Path.Combine(output, "settings-english.png"));
                FindScroll(window)?.ScrollToEnd(); window.UpdateLayout();
                Capture(window, Path.Combine(output, "shortcuts-english.png"));
                ((ComboBox)window.FindName("LanguagePicker")).SelectedIndex = 0;
                Capture(window, Path.Combine(output, "shortcuts-chinese.png"));
                start.Focus();
                preferences.Hotkeys.IsCapturing = true;
                typeof(Window).GetMethod("OnDeactivated", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(window, [EventArgs.Empty]);
                Require(!preferences.Hotkeys.IsCapturing, "Deactivation left capture enabled");
                window.Close();
                var reopened = new SettingsWindow(host, store, preferences: preferences);
                Require(((TextBox)reopened.FindName("StartKey")).Text == "Ctrl+Shift+F8", "Reopening lost shortcut");
                reopened.Close();
                var blocked = Path.Combine(Path.GetFullPath(output), "blocked-preferences");
                File.WriteAllText(blocked, "file");
                using var bad = new PreferencesController(host, new SettingsStore(Path.Combine(blocked, "settings.json")));
                var before = bad.Current;
                Require(!bad.SaveLanguage("en-US", out var error) && error == "SaveFailed" && bad.Current == before, "Failed language write mutated preferences");
                File.WriteAllText(store.PreferencesPath, "{bad");
                Require(store.LoadPreferences(out var invalid) == new AppPreferences() && invalid, "Corrupt preferences were not recovered");
                Console.WriteLine("PASS: parsing, native reservations/conflicts/rollback/dispatch/release, idempotent start/pause, language resources and persistence, controls, failed writes, reopening and screenshots.");
            }
            catch (Exception ex) { failure = ex.ToString(); }
            app.Shutdown(failure == null ? 0 : 1);
        };
        app.Run();
        File.WriteAllText(Path.Combine(output, "result.txt"), failure ?? "PASS");
        if (failure != null) throw new Exception(failure);
    }
    private static void NativeChecks()
    {
        var starts = 0; var pauses = 0;
        using var service = new HotkeyService(() => starts++, () => pauses++);
        Require(service.TryApply("Ctrl+Shift+F6", "Ctrl+Shift+F7", () => { }, out _), "Native registration failed");
        using (var blocker = new HotkeyService(() => { }, () => { }))
        {
            Require(blocker.TryApply("Ctrl+Shift+F10", "", () => { }, out _), "Blocker failed");
            Require(!service.TryApply("Ctrl+Shift+F10", "", () => throw new Exception("Persisted occupied keys"), out var error) && error == "OccupiedHotkey", "Conflict was not reported");
        }
        try { service.TryApply("Ctrl+Shift+F10", "", () => throw new IOException("test"), out _); throw new Exception("Write failure missing"); }
        catch (IOException) { }
        Dispatch(service, "Ctrl+Shift+F6"); Require(starts == 1, "Old shortcut lost after failure");
        Require(service.TryApply("Ctrl+Shift+F7", "Ctrl+Shift+F6", () => { }, out _), "Swap failed");
        Dispatch(service, "Ctrl+Shift+F6"); Require(pauses == 1, "Swap action incorrect");
        string? captured = null; service.Captured += key => captured = key;
        service.IsCapturing = true; Dispatch(service, "Ctrl+Shift+F7");
        Require(captured == "Ctrl+Shift+F7" && starts == 1, "Capturing activated an action");
        Require(service.TryApply("", "", () => { }, out _), "Disabling failed");
        using var released = new HotkeyService(() => { }, () => { });
        Require(released.TryApply("Ctrl+Shift+F6", "Ctrl+Shift+F10", () => { }, out _), "Registration was leaked");
    }
    private static void Dispatch(HotkeyService service, string text)
    {
        Hotkey.TryParse(text, out var key);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var source = (HwndSource)typeof(HotkeyService).GetField("_source", flags)!.GetValue(service)!;
        var registrations = (Dictionary<Hotkey, int>)typeof(HotkeyService).GetField("_registered", flags)!.GetValue(service)!;
        SendMessage(source.Handle, 0x0312, registrations[key], IntPtr.Zero);
    }
    private static ScrollViewer? FindScroll(DependencyObject root)
    {
        if (root is ScrollViewer viewer) return viewer;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            if (FindScroll(VisualTreeHelper.GetChild(root, i)) is { } child) return child;
        return null;
    }
    private static void Capture(Window window, string path)
    {
        window.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(path); encoder.Save(file);
    }
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
}
