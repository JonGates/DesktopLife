using System.Globalization;
namespace DesktopLife.Engine.ScreenSaving;

public enum ScreenSaverMode { Invalid, Configure, Run, Preview }
public readonly record struct ScreenSaverArguments(ScreenSaverMode Mode, long Parent = 0)
{
    public static bool IsInvocation(string[] args) => args.Length > 0 &&
        args[0].Split(':', 2)[0].ToLowerInvariant() is "/s" or "-s" or "/p" or "-p" or "/c" or "-c";
    public static ScreenSaverArguments Parse(string[] args)
    {
        if (args.Length == 0) return new(ScreenSaverMode.Configure);
        if (args.Length > 2) return new(ScreenSaverMode.Invalid);
        var parts = args[0].Split(':', 2);
        var option = parts[0].ToLowerInvariant();
        var value = parts.Length == 2 ? parts[1] : args.Length == 2 ? args[1] : null;
        if (parts.Length == 2 && args.Length == 2) return new(ScreenSaverMode.Invalid);
        if (option is "/s" or "-s") return new(value == null ? ScreenSaverMode.Run : ScreenSaverMode.Invalid);
        if (option is not ("/c" or "-c" or "/p" or "-p")) return new(ScreenSaverMode.Invalid);
        var preview = option[1] == 'p';
        long parent = 0;
        if (value != null && (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out parent) || parent < 0)) return new(ScreenSaverMode.Invalid);
        if (preview && parent == 0) return new(ScreenSaverMode.Invalid);
        return new(preview ? ScreenSaverMode.Preview : ScreenSaverMode.Configure, parent);
    }
}
