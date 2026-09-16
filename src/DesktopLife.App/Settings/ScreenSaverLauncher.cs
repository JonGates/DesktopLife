using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace DesktopLife.App.Settings;

public static class ScreenSaverLauncher
{
    public static ProcessStartInfo SelfCommand(string argument)
    {
        if (argument is not ("/s" or "/c")) throw new ArgumentOutOfRangeException(nameof(argument));
        var info = new ProcessStartInfo(Environment.ProcessPath ?? throw new IOException("Executable unavailable")) { UseShellExecute = false, CreateNoWindow = true };
        info.ArgumentList.Add(argument); return info;
    }

    public static string PrepareInstall(string source, string directory)
    {
        // The portable bundle contains the runtime and saver; a development apphost does not.
        if (File.Exists(Path.ChangeExtension(source, ".dll"))) throw new InvalidOperationException("Use the self-contained portable build to install the screen saver.");
        using var input = File.OpenRead(source);
        var hash = Convert.ToHexString(SHA256.HashData(input));
        var folder = Path.Combine(directory, hash[..16]); Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, "DesktopLife.scr");
        if (File.Exists(target))
        {
            using var existing = File.OpenRead(target);
            if (Convert.ToHexString(SHA256.HashData(existing)) == hash) return target;
        }
        var temporary = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.Copy(source, temporary); File.Move(temporary, target, true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        return target;
    }

    public static ProcessStartInfo WindowsSettingsCommand(string scr)
    {
        var info = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "rundll32.exe")) { UseShellExecute = false, CreateNoWindow = true };
        info.ArgumentList.Add("desk.cpl,InstallScreenSaver"); info.ArgumentList.Add(scr); return info;
    }

    public static string InstallCopy() => PrepareInstall(Environment.ProcessPath ?? throw new IOException("Executable unavailable"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopLife", "ScreenSaver"));

    public static (bool Selected, bool Enabled, int? Seconds) ReadRegistration()
    {
        using var desktop = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
        using var policy = Registry.CurrentUser.OpenSubKey(@"Software\Policies\Microsoft\Windows\Control Panel\Desktop");
        string? Value(string key) => (policy?.GetValue(key) ?? desktop?.GetValue(key))?.ToString();
        var path = Environment.ExpandEnvironmentVariables(Value("SCRNSAVE.EXE")?.Trim('"') ?? "");
        var selected = File.Exists(path) && Path.GetFileName(path).Equals("DesktopLife.scr", StringComparison.OrdinalIgnoreCase);
        return (selected, Value("ScreenSaveActive") == "1", int.TryParse(Value("ScreenSaveTimeOut"), out var seconds) ? seconds : null);
    }
}
