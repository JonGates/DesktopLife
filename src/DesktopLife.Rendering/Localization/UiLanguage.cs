using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace DesktopLife.Rendering.Localization;

public sealed record LanguageOption(string Code, string Name);

/// <summary>Shared translations for the desktop app and standalone screen saver host.</summary>
public static class UiLanguage
{
    public static IReadOnlyList<LanguageOption> Supported { get; } = Array.AsReadOnly(new[] {
        new LanguageOption("zh-CN", "简体中文"), new("en-US", "English"), new("zh-TW", "繁體中文"), new("ja-JP", "日本語"), new("ko-KR", "한국어") });
    public static bool IsSupported(string? language) => Supported.Any(l => l.Code == language);
    public static int IndexOf(string language) => Math.Max(0, Supported.ToList().FindIndex(l => l.Code == language));
    private static readonly Dictionary<string, string[]> Translations = Load();
    private static readonly Dictionary<string, string> Traditional = LoadTraditional();
    private static Dictionary<string, string> LoadTraditional()
    {
        using var stream = typeof(UiLanguage).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Localization.zh-TW.json")!;
        return JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;
    }
    public static bool HasTraditional(string chinese) => chinese.Length == 0 || Traditional.ContainsKey(chinese);
    private static Dictionary<string, string[]> Load()
    {
        using var stream = typeof(UiLanguage).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Localization.translations.tsv")!;
        using var reader = new System.IO.StreamReader(stream);
        var result = new Dictionary<string, string[]>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split('|');
            if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace)) throw new InvalidOperationException("Invalid translation: " + line);
            result.Add(parts[0], [parts[1], parts[2]]);
        }
        return result;
    }
    public static bool HasTranslation(string english) => english.Length == 0 || Translations.ContainsKey(english);
    public static string Text(string language, string chinese, string english)
    {
        if (language == "zh-CN") return chinese;
        if (language == "zh-TW") return Traditional.GetValueOrDefault(chinese, chinese);
        if (language == "en-US" || !IsSupported(language) || english.Length == 0) return english;
        if (Translations.TryGetValue(english, out var translated)) return translated[language == "ko-KR" ? 1 : 0];
        return english; // New or unsupported text safely falls back to English.
    }
    public static string Format(string language, string chinese, string english, params object[] values) =>
        string.Format(CultureInfo.InvariantCulture, Text(language, chinese, english), values);
}
