using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using DesktopLife.App.Settings;
using DesktopLife.ScreenSaver;
using DesktopLife.Rendering.Localization;
using DesktopLife.Engine.Creatures;

namespace DesktopLife.Diagnostics;
internal static class LocalizationProbe
{
    public static void Run(string output)
    {
        Directory.CreateDirectory(output);
        var texts = (Dictionary<string, (string Zh, string En)>)typeof(LanguageService).GetField("Texts", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        foreach (var text in texts.Values)
            if (!UiLanguage.HasTranslation(text.En) || !UiLanguage.HasTraditional(text.Zh)) throw new Exception("Missing translation: " + text.En);
        foreach (var species in InsectCatalog.Additional.Concat(OceanCatalog.Fish))
            if (!UiLanguage.HasTranslation(species.EnglishName)) throw new Exception("Missing species: " + species.EnglishName);
        var main = new SettingsStore(Path.GetFullPath(Path.Combine(output, "main.json")));
        var saver = new SaverSettingsStore(Path.GetFullPath(Path.Combine(output, "saver.json")));
        foreach (var option in UiLanguage.Supported)
        {
            LanguageService.Apply(option.Code);
            foreach (var pair in texts)
                if (pair.Key != "Unit" && string.IsNullOrWhiteSpace(LanguageService.Get(pair.Key))) throw new Exception("Empty resource: " + pair.Key);
            main.SavePreferences(new(Language: option.Code, StartHotkey: "", PauseHotkey: ""));
            if (main.LoadPreferences(out var invalid).Language != option.Code || invalid) throw new Exception("Desktop language did not round trip");
            saver.Save(new(Language: option.Code, Cockroaches: 47, RainLevel: 5));
            var restored = saver.Load(out var warning);
            if (restored.Language != option.Code || warning != null || restored.Cockroaches != 47 || restored.RainLevel != 5) throw new Exception("Saver language did not round trip");
            if (!LanguageService.Format("{0} 块屏幕", "{0} displays", 3).Contains('3')) throw new Exception("Format argument lost");
        }
        LanguageService.Apply("zh-TW");
        if (!LanguageService.Get("Title").Contains("設定")) throw new Exception("Traditional Chinese conversion failed");
        LanguageService.Apply("ja-JP"); if (LanguageService.Get("Language") != "言語") throw new Exception("Japanese not active");
        LanguageService.Apply("ko-KR"); if (LanguageService.Get("Language") != "언어") throw new Exception("Korean not active");
        LanguageService.Apply("invalid"); if (LanguageService.Current != "zh-CN") throw new Exception("Invalid language fallback failed");
        using var stream = typeof(UiLanguage).Assembly.GetManifestResourceStream("DesktopLife.Rendering.Localization.translations.tsv")!;
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var fields = line.Split('|');
            string[] Arguments(string text) => Regex.Matches(text, @"\{\d+\}").Select(m => m.Value).Order().ToArray();
            foreach (var translated in fields.Skip(1))
                if (!Arguments(fields[0]).SequenceEqual(Arguments(translated))) throw new Exception("Placeholder mismatch: " + fields[0]);
        }
        Console.WriteLine("PASS: five languages, complete main resources and creature names, format arguments, traditional conversion, independent desktop/saver persistence, invalid fallback.");
    }
}
