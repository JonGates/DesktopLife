using System.IO;
using System.Text.Json;
using DesktopLife.Creatures.Displays;
namespace DesktopLife.ScreenSaver;

public sealed record SaverSettings(bool Light = false, int Cockroaches = 20, int Ants = 20, int Caterpillars = 3)
{
    [System.Text.Json.Serialization.JsonIgnore]
    public PopulationSettings Population => new(Cockroaches, Ants, Caterpillars);
}

public sealed class SaverSettingsStore(string? path = null)
{
    public string Path { get; } = path ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DesktopLife", "screensaver.json");
    public SaverSettings Load(out string? warning)
    {
        warning = null;
        try
        {
            if (!File.Exists(Path)) return new();
            var settings = JsonSerializer.Deserialize<SaverSettings>(File.ReadAllText(Path)) ?? throw new JsonException();
            settings.Population.Validate();
            return settings;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or ArgumentOutOfRangeException)
        {
            warning = "配置无法读取，已使用默认值。 / Could not read settings; using defaults.";
            return new();
        }
    }
    public void Save(SaverSettings settings)
    {
        settings.Population.Validate();
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path))!);
        var temporary = Path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, Path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
