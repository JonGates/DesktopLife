using System.IO;
namespace DesktopLife.App.Settings;

public sealed class PreferencesController : IDisposable
{
    private readonly SettingsStore _store;
    private readonly DesktopHost _host;
    public CaptureProtection Capture { get; } = new();
    public event Action? CaptureFailed;
    public HotkeyService Hotkeys { get; }
    public AppPreferences Current { get; private set; }
    private string? _warningKey;
    private string? _captureWarning;
    public string? WarningKey => _captureWarning ?? _warningKey;
    public PreferencesController(DesktopHost host, SettingsStore store)
    {
        _store = store;
        _host = host;
        Current = store.LoadPreferences(out var invalid);
        Capture.Failed += () => { _captureWarning = "CaptureFailed"; CaptureFailed?.Invoke(); };
        try { Capture.Configure(Current.ExcludeFromCapture, () => { }); } catch (System.ComponentModel.Win32Exception) { _captureWarning = "CaptureFailed"; }
        host.WindowCreated += Capture.Track;
        foreach (var window in host.Overlays) Capture.Track(window);
        LanguageService.Apply(Current.Language);
        if (invalid) _warningKey = "ConfigWarning";
        Hotkeys = new(() => { if (host.IsPaused) host.TogglePause(); }, () => { if (!host.IsPaused) host.TogglePause(); });
        if (!Hotkeys.TryApply(Current.StartHotkey, Current.PauseHotkey, () => { }, out var error)) _warningKey = error;
    }
    public bool SaveLanguage(string language, out string error)
    {
        error = "";
        var next = Current with { Language = language };
        try { _store.SavePreferences(next); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { error = "SaveFailed"; return false; }
        Current = next;
        LanguageService.Apply(language);
        return true;
    }
    public bool SaveHotkeys(string start, string pause, out string error)
    {
        var next = Current with { StartHotkey = start, PauseHotkey = pause };
        try
        {
            if (!Hotkeys.TryApply(start, pause, () => _store.SavePreferences(next), out error)) return false;
            Current = next; _warningKey = null; return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { error = "SaveFailed"; return false; }
    }
    public bool SaveCapture(bool enabled, out string error)
    {
        var next = Current with { ExcludeFromCapture = enabled };
        try
        {
            Capture.Configure(enabled, () => _store.SavePreferences(next));
            Current = next; _captureWarning = null; error = ""; return true;
        }
        catch (System.ComponentModel.Win32Exception) { error = "CaptureFailed"; return false; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { error = "SaveFailed"; return false; }
    }
    public void Dispose() { _host.WindowCreated -= Capture.Track; Capture.Dispose(); Hotkeys.Dispose(); }
}
