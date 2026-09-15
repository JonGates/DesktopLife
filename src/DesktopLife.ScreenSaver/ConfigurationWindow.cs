using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        var root = new DockPanel { Margin = new Thickness(22) }; Content = root;
        var panel = new StackPanel();
        void Label(string text, double size = 14) => panel.Children.Add(new TextBlock { Text = text, FontSize = size, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) });
        Label("DesktopLife 屏幕保护程序", 25);
        Label("Screen saver · 所有屏幕共享数量 / One population across all monitors");
        Label("背景 / Background");
        var theme = new ComboBox { Margin = new Thickness(0, 0, 0, 18), Height = 32, ItemsSource = new[] { "深色 / Dark", "浅色 / Light" }, SelectedIndex = settings.Light ? 1 : 0 };
        panel.Children.Add(theme);
        TextBox Count(string label, int value)
        {
            Label(label);
            var box = new TextBox { Text = value.ToString(CultureInfo.InvariantCulture), Height = 30, Padding = new Thickness(6, 3, 6, 3), Margin = new Thickness(0, 0, 0, 13) };
            panel.Children.Add(box); return box;
        }
        var roaches = Count("蟑螂 / Cockroaches (0–500)", settings.Cockroaches);
        var ants = Count("蚂蚁 / Ants (0–500)", settings.Ants);
        var caterpillars = Count("毛毛虫 / Caterpillars (0–100)", settings.Caterpillars);
        Label("苍蝇固定 1 只，自动飞行与停落。移动鼠标或按键退出屏保。\nOne fly roams automatically. Move the mouse or press a key to exit.");
        Label("等待时间和恢复登录由 Windows 屏幕保护程序设置管理。\nChoose the idle timeout and sign-in option in Windows settings.");
        var footer = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var status = new TextBlock { Text = warning, Foreground = Brushes.Firebrick, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) }; footer.Children.Add(status);
        var save = new Button { Content = "保存 / Save", Height = 36, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right, Width = 120 }; footer.Children.Add(save);
        root.Children.Add(new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        save.Click += (_, _) =>
        {
            if (!int.TryParse(roaches.Text, out var r) || r is < 0 or > 500 || !int.TryParse(ants.Text, out var a) || a is < 0 or > 500 || !int.TryParse(caterpillars.Text, out var c) || c is < 0 or > 100)
            { status.Text = "请输入范围内的整数。 / Enter whole numbers within the ranges."; return; }
            try { store.Save(new(theme.SelectedIndex == 1, r, a, c)); Close(); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { status.Text = "保存失败。 / Could not save: " + e.Message; }
        };
    }
}
