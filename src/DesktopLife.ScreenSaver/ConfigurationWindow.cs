using System.Globalization;
using DesktopLife.Rendering.Localization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Automation;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using DesktopLife.Rendering;
namespace DesktopLife.ScreenSaver;

public sealed class ConfigurationWindow : Window
{
    private readonly List<Action> _translations = [];
    private string _language;
    private string Text(string zh, string en) => UiLanguage.Text(_language, zh, en);
    private void Translate(Action update) { _translations.Add(update); update(); }

    public ConfigurationWindow(SaverSettingsStore store)
    {
        var settings = store.Load(out var warning);
        _language = settings.Language ?? DesktopLanguage();
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/DesktopLife.Rendering;component/Themes/SettingsTheme.xaml", UriKind.Relative) });
        NameScope.SetNameScope(this, new NameScope());
        Translate(() => Title = Text("DesktopLife · 屏保设置", "DesktopLife · Screen saver settings"));
        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/DesktopLife.ScreenSaver;component/DesktopLife.ico"));
        Width = Math.Min(620, SystemParameters.WorkArea.Width); Height = Math.Min(800, SystemParameters.WorkArea.Height - 40);
        MinWidth = 470; MinHeight = 480; WindowStartupLocation = WindowStartupLocation.CenterScreen;
        FontFamily = new FontFamily("Microsoft YaHei UI"); FontSize = 12; UseLayoutRounding = true; Foreground = Brush("#243C33");
        var root = new DockPanel(); Content = root;
        var footer = new StackPanel { Margin = new Thickness(22, 10, 22, 18) };
        DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brush("#65776F"), MinHeight = 30, FontSize = 11, Margin = new Thickness(0, 0, 0, 10) };
        AutomationProperties.SetLiveSetting(status, AutomationLiveSetting.Polite);
        Action statusMessage = () => status.Text = warning != null ? Text("原配置无法读取，已显示默认值。", "Could not read the previous settings; defaults are shown.") : Text("保存后应用场景与两组数量，桌面宠物配置保持独立。", "Save to apply the scene and both populations. Desktop settings stay separate.");
        Translate(() => statusMessage()); footer.Children.Add(status);
        var save = new Button { Name = "SaveButton", Height = 36, MinWidth = 132, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right, Foreground = Brushes.White };
        RegisterName(save.Name, save); Translate(() => save.Content = Text("保存屏保设置", "Save screen saver")); footer.Children.Add(save);
        var panel = new StackPanel { Margin = new Thickness(22, 16, 22, 12) };
        var scroll = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }; root.Children.Add(scroll);
        TextBlock Label(string zh, string en, bool heading = false)
        {
            var label = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brush(heading ? "#243C33" : "#65776F"), FontWeight = heading ? FontWeights.SemiBold : FontWeights.Normal, Margin = new Thickness(0, 0, 0, 6) };
            Translate(() => label.Text = Text(zh, en)); return label;
        }
        Border Card(UIElement content) => new() { Style = (Style)FindResource("Card"), Child = content, Margin = new Thickness(0, 0, 0, 10) };
        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 18) }; panel.Children.Add(top);
        var language = new ComboBox { Name = "LanguagePicker", Width = 108, Height = 30, VerticalAlignment = VerticalAlignment.Center, ItemsSource = UiLanguage.Supported.Select(l => l.Name).ToArray(), SelectedIndex = UiLanguage.IndexOf(_language) };
        RegisterName(language.Name, language); DockPanel.SetDock(language, Dock.Right); top.Children.Add(language); Translate(() => AutomationProperties.SetName(language, Text("语言", "Language")));
        var brand = new StackPanel(); top.Children.Add(brand);
        brand.Children.Add(new TextBlock { Text = "DesktopLife", FontFamily = new FontFamily("Segoe UI Semibold"), FontSize = 25 }); brand.Children.Add(Label("让闲置屏幕也有生机", "Bring your idle screen to life"));
        var appearance = new Grid(); appearance.ColumnDefinitions.Add(new ColumnDefinition()); appearance.ColumnDefinitions.Add(new ColumnDefinition());
        ComboBox Picker(string name, int selected, string zh, string en, string firstZh, string firstEn, string secondZh, string secondEn, int column)
        {
            var section = new StackPanel { Margin = new Thickness(column == 0 ? 0 : 8, 0, column == 0 ? 8 : 0, 0) }; Grid.SetColumn(section, column); appearance.Children.Add(section); section.Children.Add(Label(zh, en, true));
            var combo = new ComboBox { Name = name, Height = 34 }; var first = new ComboBoxItem(); var second = new ComboBoxItem(); combo.Items.Add(first); combo.Items.Add(second); combo.SelectedIndex = selected;
            Translate(() => { first.Content = Text(firstZh, firstEn); second.Content = Text(secondZh, secondEn); AutomationProperties.SetName(combo, Text(zh, en)); });
            RegisterName(name, combo); section.Children.Add(combo); return combo;
        }
        var theme = Picker("ThemePicker", settings.Light ? 1 : 0, "屏保背景", "Background", "深色", "Dark", "浅色", "Light", 0);
        var style = Picker("StylePicker", (int)settings.Style, "生物风格", "Creature style", "写实", "Realistic", "可爱", "Cute", 1); panel.Children.Add(Card(appearance));
        string? backgroundPath = settings.BackgroundImage;
        var imagePanel = new StackPanel();
        imagePanel.Children.Add(Label("背景图片", "Background image", true));
        var imagePreview = new Image { Height = 90, Stretch = Stretch.UniformToFill, Margin = new Thickness(0, 0, 0, 8) };
        imagePanel.Children.Add(imagePreview);
        var imageName = Label("", ""); imageName.TextWrapping = TextWrapping.NoWrap; imageName.TextTrimming = TextTrimming.CharacterEllipsis; imagePanel.Children.Add(imageName);
        void UpdateImage()
        {
            imagePreview.Source = SaverBackground.Load(backgroundPath);
            imagePreview.Visibility = imagePreview.Source == null ? Visibility.Collapsed : Visibility.Visible;
            imageName.Text = string.IsNullOrWhiteSpace(backgroundPath) ? Text("使用纯色背景", "Using a solid background") : imagePreview.Source == null ? Text("图片不可用，屏保将使用纯色背景。", "Image unavailable; the screen saver will use a solid background.") : Path.GetFileName(backgroundPath);
        }
        Translate(UpdateImage);
        var imageActions = new WrapPanel(); imagePanel.Children.Add(imageActions);
        var chooseImage = new Button { Name = "ChooseBackgroundButton", Margin = new Thickness(0, 0, 8, 0) };
        var clearImage = new Button { Name = "ClearBackgroundButton" };
        RegisterName(chooseImage.Name, chooseImage); RegisterName(clearImage.Name, clearImage);
        Translate(() => { chooseImage.Content = Text("选择图片…", "Choose image…"); clearImage.Content = Text("恢复纯色", "Use solid color"); });
        imageActions.Children.Add(chooseImage); imageActions.Children.Add(clearImage);
        imagePanel.Children.Add(Label("每屏等比填充，超出部分裁切；保存时复制图片，移动原图不会影响屏保。", "Fills each screen without distortion, cropping the edges. Saving keeps a copy of the image."));
        chooseImage.Click += (_, _) =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = Text("图片", "Images") + "|*.png;*.jpg;*.jpeg;*.bmp", CheckFileExists = true };
            if (dialog.ShowDialog(this) != true) return;
            if (SaverBackground.Load(dialog.FileName) == null)
            { status.Foreground = Brushes.Firebrick; statusMessage = () => status.Text = Text("无法读取图片，请选择有效的 PNG、JPG 或 BMP。", "Cannot read the image. Choose a valid PNG, JPG or BMP."); statusMessage(); return; }
            backgroundPath = dialog.FileName; UpdateImage();
        };
        clearImage.Click += (_, _) => { backgroundPath = null; UpdateImage(); };
        panel.Children.Add(Card(imagePanel));
        var tabs = new TabControl { Name = "HabitatTabs", Padding = new Thickness(0, 12, 0, 0) }; RegisterName(tabs.Name, tabs); panel.Children.Add(tabs);
        var rows = new List<(CreatureKind Kind, int Limit, TextBox Count, TextBox Min, TextBox Max)>();
        var forest = new StackPanel(); var ocean = new StackPanel();
        var forestTab = new TabItem { Content = forest, Background = Brush("#E6EEE9"), Foreground = Brush("#306951") }; var oceanTab = new TabItem { Content = ocean, Background = Brush("#E1F0F6"), Foreground = Brush("#1C647D") };
        Translate(() => { forestTab.Header = Text("森林", "Forest"); oceanTab.Header = Text("海洋", "Ocean"); }); tabs.Items.Add(forestTab); tabs.Items.Add(oceanTab); tabs.SelectedIndex = (int)settings.Habitat;
        var rainContent = new StackPanel();
        rainContent.Children.Add(Label("屏幕上的雨窗", "Rain on your screen", true));
        rainContent.Children.Add(Label("雨滴会积聚、滑落并吞并沿途雨滴。屏保中自动演出；真实鼠标或键盘输入仍会退出。桌面雨窗模式支持鼠标拨动和点击碎裂。", "Drops gather, slide and merge. The screen saver plays automatically; real mouse or keyboard input exits. Desktop rain supports pointer interaction and click fractures."));
        rainContent.Children.Add(Label("雨量", "Rain intensity"));
        var rainLevel = new ComboBox { SelectedIndex = settings.RainLevel - 1, Height = 34, Margin = new Thickness(0, 6, 0, 8) };
        RegisterName("RainLevelPicker", rainLevel);
        var rainNames = new[] { ("毛毛雨", "Drizzle"), ("小雨", "Light rain"), ("中雨", "Moderate rain"), ("大雨", "Heavy rain"), ("暴雨", "Downpour") };
        foreach (var names in rainNames)
        {
            var item = new ComboBoxItem();
            Translate(() => item.Content = Text(names.Item1, names.Item2));
            rainLevel.Items.Add(item);
        }
        rainLevel.SelectedIndex = settings.RainLevel - 1;
        rainContent.Children.Add(rainLevel);
        var rainTab = new TabItem { Content = Card(rainContent), Background = Brush("#E2ECF4"), Foreground = Brush("#42657D") };
        Translate(() => rainTab.Header = Text("雨窗", "Rain window")); tabs.Items.Add(rainTab); tabs.SelectedIndex = (int)settings.Habitat;
        void Theme() { var sea = tabs.SelectedIndex == 1; Background = Brush(sea ? "#EFF5F8" : "#F0F4F1"); save.Background = save.BorderBrush = Brush(sea ? "#1C647D" : "#306951"); }
        tabs.SelectionChanged += (_, e) => { if (e.Source == tabs) Theme(); }; Theme();
        Grid Row(StackPanel table)
        {
            var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) }; grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (var i = 0; i < 3; i++) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(66) }); table.Children.Add(grid); return grid;
        }
        StackPanel SpeciesTable(StackPanel destination, bool sea)
        {
            var companion = new StackPanel(); companion.Children.Add(Label(sea ? "绿海龟 · 固定 1 只" : "苍蝇 · 固定 1 只", sea ? "Green turtle · always one" : "Fly · always one", true)); companion.Children.Add(Label("屏保中自动活动，移动鼠标或按键即可退出。", "Roams automatically. Move the mouse or press a key to exit.")); destination.Children.Add(Card(companion));
            var table = new StackPanel(); destination.Children.Add(Card(table));
            table.Children.Add(Label(sea ? "海洋生物 · 12 种鱼" : "生物配置 · 12 种", sea ? "Ocean creatures · 12 species" : "Creatures · 12 species", true));
            table.Children.Add(Label(sea ? "数量 0–100，0 为关闭；尺寸 10–300%。" : "数量设为 0 可关闭。蟑螂、蚂蚁上限 500，其余 100；尺寸 10–300%。", sea ? "Counts 0–100; 0 disables a species. Sizes: 10–300%." : "0 disables a species. Roaches and ants: up to 500; others: 100. Sizes: 10–300%."));
            var header = Row(table); var zh = new[] { "数量", "最小 %", "最大 %" }; var en = new[] { "Count", "Min %", "Max %" };
            for (var i = 0; i < 3; i++) { var label = Label(zh[i], en[i]); label.TextAlignment = TextAlignment.Center; Grid.SetColumn(label, i + 1); header.Children.Add(label); } return table;
        }
        void AddRow(StackPanel table, CreatureKind kind, string zh, string en, int limit, SpeciesPopulation value)
        {
            var grid = Row(table); var label = Label(zh, en); label.Foreground = Foreground; label.Margin = new Thickness(0, 0, 6, 0); label.VerticalAlignment = VerticalAlignment.Center; grid.Children.Add(label);
            TextBox Field(int number, int column, string suffix)
            {
                var box = new TextBox { Name = kind + suffix, Text = number.ToString(CultureInfo.InvariantCulture), MaxLength = 3, MinHeight = 29, FontSize = 13, Margin = new Thickness(4, 0, 0, 0) };
                Translate(() => AutomationProperties.SetName(box, Text(zh, en) + " · " + Text(new[] { "数量", "最小 %", "最大 %" }[column - 1], new[] { "Count", "Min %", "Max %" }[column - 1])));
                RegisterName(box.Name, box); AutomationProperties.SetAutomationId(box, box.Name); Grid.SetColumn(box, column); grid.Children.Add(box); return box;
            }
            var count = Field(value.Count, 1, "Count"); count.ToolTip = $"0–{limit}"; rows.Add((kind, limit, count, Field(value.MinPercent, 2, "Min"), Field(value.MaxPercent, 3, "Max")));
        }
        var forestTable = SpeciesTable(forest, false);
        AddRow(forestTable, CreatureKind.Cockroach, "蟑螂", "Cockroach", 500, new(settings.Cockroaches, settings.RoachMin, settings.RoachMax));
        AddRow(forestTable, CreatureKind.Ant, "蚂蚁", "Ant", 500, new(settings.Ants, settings.AntMin, settings.AntMax));
        AddRow(forestTable, CreatureKind.Caterpillar, "毛毛虫", "Caterpillar", 100, new(settings.Caterpillars, settings.CaterpillarMin, settings.CaterpillarMax));
        foreach (var d in InsectCatalog.Additional) AddRow(forestTable, d.Kind, d.ChineseName, d.EnglishName, d.MaxCount, settings.Population.GetAdditional(d.Kind));
        var oceanTable = SpeciesTable(ocean, true);
        foreach (var d in OceanCatalog.Fish) AddRow(oceanTable, d.Kind, d.ChineseName, d.EnglishName, d.MaxCount, settings.Population.GetOcean(d.Kind));
        panel.Children.Add(Label("自动启动的等待时间与恢复登录选项，请在 Windows 屏保设置中调整。", "Choose the automatic activation delay and sign-in option in Windows screen saver settings."));
        language.SelectionChanged += (_, _) => { _language = UiLanguage.Supported[language.SelectedIndex].Code; foreach (var update in _translations) update(); };
        void Error(TextBox field, CreatureKind kind, string zh, string en, params object[] values)
        {
            tabs.SelectedIndex = OceanCatalog.IsOcean(kind) ? 1 : 0; status.Foreground = Brushes.Firebrick; statusMessage = () => status.Text = string.Format(CultureInfo.InvariantCulture, Text(zh, en), values); statusMessage(); field.BringIntoView(); field.Focus(); field.SelectAll();
        }
        save.Click += (_, _) =>
        {
            var values = new Dictionary<CreatureKind, SpeciesPopulation>();
            foreach (var row in rows)
            {
                if (!int.TryParse(row.Count.Text, out var count) || count < 0 || count > row.Limit)
                { Error(row.Count, row.Kind, "数量请输入 0–{0} 的整数。", "Enter a whole count from 0 to {0}.", row.Limit); return; }
                if (!int.TryParse(row.Min.Text, out var min) || !int.TryParse(row.Max.Text, out var max) || min < 10 || max > 300 || min > max)
                { Error(row.Min, row.Kind, "尺寸范围 10–300%，最小值不能大于最大值。", "Sizes must be 10–300%, with minimum no greater than maximum."); return; }
                values.Add(row.Kind, new(count, min, max));
            }
            var r = values[CreatureKind.Cockroach]; var a = values[CreatureKind.Ant]; var c = values[CreatureKind.Caterpillar];
            try
            {
                store.Save(new(theme.SelectedIndex == 1, r.Count, a.Count, c.Count, (CreatureStyle)style.SelectedIndex, r.MinPercent, r.MaxPercent, a.MinPercent, a.MaxPercent, c.MinPercent, c.MaxPercent,
                    InsectCatalog.Additional.ToDictionary(d => d.Kind, d => values[d.Kind]), (Habitat)tabs.SelectedIndex, OceanCatalog.Fish.ToDictionary(d => d.Kind, d => values[d.Kind]), _language, SaverBackground.Import(backgroundPath, store.Path), rainLevel.SelectedIndex + 1)); Close();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            { status.Foreground = Brushes.Firebrick; statusMessage = () => status.Text = Text("无法保存，请检查配置文件夹是否可写。", "Unable to save. Check that the settings folder is writable."); statusMessage(); }
        };
    }
    private static SolidColorBrush Brush(string color) => new((Color)ColorConverter.ConvertFromString(color));
    private static string DesktopLanguage()
    {
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DesktopLife", "settings.preferences.json");
            if (File.Exists(path))
            {
                using var json = JsonDocument.Parse(File.ReadAllText(path));
                if (json.RootElement.TryGetProperty("Language", out var value) && UiLanguage.IsSupported(value.GetString())) return value.GetString()!;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException) { }
        return "zh-CN";
    }
}
