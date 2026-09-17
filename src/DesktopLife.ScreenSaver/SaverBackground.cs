using System.IO;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

namespace DesktopLife.ScreenSaver;

public static class SaverBackground
{
    public static BitmapSource? Load(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        try
        {
            using var stream = File.OpenRead(path);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 3840;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or System.IO.FileFormatException)
        { return null; }
    }

    public static string? Import(string? source, string settingsPath)
    {
        if (string.IsNullOrWhiteSpace(source)) return null;
        if (Load(source) == null) throw new IOException("Background image is unreadable.");
        var directory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(settingsPath))!, "backgrounds");
        Directory.CreateDirectory(directory);
        using var stream = File.OpenRead(source);
        var hash = Convert.ToHexString(SHA256.HashData(stream));
        directory = Path.Combine(directory, hash);
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, Path.GetFileName(source));
        if (!File.Exists(target))
        {
            var temporary = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                stream.Position = 0;
                using (var destination = File.Create(temporary)) stream.CopyTo(destination);
                File.Move(temporary, target, true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        return target;
    }
}
