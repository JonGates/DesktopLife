using System.IO;
using System.Text.Json;
using DesktopLife.Creatures.Displays;
namespace DesktopLife.App.Settings;

public sealed class SettingsStore(string? filePath = null)
{
    public string FilePath { get; } = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DesktopLife", "settings.json");
    public PopulationSettings Load(out string? warning)
    {
        warning = null;
        try
        {
            if (!File.Exists(FilePath)) return new();
            var settings = JsonSerializer.Deserialize<PopulationSettings>(File.ReadAllText(FilePath)) ?? throw new JsonException();
            settings.Validate();
            return settings;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or ArgumentOutOfRangeException)
        {
            warning = LanguageService.Get("ConfigWarning");
            return new();
        }
    }

    public void Save(PopulationSettings settings)
    {
        settings.Validate();
        Write(FilePath, settings);
    }
    private static void Write<T>(string path, T settings)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, fullPath, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public string PreferencesPath => Path.ChangeExtension(FilePath, ".preferences.json");
    public AppPreferences LoadPreferences(out bool invalid)
    {
        invalid = false;
        try
        {
            if (!File.Exists(PreferencesPath)) return new();
            var value = JsonSerializer.Deserialize<AppPreferences>(File.ReadAllText(PreferencesPath)) ?? throw new JsonException();
            if (value.StartHotkey == null || value.PauseHotkey == null || value.Language is not ("zh-CN" or "en-US") || !Hotkey.TryParse(value.StartHotkey, out var a) || !Hotkey.TryParse(value.PauseHotkey, out var b) || (a.Key != 0 && a == b)) throw new JsonException();
            return value;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        { invalid = true; return new(); }
    }
    public void SavePreferences(AppPreferences value) => Write(PreferencesPath, value);
}
