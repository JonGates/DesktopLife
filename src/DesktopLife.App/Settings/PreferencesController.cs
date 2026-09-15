using System.IO;
namespace DesktopLife.App.Settings;

public sealed class PreferencesController : IDisposable
{
    private readonly SettingsStore _store;
    public HotkeyService Hotkeys { get; }
    public AppPreferences Current { get; private set; }
    public string? WarningKey { get; private set; }
    public PreferencesController(DesktopHost host, SettingsStore store)
    {
        _store = store;
        Current = store.LoadPreferences(out var invalid);
        LanguageService.Apply(Current.Language);
        if (invalid) WarningKey = "ConfigWarning";
        Hotkeys = new(() => { if (host.IsPaused) host.TogglePause(); }, () => { if (!host.IsPaused) host.TogglePause(); });
        if (!Hotkeys.TryApply(Current.StartHotkey, Current.PauseHotkey, () => { }, out var error)) WarningKey = error;
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
            Current = next; WarningKey = null; return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { error = "SaveFailed"; return false; }
    }
    public void Dispose() => Hotkeys.Dispose();
}
