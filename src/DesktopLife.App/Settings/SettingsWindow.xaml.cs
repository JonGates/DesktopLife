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
        Height = Math.Min(880, SystemParameters.WorkArea.Height - 60);
        RoachCount.Text = host.Simulation.TotalCockroachCount.ToString(CultureInfo.InvariantCulture);
        RoachSlider.Value = host.Simulation.TotalCockroachCount;
        RoachCount.LostFocus += (_, _) => SyncSlider(RoachCount, RoachSlider);
        AntCount.Text = host.Simulation.TotalAntCount.ToString(CultureInfo.InvariantCulture);
        CaterpillarCount.Text = host.Simulation.TotalCaterpillarCount.ToString(CultureInfo.InvariantCulture);
        AntSlider.Value = host.Simulation.TotalAntCount;
        CaterpillarSlider.Value = host.Simulation.TotalCaterpillarCount;
        AntCount.LostFocus += (_, _) => SyncSlider(AntCount, AntSlider);
        CaterpillarCount.LostFocus += (_, _) => SyncSlider(CaterpillarCount, CaterpillarSlider);
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
    private void AntSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (AntCount != null) AntCount.Text = ((int)e.NewValue).ToString(CultureInfo.InvariantCulture);
    }
    private void RoachSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RoachCount != null) RoachCount.Text = ((int)e.NewValue).ToString(CultureInfo.InvariantCulture);
    }
    private void CaterpillarSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CaterpillarCount != null) CaterpillarCount.Text = ((int)e.NewValue).ToString(CultureInfo.InvariantCulture);
    }
    private void ApplyClicked(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RoachCount.Text, out var roaches) || roaches < 0 || roaches > PopulationSettings.MaxCockroaches ||
            !int.TryParse(AntCount.Text, out var ants) || ants < 0 || ants > PopulationSettings.MaxAnts ||
            !int.TryParse(CaterpillarCount.Text, out var caterpillars) || caterpillars < 0 || caterpillars > PopulationSettings.MaxCaterpillars)
        {
            Status.Text = "请输入有效整数：蟑螂、蚂蚁 0–500，毛毛虫 0–100。";
            return;
        }
        try
        {
            var settings = new PopulationSettings(roaches, ants, caterpillars);
            _store.Save(settings);
            _host.SetPopulation(settings);
            RoachSlider.Value = roaches;
            AntSlider.Value = ants;
            CaterpillarSlider.Value = caterpillars;
            Status.Text = $"已保存：蟑螂 {roaches}、蚂蚁 {ants}、毛毛虫 {caterpillars}。苍蝇固定 1 只。";
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Status.Text = "保存失败，数量未更改。请检查用户配置目录是否可写。";
        }
    }    private void PauseClicked(object sender, RoutedEventArgs e) => _host.TogglePause();
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
