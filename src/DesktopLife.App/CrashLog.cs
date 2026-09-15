using System.IO;
namespace DesktopLife.App;
internal static class CrashLog
{
    public static void Write(object? exception)
    {
        try
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopLife", "logs");
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory, $"{DateTime.Now:yyyy-MM-dd}.log"), $"{DateTime.Now:O} {exception}{Environment.NewLine}");
        }
        catch (Exception) { /* Logging must not cause a second crash during shutdown. */ }
    }
}
