namespace DesktopLife.App.Settings;

public sealed record AppPreferences(string Language = "zh-CN", string StartHotkey = "Ctrl+Alt+S", string PauseHotkey = "Ctrl+Alt+P");
