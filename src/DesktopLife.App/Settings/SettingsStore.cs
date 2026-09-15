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
            warning = "无法读取已保存的数量，已使用默认值。可重新设置后保存。";
            return new();
        }
    }

    public void Save(PopulationSettings settings)
    {
        settings.Validate();
        var fullPath = Path.GetFullPath(FilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, fullPath, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
