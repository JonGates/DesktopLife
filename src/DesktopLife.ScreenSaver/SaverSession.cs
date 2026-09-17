using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Input;
using DesktopLife.Engine.ScreenSaving;
using DesktopLife.Engine.World;
using DesktopLife.Windows;
using Microsoft.Win32;
namespace DesktopLife.ScreenSaver;

public sealed class SaverSession : IDisposable
{
    private readonly Application _app;
    private readonly nint _parent;
    private readonly List<SaverWindow> _windows = [];
    private readonly List<SaverSurface> _surfaces = [];
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _clock = new();
    private readonly ScreenSaverInputGuard _input = new();
    private HwndSource? _preview;
    private bool _disposed;
    private double _previous;
    private long _lastLanding;
    private Vector2 _autoCursor;
    private Vector2 _target;
    private double _nextTarget;
    private readonly Random _random = new();
    public DisplaySimulation Simulation { get; } = new(Environment.TickCount);
    public nint PreviewHandle => _preview?.Handle ?? 0;

    public SaverSession(Application app, SaverSettings settings, nint previewParent = 0, Func<IReadOnlyList<DisplayArea>>? getDisplays = null)
    {
        _app = app; _parent = previewParent;
        _timer = new DispatcherTimer(DispatcherPriority.Render, app.Dispatcher) { Interval = TimeSpan.FromMilliseconds(1000.0 / 30) };
        _timer.Tick += Tick;
        var displays = previewParent == 0 ? (getDisplays ?? MonitorService.GetDisplays)() : [new DisplayArea("preview", new(0, 0, 960, 540), true)];
        if (displays.Count == 0) throw new InvalidOperationException("No displays available");
        Simulation.Synchronize(displays);
        Simulation.SetPopulation(settings.Population);
        var backgroundImage = SaverBackground.Load(settings.BackgroundImage);
        _autoCursor = _target = displays[0].Bounds.Center;
        try
        {
            if (previewParent == 0)
            {
                foreach (var display in displays)
                {
                    var surface = new SaverSurface(Simulation, display.Bounds, settings.Light, style: settings.Style, backgroundImage: backgroundImage);
                    var window = new SaverWindow(display.Bounds, surface);
                    window.Closed += WindowClosed;
                    _windows.Add(window); _surfaces.Add(surface); window.Show();
                }
                _windows[0].Activate();
                SystemEvents.DisplaySettingsChanged += DisplaysChanged;
                SystemEvents.SessionSwitch += SessionChanged;
            }
            else
            {
                if (!SaverNative.IsWindow(previewParent)) throw new ArgumentException("Invalid preview parent");
                var context = SaverNative.SetThreadDpiAwarenessContext(SaverNative.GetWindowDpiAwarenessContext(previewParent));
                try
                {
                    _preview = new HwndSource(new HwndSourceParameters("DesktopLife Screen Saver Preview")
                    { ParentWindow = previewParent, WindowStyle = 0x40000000 | 0x10000000, Width = 1, Height = 1 });
                }
                finally { if (context != 0) SaverNative.SetThreadDpiAwarenessContext(context); }
                var surface = new SaverSurface(Simulation, displays[0].Bounds, settings.Light, preview: true, style: settings.Style, backgroundImage: backgroundImage);
                _surfaces.Add(surface); _preview.RootVisual = surface;
                ResizePreview();
            }
            _clock.Start(); _timer.Start();
        }
        catch { Dispose(); throw; }
    }

    private void Tick(object? sender, EventArgs e)
    {
        if (_disposed) return;
        var elapsed = _clock.Elapsed.TotalSeconds;
        var frameElapsed = elapsed - _previous;
        var delta = Math.Min(frameElapsed, .05); _previous = elapsed;
        if (_parent != 0)
        {
            if (!SaverNative.IsWindow(_parent) || _preview == null || !SaverNative.IsWindow(_preview.Handle)) { _app.Shutdown(); return; }
            ResizePreview();
        }
        else
        {
            var input = new SaverNative.LastInput { Size = (uint)Marshal.SizeOf<SaverNative.LastInput>() };
            if (!SaverNative.GetCursorPos(out var cursor) || !SaverNative.GetLastInputInfo(ref input) || _input.ShouldExit(elapsed, new(cursor.X, cursor.Y), input.Time))
            { _app.Shutdown(); return; }
        }
        if (elapsed >= _nextTarget)
        {
            var display = Simulation.Layout.Displays[_random.Next(Simulation.Layout.Displays.Count)].Bounds;
            _target = new(display.Left + display.Width * (.15f + _random.NextSingle() * .7f), display.Top + display.Height * (.15f + _random.NextSingle() * .7f));
            _nextTarget = elapsed + 5;
        }
        var direction = _target - _autoCursor;
        if (direction.LengthSquared() > 1) _autoCursor += Vector2.Normalize(direction) * Math.Min(direction.Length(), 100 * (float)delta);
        // A virtual target keeps the fly active while the real mouse remains idle.
        var target = Simulation.Layout.Clamp(_autoCursor);
        var cycle = (long)(elapsed / 16);
        MouseClick? click = cycle > _lastLanding ? new MouseClick(cycle, target) : null;
        _lastLanding = cycle;
        Simulation.Update((float)frameElapsed, target, click);
        foreach (var surface in _surfaces) surface.InvalidateVisual();
    }
    private void ResizePreview()
    {
        if (_preview == null || !SaverNative.GetClientRect(_parent, out var rect)) return;
        var width = Math.Max(1, rect.Right - rect.Left); var height = Math.Max(1, rect.Bottom - rect.Top);
        if (!SaverNative.SetWindowPos(_preview.Handle, 0, 0, 0, width, height, 0x0014)) throw new InvalidOperationException("Could not resize preview");
        var transform = _preview.CompositionTarget?.TransformFromDevice ?? System.Windows.Media.Matrix.Identity;
        var size = transform.Transform(new Point(width, height));
        var surface = _surfaces[0]; surface.Measure(new Size(size.X, size.Y)); surface.Arrange(new Rect(0, 0, size.X, size.Y));
    }
    private void WindowClosed(object? sender, EventArgs args) { if (!_disposed) _app.Shutdown(); }
    private void DisplaysChanged(object? sender, EventArgs args) => _app.Dispatcher.BeginInvoke(new Action(() => _app.Shutdown()));
    private void SessionChanged(object sender, SessionSwitchEventArgs args) => _app.Dispatcher.BeginInvoke(new Action(() => _app.Shutdown()));
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true; _timer.Stop(); _timer.Tick -= Tick; _clock.Stop();
        SystemEvents.DisplaySettingsChanged -= DisplaysChanged; SystemEvents.SessionSwitch -= SessionChanged;
        _preview?.Dispose(); _preview = null;
        foreach (var window in _windows) { window.Closed -= WindowClosed; window.Close(); }
        _windows.Clear(); _surfaces.Clear();
    }
}
