using System.Windows.Input;
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
    private readonly PreferencesController _preferences;
    private bool _ready;
    private string _statusKey = "Hint";
    public SettingsWindow(DesktopHost host, SettingsStore store, string? warning = null, PreferencesController? preferences = null)
    {
        _host = host;
        _store = store;
        _preferences = preferences ?? new PreferencesController(host, store);
        InitializeComponent();
        LanguagePicker.SelectedIndex = LanguageService.Current == "en-US" ? 1 : 0;
        StartKey.Text = _preferences.Current.StartHotkey;
        PauseKey.Text = _preferences.Current.PauseHotkey;
        LanguageService.Changed += Translate;
        Closed += (_, _) => { LanguageService.Changed -= Translate; _preferences.Hotkeys.Captured -= CapturedHotkey; _preferences.Hotkeys.IsCapturing = false; if (preferences == null) _preferences.Dispose(); };
        _preferences.Hotkeys.Captured += CapturedHotkey;
        Deactivated += (_, _) => _preferences.Hotkeys.IsCapturing = false;
        Activated += (_, _) => _preferences.Hotkeys.IsCapturing = StartKey.IsKeyboardFocusWithin || PauseKey.IsKeyboardFocusWithin;
        CaptureToggle.IsChecked = _preferences.Current.ExcludeFromCapture;
        _preferences.Capture.Track(this);
        _preferences.CaptureFailed += CaptureError;
        Closed += (_, _) => _preferences.CaptureFailed -= CaptureError;
        _ready = true;
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
        if (warning != null) SetStatus("ConfigWarning");
        if (_preferences.WarningKey != null) SetStatus(_preferences.WarningKey);
    }

    private void CaptureError() => SetStatus("CaptureFailed");
    private void CaptureChanged(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        if (_preferences.SaveCapture(CaptureToggle.IsChecked == true, out var error)) SetStatus("CaptureSaved");
        else
        {
            _ready = false; CaptureToggle.IsChecked = _preferences.Current.ExcludeFromCapture; _ready = true;
            SetStatus(error);
        }
    }
    private void SetStatus(string key) { _statusKey = key; Status.Text = LanguageService.Get(key); }
    private void Translate() { RefreshState(); RefreshLayout(); SetStatus(_statusKey); }
    private void LanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        var language = ((ComboBoxItem)LanguagePicker.SelectedItem).Tag.ToString()!;
        if (_preferences.SaveLanguage(language, out var error)) SetStatus("LanguageSaved");
        else
        {
            _ready = false; LanguagePicker.SelectedIndex = LanguageService.Current == "en-US" ? 1 : 0; _ready = true;
            SetStatus(error);
        }
    }
    private void SaveKeysClicked(object sender, RoutedEventArgs e) => SetStatus(_preferences.SaveHotkeys(StartKey.Text, PauseKey.Text, out var error) ? "KeysSaved" : error);
    private void CapturedHotkey(string text) { if (Keyboard.FocusedElement is TextBox input && (input == StartKey || input == PauseKey)) input.Text = text; }
    private void KeyFocus(object sender, KeyboardFocusChangedEventArgs e) => _preferences.Hotkeys.IsCapturing = true;
    private void KeyBlur(object sender, KeyboardFocusChangedEventArgs e) => _preferences.Hotkeys.IsCapturing = false;
    private void CaptureKey(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Tab) return;
        e.Handled = true;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift) return;
        var input = (TextBox)sender;
        if (key == Key.Back && Keyboard.Modifiers == ModifierKeys.None) { input.Text = ""; return; }
        var modifiers = Keyboard.Modifiers;
        var name = key is >= Key.D0 and <= Key.D9 ? ((int)(key - Key.D0)).ToString() : key.ToString();
        var text = (modifiers.HasFlag(ModifierKeys.Control) ? "Ctrl+" : "") + (modifiers.HasFlag(ModifierKeys.Alt) ? "Alt+" : "") + (modifiers.HasFlag(ModifierKeys.Shift) ? "Shift+" : "") + name;
        if (modifiers.HasFlag(ModifierKeys.Windows) || !Hotkey.TryParse(text, out _)) { SetStatus("InvalidHotkey"); return; }
        input.Text = text;
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
            SetStatus("InvalidCounts");
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
            SetStatus("Saved");
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            SetStatus("SaveFailed");
        }
    }
    private void PauseClicked(object sender, RoutedEventArgs e) => _host.TogglePause();
    private void RefreshState() => PauseButton.Content = LanguageService.Get(_host.IsPaused ? "Resume" : "Pause");
    private void MapSizeChanged(object sender, SizeChangedEventArgs e) => RefreshLayout();

    private void RefreshLayout()
    {
        if (DisplayMap == null) return;
        DisplaySummary.Text = LanguageService.Choose($"{_host.Simulation.Worlds.Count} 块屏幕", $"{_host.Simulation.Worlds.Count} displays");
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
                Child = new TextBlock { Text = $"{i + 1}" + (display.IsPrimary ? " · " + LanguageService.Get("Primary") : ""), FontFamily = new FontFamily("Segoe UI"),
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 13 }
            };
            Canvas.SetLeft(tile, offsetX + (display.Bounds.Left - layout.Bounds.Left) * scale);
            Canvas.SetTop(tile, offsetY + (display.Bounds.Top - layout.Bounds.Top) * scale);
            DisplayMap.Children.Add(tile);
        }
    }
}
