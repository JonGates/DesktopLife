using System.Windows.Input;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Creatures;
using System.Windows.Automation;
namespace DesktopLife.App.Settings;

public partial class SettingsWindow : Window
{
    private readonly DesktopHost _host;
    private readonly SettingsStore _store;
    private readonly PreferencesController _preferences;
    private bool _ready;
    private string _statusKey = "Hint";
    private readonly List<(InsectDefinition Definition, TextBlock Label, TextBox Count, TextBox Min, TextBox Max)> _additionalRows = [];
    public SettingsWindow(DesktopHost host, SettingsStore store, string? warning = null, PreferencesController? preferences = null)
    {
        _host = host;
        _store = store;
        _preferences = preferences ?? new PreferencesController(host, store);
        InitializeComponent();
        LanguagePicker.SelectedIndex = LanguageService.Current == "en-US" ? 1 : 0;
        StylePicker.SelectedIndex = (int)_preferences.Current.Style;
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
        var sizes = host.Simulation.Settings;
        RoachMin.Text = sizes.RoachMin.ToString(); RoachMax.Text = sizes.RoachMax.ToString();
        AntMin.Text = sizes.AntMin.ToString(); AntMax.Text = sizes.AntMax.ToString();
        CaterpillarMin.Text = sizes.CaterpillarMin.ToString(); CaterpillarMax.Text = sizes.CaterpillarMax.ToString();
        BuildAdditionalRows(sizes);
        _ready = true;
        Height = Math.Min(880, SystemParameters.WorkArea.Height - 60);
        RoachCount.Text = host.Simulation.TotalCockroachCount.ToString(CultureInfo.InvariantCulture);
        AntCount.Text = host.Simulation.TotalAntCount.ToString(CultureInfo.InvariantCulture);
        CaterpillarCount.Text = host.Simulation.TotalCaterpillarCount.ToString(CultureInfo.InvariantCulture);
        _host.LayoutChanged += RefreshLayout;
        _host.StateChanged += RefreshState;
        Closed += (_, _) => { _host.LayoutChanged -= RefreshLayout; _host.StateChanged -= RefreshState; };
        Loaded += (_, _) => RefreshLayout();
        RefreshState();
        if (warning != null) SetStatus("ConfigWarning");
        if (_preferences.WarningKey != null) SetStatus(_preferences.WarningKey);
    }

    private void StyleChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        if (_preferences.SaveStyle((DesktopLife.Rendering.CreatureStyle)StylePicker.SelectedIndex, out var error)) SetStatus("StyleSaved");
        else
        {
            _ready = false; StylePicker.SelectedIndex = (int)_preferences.Current.Style; _ready = true;
            SetStatus(error);
        }
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
    private void Translate() { RefreshState(); RefreshLayout(); TranslateAdditionalRows(); SetStatus(_statusKey); }
    private void BuildAdditionalRows(PopulationSettings settings)
    {
        Grid Row()
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            for (var i = 0; i < 3; i++) row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(66) });
            AdditionalSpeciesPanel.Children.Add(row);
            return row;
        }
        foreach (var definition in InsectCatalog.Additional)
        {
            var value = settings.GetAdditional(definition.Kind);
            var row = Row();
            var label = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 6, 0) };
            row.Children.Add(label);
            TextBox Field(int number, int column, string suffix)
            {
                var field = new TextBox { Name = definition.Kind + suffix, Text = number.ToString(CultureInfo.InvariantCulture), MaxLength = 3, Margin = new Thickness(4, 0, 0, 0), FontSize = 13, MinHeight = 29 };
                RegisterName(field.Name, field);
                AutomationProperties.SetAutomationId(field, field.Name);
                Grid.SetColumn(field, column); row.Children.Add(field); return field;
            }
            _additionalRows.Add((definition, label, Field(value.Count, 1, "Count"), Field(value.MinPercent, 2, "Min"), Field(value.MaxPercent, 3, "Max")));
        }
        TranslateAdditionalRows();
    }
    private void TranslateAdditionalRows()
    {
        foreach (var row in _additionalRows)
        {
            var name = LanguageService.Choose(row.Definition.ChineseName, row.Definition.EnglishName);
            row.Label.Text = name;
            row.Count.ToolTip = $"{name} · 0–{row.Definition.MaxCount}";
            AutomationProperties.SetName(row.Count, name + " · " + LanguageService.Get("CountColumn"));
            AutomationProperties.SetName(row.Min, name + " · " + LanguageService.Get("MinSize"));
            AutomationProperties.SetName(row.Max, name + " · " + LanguageService.Get("MaxSize"));
        }
    }
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
            if (!int.TryParse(RoachMin.Text, out var rMin) || !int.TryParse(RoachMax.Text, out var rMax) ||
                !int.TryParse(AntMin.Text, out var aMin) || !int.TryParse(AntMax.Text, out var aMax) ||
                !int.TryParse(CaterpillarMin.Text, out var cMin) || !int.TryParse(CaterpillarMax.Text, out var cMax))
            { SetStatus("InvalidSizes"); return; }
            var additional = new Dictionary<CreatureKind, SpeciesPopulation>();
            foreach (var row in _additionalRows)
            {
                if (!int.TryParse(row.Count.Text, out var count) || count < 0 || count > row.Definition.MaxCount)
                { SetStatus("InvalidCounts"); row.Count.Focus(); return; }
                if (!int.TryParse(row.Min.Text, out var min) || !int.TryParse(row.Max.Text, out var max) || min < 10 || max > 300 || min > max)
                { SetStatus("InvalidSizes"); row.Min.Focus(); return; }
                additional.Add(row.Definition.Kind, new(count, min, max));
            }
            var settings = new PopulationSettings(roaches, ants, caterpillars, rMin, rMax, aMin, aMax, cMin, cMax,
                additional.Values.All(value => value == new SpeciesPopulation()) ? null : additional);
            try { settings.Validate(); } catch (ArgumentOutOfRangeException) { SetStatus("InvalidSizes"); return; }
            _store.Save(settings);
            _host.SetPopulation(settings);
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
