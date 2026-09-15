using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace DesktopLife.App.Settings;

public readonly record struct Hotkey(uint Modifiers, uint Key)
{
    public override string ToString() => Key == 0 ? "" :
        ((Modifiers & 2) != 0 ? "Ctrl+" : "") + ((Modifiers & 1) != 0 ? "Alt+" : "") + ((Modifiers & 4) != 0 ? "Shift+" : "") +
        (Key is >= 112 and <= 122 ? $"F{Key - 111}" : ((char)Key).ToString());
    public static bool TryParse(string? text, out Hotkey hotkey)
    {
        hotkey = default;
        if (string.IsNullOrWhiteSpace(text)) return true;
        var parts = text.Split('+', StringSplitOptions.TrimEntries);
        uint modifiers = 0;
        foreach (var part in parts[..^1])
        {
            var flag = part.ToUpperInvariant() switch { "CTRL" => 2u, "ALT" => 1u, "SHIFT" => 4u, _ => 0u };
            if (flag == 0 || (modifiers & flag) != 0) return false;
            modifiers |= flag;
        }
        var key = parts[^1].ToUpperInvariant();
        uint code;
        if (key.Length == 1 && char.IsAsciiLetterOrDigit(key[0])) code = key[0];
        else if (key.StartsWith('F') && int.TryParse(key[1..], out var f) && f is >= 1 and <= 11) code = (uint)(111 + f);
        else return false;
        if (modifiers == 0) return false;
        hotkey = new(modifiers, code);
        return true;
    }
}

/// <summary>Reserves new combinations before persistence, retaining old registrations on failure.</summary>
public sealed class HotkeyService : IDisposable
{
    private readonly HwndSource _source = new(new HwndSourceParameters("DesktopLifeHotkeys") { ParentWindow = new IntPtr(-3) });
    private readonly Dictionary<Hotkey, int> _registered = [];
    private readonly Action _start;
    private readonly Action _pause;
    private Hotkey _startKey, _pauseKey;

    public bool IsCapturing { get; set; }
    public event Action<string>? Captured;
    public HotkeyService(Action start, Action pause)
    {
        _start = start; _pause = pause;
        _source.AddHook(ProcessMessage);
    }
    public bool TryApply(string start, string pause, Action persist, out string error)
    {
        error = "";
        if (!Hotkey.TryParse(start, out var a) || !Hotkey.TryParse(pause, out var b)) { error = "InvalidHotkey"; return false; }
        if (a.Key != 0 && a == b) { error = "DuplicateHotkey"; return false; }
        var desired = new[] { a, b }.Where(k => k.Key != 0).ToHashSet();
        var added = new Dictionary<Hotkey, int>();
        var committed = false;
        try
        {
            foreach (var key in desired.Where(k => !_registered.ContainsKey(k)))
            {
                var id = Enumerable.Range(100, 0xBF00).First(i => !_registered.ContainsValue(i) && !added.ContainsValue(i));
                if (!RegisterHotKey(_source.Handle, id, key.Modifiers | 0x4000, key.Key)) { error = "OccupiedHotkey"; return false; }
                added.Add(key, id);
            }
            persist();
            foreach (var old in _registered.Where(p => !desired.Contains(p.Key)).ToArray())
            { UnregisterHotKey(_source.Handle, old.Value); _registered.Remove(old.Key); }
            foreach (var pair in added) _registered.Add(pair.Key, pair.Value);
            _startKey = a; _pauseKey = b;
            committed = true;
            return true;
        }
        finally
        {
            if (!committed) foreach (var id in added.Values) UnregisterHotKey(_source.Handle, id);
        }
    }
    private IntPtr ProcessMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != 0x0312) return IntPtr.Zero;
        var key = _registered.FirstOrDefault(p => p.Value == wParam.ToInt32()).Key;
        if (key.Key == 0) return IntPtr.Zero;
        handled = true;
        if (IsCapturing) { Captured?.Invoke(key.ToString()); return IntPtr.Zero; }
        if (key == _startKey) _start();
        else if (key == _pauseKey) _pause();
        return IntPtr.Zero;
    }
    public void Dispose()
    {
        foreach (var id in _registered.Values) UnregisterHotKey(_source.Handle, id);
        _registered.Clear(); _source.RemoveHook(ProcessMessage); _source.Dispose();
    }
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hwnd, int id);
}
