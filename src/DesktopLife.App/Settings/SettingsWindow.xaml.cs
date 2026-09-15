using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesktopLife.Creatures.Displays;
namespace DesktopLife.App.Settings;

public partial class SettingsWindow : Window
{
    private readonly DesktopHost _host;
    private readonly SettingsStore _store;
    public SettingsWindow(DesktopHost host, SettingsStore store, string? warning = null)
    {
        _host = host;
        _store = store;
        InitializeComponent();
        FlyCount.Text = host.Simulation.TotalFlyCount.ToString(CultureInfo.InvariantCulture);
        RoachCount.Text = host.Simulation.TotalCockroachCount.ToString(CultureInfo.InvariantCulture);
        FlySlider.Value = host.Simulation.TotalFlyCount;
        RoachSlider.Value = host.Simulation.TotalCockroachCount;
        FlyCount.LostFocus += (_, _) => SyncSlider(FlyCount, FlySlider);
        RoachCount.LostFocus += (_, _) => SyncSlider(RoachCount, RoachSlider);
        _host.LayoutChanged += RefreshLayout;
        _host.StateChanged += RefreshState;
        Closed += (_, _) => { _host.LayoutChanged -= RefreshLayout; _host.StateChanged -= RefreshState; };
        Loaded += (_, _) => RefreshLayout();
        RefreshState();
        if (warning != null) Status.Text = warning;
    }

    private static void SyncSlider(TextBox input, Slider slider)
    {
        if (int.TryParse(input.Text, out var count) && count >= slider.Minimum && count <= slider.Maximum) slider.Value = count;
    }
    private void FlySliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (FlyCount != null) FlyCount.Text = ((int)e.NewValue).ToString(CultureInfo.InvariantCulture);
    }
    private void RoachSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RoachCount != null) RoachCount.Text = ((int)e.NewValue).ToString(CultureInfo.InvariantCulture);
    }
    private void ApplyClicked(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(FlyCount.Text, out var flies) || flies < 0 || flies > PopulationSettings.MaxFlies ||
            !int.TryParse(RoachCount.Text, out var roaches) || roaches < 0 || roaches > PopulationSettings.MaxCockroaches)
        {
            Status.Text = "请输入有效整数：苍蝇 0–20，蟑螂 0–500。";
            return;
        }
        try
        {
            _store.Save(new(flies, roaches));
            _host.SetPopulation(flies, roaches);
            FlySlider.Value = flies;
            RoachSlider.Value = roaches;
            Status.Text = $"已保存：全桌面 {flies} 只苍蝇、{roaches} 只蟑螂。下次启动自动恢复。";
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Status.Text = "保存失败，数量未更改。请检查用户配置目录是否可写。";
        }
    }
    private void PauseClicked(object sender, RoutedEventArgs e) => _host.TogglePause();
    private void RefreshState() => PauseButton.Content = _host.IsPaused ? "恢复全部" : "暂停全部";
    private void MapSizeChanged(object sender, SizeChangedEventArgs e) => RefreshLayout();

    private void RefreshLayout()
    {
        if (DisplayMap == null) return;
        DisplaySummary.Text = $"{_host.Simulation.Worlds.Count} 块屏幕";
        DisplayMap.Children.Clear();
        var layout = _host.Simulation.Layout;
        if (layout.Displays.Count == 0) return;
        var width = Math.Max(1, DisplayMap.ActualWidth);
        var height = DisplayMap.Height;
        var scale = Math.Min((width - 12) / layout.Bounds.Width, (height - 12) / layout.Bounds.Height);
        if (scale <= 0) return;
        var offsetX = (width - layout.Bounds.Width * scale) / 2;
        var offsetY = (height - layout.Bounds.Height * scale) / 2;
        for (var i = 0; i < layout.Displays.Count; i++)
        {
            var display = layout.Displays[i];
            var tile = new Border
            {
                Width = display.Bounds.Width * scale, Height = display.Bounds.Height * scale,
                Background = new SolidColorBrush(display.IsPrimary ? Color.FromRgb(225, 238, 254) : Color.FromRgb(241, 245, 249)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(87, 127, 167)), BorderThickness = new Thickness(1.5),
                ToolTip = $"{display.Id}\n{display.Bounds.Width} × {display.Bounds.Height}\n({display.Bounds.Left}, {display.Bounds.Top})",
                Child = new TextBlock { Text = $"{i + 1}" + (display.IsPrimary ? " · 主屏" : ""), FontFamily = new FontFamily("Segoe UI"),
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 13 }
            };
            Canvas.SetLeft(tile, offsetX + (display.Bounds.Left - layout.Bounds.Left) * scale);
            Canvas.SetTop(tile, offsetY + (display.Bounds.Top - layout.Bounds.Top) * scale);
            DisplayMap.Children.Add(tile);
        }
    }
}
