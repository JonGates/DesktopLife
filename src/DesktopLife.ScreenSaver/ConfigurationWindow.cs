using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Automation;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
namespace DesktopLife.ScreenSaver;

public sealed class ConfigurationWindow : Window
{
    public ConfigurationWindow(SaverSettingsStore store)
    {
        var settings = store.Load(out var warning);
        Title = "DesktopLife · 屏保设置 / Screen saver settings";
        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/DesktopLife.ScreenSaver;component/DesktopLife.ico"));
        Width = Math.Min(540, SystemParameters.WorkArea.Width);
        Height = Math.Min(760, SystemParameters.WorkArea.Height);
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(242, 246, 250));
        var root = new DockPanel { Margin = new Thickness(16) }; Content = root;
        var panel = new StackPanel();
        void Label(string text, double size = 12) => panel.Children.Add(new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) });
        Label("DesktopLife 屏幕保护程序", 21);
        Label("Screen saver · 所有屏幕共享数量 / One population across all monitors");
        Label("背景 / Background");
        var theme = new ComboBox { Margin = new Thickness(0, 0, 0, 10), Height = 32, ItemsSource = new[] { "深色 / Dark", "浅色 / Light" }, SelectedIndex = settings.Light ? 1 : 0 };
        panel.Children.Add(theme);
        Label("生物风格 / Creature style");
        var style = new ComboBox { Margin = new Thickness(0, 0, 0, 10), Height = 32, ItemsSource = new[] { "写实 / Realistic", "可爱 / Cute" }, SelectedIndex = (int)settings.Style };
        panel.Children.Add(style);
        TextBox Count(string label, int value)
        {
            Label(label);
            var box = new TextBox { Text = value.ToString(CultureInfo.InvariantCulture), Height = 26, Padding = new Thickness(6, 3, 6, 3), Margin = new Thickness(0, 0, 0, 6) };
            panel.Children.Add(box); return box;
        }
        (TextBox Min, TextBox Max) SizeRange(int min, int max)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            TextBox Field(string label, int value)
            {
                row.Children.Add(new TextBlock { Text = label, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
                var box = new TextBox { Text = value.ToString(), Width = 48, FontSize = 12, Padding = new Thickness(4, 2, 4, 2), Margin = new Thickness(5, 0, 10, 0) };
                row.Children.Add(box); return box;
            }
            var low = Field("最小 / Min %", min); var high = Field("最大 / Max %", max);
            panel.Children.Add(row); return (low, high);
        }
        var sharedPanel = panel;
        var tabs = new TabControl { Name = "HabitatTabs", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Padding = new Thickness(0, 10, 0, 0) };
        NameScope.SetNameScope(this, new NameScope()); RegisterName(tabs.Name, tabs);
        sharedPanel.Children.Add(tabs);
        var forestPanel = new StackPanel(); var oceanPanel = new StackPanel();
        tabs.Items.Add(new TabItem { Header = "森林 / Forest", Content = forestPanel, Padding = new Thickness(22, 8, 22, 8), Background = new SolidColorBrush(Color.FromRgb(231, 239, 233)), Foreground = new SolidColorBrush(Color.FromRgb(48, 105, 81)) });
        tabs.Items.Add(new TabItem { Header = "海洋 / Ocean", Content = oceanPanel, Padding = new Thickness(22, 8, 22, 8), Background = new SolidColorBrush(Color.FromRgb(225, 240, 246)), Foreground = new SolidColorBrush(Color.FromRgb(28, 100, 125)) });
        tabs.SelectedIndex = (int)settings.Habitat;
        panel = forestPanel;
        Label("苍蝇 · 固定 1 只 / Fly · always one");
        var roaches = Count("蟑螂 / Cockroaches (0–500)", settings.Cockroaches);
        var roachSize = SizeRange(settings.RoachMin, settings.RoachMax);
        var ants = Count("蚂蚁 / Ants (0–500)", settings.Ants);
        var antSize = SizeRange(settings.AntMin, settings.AntMax);
        var caterpillars = Count("毛毛虫 / Caterpillars (0–100)", settings.Caterpillars);
        var caterpillarSize = SizeRange(settings.CaterpillarMin, settings.CaterpillarMax);
        Label("更多生物 / More creatures", 16);
        Label("每种 0–100 只，尺寸 10–300%；数量为 0 时关闭。\n0–100 per species, size 10–300%; a count of 0 disables it.\n所有生物支持写实和可爱风格。 / All creatures support realistic and cute styles.");
        var additionalRows = new List<(InsectDefinition Definition, TextBox Count, TextBox Min, TextBox Max)>();
        Grid AdditionalRow()
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            for (var i = 0; i < 3; i++) row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });
            panel.Children.Add(row); return row;
        }
        void Header()
        {
        var header = AdditionalRow();
        var columns = new[] { "数量\nCount", "最小 %\nMin %", "最大 %\nMax %" };
        for (var i = 0; i < columns.Length; i++)
        {
            var text = new TextBlock { Text = columns[i], TextAlignment = TextAlignment.Center, FontSize = 11 };
            Grid.SetColumn(text, i + 1); header.Children.Add(text);
        }
        }
        Header();
        void SpeciesRows(IEnumerable<InsectDefinition> definitions, bool ocean)
        {
        foreach (var definition in definitions)
        {
            var value = ocean ? settings.Population.GetOcean(definition.Kind) : settings.Population.GetAdditional(definition.Kind);
            var row = AdditionalRow();
            var name = definition.ChineseName + " / " + definition.EnglishName;
            row.Children.Add(new TextBlock { Text = name, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
            TextBox Field(int number, int column, string suffix)
            {
                var box = new TextBox { Name = definition.Kind + suffix, Text = number.ToString(CultureInfo.InvariantCulture), MaxLength = 3, MinHeight = 29, Padding = new Thickness(5, 3, 5, 3), Margin = new Thickness(4, 0, 0, 0) };
                AutomationProperties.SetName(box, name + " " + new[] { "数量 Count", "最小 Min %", "最大 Max %" }[column - 1]);
                RegisterName(box.Name, box);
                AutomationProperties.SetAutomationId(box, box.Name);
                Grid.SetColumn(box, column); row.Children.Add(box); return box;
            }
            var count = Field(value.Count, 1, "Count"); count.ToolTip = $"0–{definition.MaxCount}";
            additionalRows.Add((definition, count, Field(value.MinPercent, 2, "Min"), Field(value.MaxPercent, 3, "Max")));
        }
        }
        SpeciesRows(InsectCatalog.Additional, false);
        panel = oceanPanel;
        Label("绿海龟 · 固定 1 只 / Green turtle · always one", 16);
        Label("海龟和鱼在所有屏幕间悠游。 / The turtle and fish swim across all displays.");
        Label("海洋生物 · 12 种鱼 / Ocean · 12 fish species", 16);
        Label("每种 0–100 条；0 为关闭。尺寸 10–300%。\n0–100 per species; 0 disables it. Sizes: 10–300%.");
        Header(); SpeciesRows(OceanCatalog.Fish, true);
        panel = sharedPanel;
        Label("场景和两组数量在保存后生效。 / Save to apply the scene and both populations.");
        Label("森林固定 1 只苍蝇，海洋固定 1 只绿海龟，自由活动。移动鼠标或按键退出屏保。\nOne fly in Forest or one green turtle in Ocean roams automatically. Move the mouse or press a key to exit.");
        Label("等待时间和恢复登录由 Windows 屏幕保护程序设置管理。\nChoose the idle timeout and sign-in option in Windows settings.");
        var footer = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var status = new TextBlock { Text = warning, Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) }; footer.Children.Add(status);
        var save = new Button { Content = "保存 / Save", Height = 36, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right, Width = 120 }; footer.Children.Add(save);
        root.Children.Add(new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        save.Click += (_, _) =>
        {
            if (!int.TryParse(roaches.Text, out var r) || r is < 0 or > 500 || !int.TryParse(ants.Text, out var a) || a is < 0 or > 500 || !int.TryParse(caterpillars.Text, out var c) || c is < 0 or > 100)
            { tabs.SelectedIndex = 0; status.Text = "请输入范围内的整数。 / Enter whole numbers within the ranges."; return; }
            if (!int.TryParse(roachSize.Min.Text, out var rMin) || !int.TryParse(roachSize.Max.Text, out var rMax) ||
                !int.TryParse(antSize.Min.Text, out var aMin) || !int.TryParse(antSize.Max.Text, out var aMax) ||
                !int.TryParse(caterpillarSize.Min.Text, out var cMin) || !int.TryParse(caterpillarSize.Max.Text, out var cMax))
            { tabs.SelectedIndex = 0; status.Text = "尺寸请输入整数 / Enter whole size percentages."; return; }
            var additional = new Dictionary<CreatureKind, SpeciesPopulation>();
            var ocean = new Dictionary<CreatureKind, SpeciesPopulation>();
            foreach (var row in additionalRows)
            {
                if (!int.TryParse(row.Count.Text, out var count) || count < 0 || count > row.Definition.MaxCount)
                { tabs.SelectedIndex = OceanCatalog.IsOcean(row.Definition.Kind) ? 1 : 0; status.Text = $"{row.Definition.ChineseName} / {row.Definition.EnglishName}: 请输入 0–{row.Definition.MaxCount} 的整数 / Enter a whole count in range."; row.Count.Focus(); return; }
                if (!int.TryParse(row.Min.Text, out var min) || !int.TryParse(row.Max.Text, out var max) || min < 10 || max > 300 || min > max)
                { tabs.SelectedIndex = OceanCatalog.IsOcean(row.Definition.Kind) ? 1 : 0; status.Text = "尺寸范围 10–300%，最小值 ≤ 最大值。 / Size 10–300%, min ≤ max."; row.Min.Focus(); return; }
                (OceanCatalog.IsOcean(row.Definition.Kind) ? ocean : additional).Add(row.Definition.Kind, new(count, min, max));
            }
            try { store.Save(new(theme.SelectedIndex == 1, r, a, c, (DesktopLife.Rendering.CreatureStyle)style.SelectedIndex, rMin, rMax, aMin, aMax, cMin, cMax, additional, (Habitat)tabs.SelectedIndex, ocean)); Close(); }
            catch (ArgumentOutOfRangeException) { tabs.SelectedIndex = 0; status.Text = "尺寸范围 10–300%，最小值 ≤ 最大值。 / Size 10–300%, min ≤ max."; }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { status.Text = "保存失败。 / Could not save: " + e.Message; }
        };
    }
}
