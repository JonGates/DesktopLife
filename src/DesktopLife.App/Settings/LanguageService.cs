using System.Windows;
namespace DesktopLife.App.Settings;
public static class LanguageService
{
    public static string Current { get; private set; } = "zh-CN";
    public static event Action? Changed;
    private static readonly Dictionary<string, (string Zh, string En)> Texts = new()
    {
        ["CaptureToggle"] = ("从录屏中隐藏昆虫和设置窗口", "Exclude creatures and settings from screen capture"),
        ["CaptureHelp"] = ("本机仍可见。需 Windows 10 2004 或更新版本；仅对支持此机制的录屏工具有效，不能保证所有监控软件均无法录制。", "Still visible on your display. Requires Windows 10 2004 or later. Only compatible capture tools honor this setting; it cannot block every monitoring tool."),
        ["CaptureSaved"] = ("录屏显示设置已保存。", "Capture preference saved."),
        ["CaptureFailed"] = ("Windows 未能对所有窗口应用录屏排除。请勿依赖此功能隐藏内容。", "Windows could not apply capture exclusion to every window. Do not rely on it to hide content."),
        ["CaptureSection"] = ("录屏与共享", "Capture & sharing"),
        ["MinSize"] = ("最小尺寸 %", "Min size %"),
        ["MaxSize"] = ("最大尺寸 %", "Max size %"),
        ["InvalidSizes"] = ("尺寸须为 10–300% 的整数，最小值不能大于最大值。", "Sizes must be whole percentages from 10–300%; min must not exceed max."),
        ["Appearance"] = ("昆虫风格", "Creature style"),
        ["RealisticStyle"] = ("写实", "Realistic"),
        ["CuteStyle"] = ("可爱 · 圆润自然", "Cute · soft and rounded"),
        ["StyleHelp"] = ("切换后立即生效，保留数量和当前位置。", "Applies immediately, keeping populations and positions."),
        ["StyleSaved"] = ("昆虫风格已保存。", "Creature style saved."),
        ["Title"] = ("DesktopLife · 数量设置", "DesktopLife · Settings"),
        ["Subtitle"] = ("让生物在整个桌面自由活动", "Let creatures roam across your desktop"),
        ["Apply"] = ("保存数量与尺寸", "Save population & sizes"),
        ["Pause"] = ("暂停全部", "Pause all"),
        ["Resume"] = ("恢复全部", "Start / resume"),
        ["Layout"] = ("屏幕排列", "Display layout"),
        ["LayoutName"] = ("已识别的屏幕排列", "Detected display layout"),
        ["LayoutHelp"] = ("相接边缘可跨屏移动，方向跟随 Windows 显示设置。", "Creatures cross touching edges, following your Windows display layout."),
        ["Counts"] = ("全桌面总数量", "Desktop population"),
        ["CountHelp"] = ("所有屏幕共享这些生物，增加屏幕不会增加数量。", "All displays share one population. Adding a display keeps the same counts."),
        ["Fly"] = ("苍蝇 · 固定 1 只", "Fly · always one"),
        ["FlyHelp"] = ("围绕鼠标飞行 · 点击后停落 3 秒并搓足", "Orbits the cursor · lands and grooms for 3 seconds after a click"),
        ["Roach"] = ("蟑螂", "Cockroaches"),
        ["RoachHelp"] = ("爬行与躲避鼠标 · 0–500", "Crawls and avoids the cursor · 0–500"),
        ["Ant"] = ("蚂蚁", "Ants"),
        ["AntHelp"] = ("快速爬行与躲避鼠标 · 0–500", "Fast walkers that avoid the cursor · 0–500"),
        ["Caterpillar"] = ("毛毛虫", "Caterpillars"),
        ["CaterpillarHelp"] = ("缓慢蠕动 · 0–100", "Slow, rippling crawl · 0–100"),
        ["RoachCountName"] = ("蟑螂总数量", "Total cockroaches"),
        ["AntCountName"] = ("蚂蚁总数量", "Total ants"),
        ["CaterpillarCountName"] = ("毛毛虫总数量", "Total caterpillars"),
        ["RoachSliderName"] = ("蟑螂数量滑块", "Cockroach count slider"),
        ["AntSliderName"] = ("蚂蚁数量滑块", "Ant count slider"),
        ["CaterpillarSliderName"] = ("毛毛虫数量滑块", "Caterpillar count slider"),
        ["Unit"] = ("只", ""),
        ["Hint"] = ("爬行昆虫设为 0 可关闭。关闭窗口后，生物会继续运行。", "Set a crawler count to 0 to disable it. Closing this window keeps DesktopLife running."),
        ["Language"] = ("语言", "Language"),
        ["Hotkeys"] = ("全局快捷键", "Global shortcuts"),
        ["Start"] = ("启动／恢复", "Start / resume"),
        ["Stop"] = ("暂停", "Pause"),
        ["SaveKeys"] = ("保存快捷键", "Save shortcuts"),
        ["KeyHelp"] = ("点击输入框后按组合键；Backspace 清除。程序运行时全局有效，退出后不可用。", "Focus a field and press a shortcut; Backspace clears it. Shortcuts work globally while DesktopLife is running."),
        ["InvalidHotkey"] = ("请使用 Ctrl、Alt 或 Shift 加字母、数字或 F1–F11。", "Use Ctrl, Alt or Shift with a letter, digit or F1–F11."),
        ["DuplicateHotkey"] = ("启动和暂停不能使用相同快捷键。", "Start and pause must use different shortcuts."),
        ["OccupiedHotkey"] = ("快捷键已被占用，未保存更改。请换一组组合键。", "A shortcut is unavailable. Changes were not saved; choose another combination."),
        ["SaveFailed"] = ("保存失败，设置未更改。请检查配置目录是否可写。", "Could not save. Settings are unchanged; check access to the settings folder."),
        ["KeysSaved"] = ("快捷键已保存，立即生效。", "Shortcuts saved and active."),
        ["LanguageSaved"] = ("语言已保存。", "Language saved."),
        ["InvalidCounts"] = ("请输入有效整数：蟑螂、蚂蚁 0–500，毛毛虫 0–100。", "Enter whole numbers: cockroaches and ants 0–500; caterpillars 0–100."),
        ["Saved"] = ("数量已保存，下次启动自动恢复。", "Population saved for the next launch."),
        ["ConfigWarning"] = ("无法读取已保存的设置，已使用默认值。", "Saved settings could not be read. Defaults are in use."),
        ["Primary"] = ("主屏", "Primary"),
        ["Settings"] = ("设置…", "Settings…"),
        ["Exit"] = ("退出", "Exit"),
    };
    public static string Get(string key) => Current == "en-US" ? Texts[key].En : Texts[key].Zh;
    public static string Choose(string zh, string en) => Current == "en-US" ? en : zh;
    public static void Apply(string language)
    {
        Current = language == "en-US" ? "en-US" : "zh-CN";
        if (Application.Current != null)
            foreach (var key in Texts.Keys) Application.Current.Resources[key] = Get(key);
        Changed?.Invoke();
    }
}
