using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;
using DesktopLife.App.Overlay;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.Time;
using DesktopLife.Engine.World;
using DesktopLife.Engine.Input;
using DesktopLife.Windows;
using Microsoft.Win32;

namespace DesktopLife.App;

/// <summary>Owns all screen windows and a single rendering/input loop on the UI dispatcher.</summary>
public sealed class DesktopHost : IDisposable
{
    private readonly Dispatcher _dispatcher;
    private readonly Func<IReadOnlyList<DisplayArea>> _getDisplays;
    private readonly bool _observeMouseClicks;
    private readonly Stopwatch _clock = new();
    private readonly GameLoop _loop = new();
    private bool _subscribed;
    private bool _started;
    private bool _disposed;
    private bool _reconciling;
    private int _refreshQueued;
    private TimeSpan? _lastRenderingTime;
    private GlobalMouseClickSource? _mouseClicks;
    public MouseClickBuffer Clicks { get; } = new();
    public bool IsClickHookInstalled => _mouseClicks?.IsInstalled == true;
    public DisplaySimulation Simulation { get; } = new(Environment.TickCount);
    public IReadOnlyList<OverlayWindow> Overlays { get; private set; } = [];
    public bool IsPaused { get; private set; }
    public DesktopLife.Rendering.CreatureStyle Style { get; private set; }
    public void SetStyle(DesktopLife.Rendering.CreatureStyle style)
    {
        _dispatcher.VerifyAccess();
        Style = style;
        foreach (var window in Overlays) window.InsectStyle = style;
    }
    public event Action<System.Windows.Window>? WindowCreated;
    public event Action? LayoutChanged;
    public event Action? StateChanged;
    public event Action? ExitRequested;

    public DesktopHost(Dispatcher dispatcher, Func<IReadOnlyList<DisplayArea>>? getDisplays = null, bool observeMouseClicks = true)
    {
        _dispatcher = dispatcher;
        _getDisplays = getDisplays ?? MonitorService.GetDisplays;
        _observeMouseClicks = observeMouseClicks;
    }

    public void Start()
    {
        _dispatcher.VerifyAccess();
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started) return;
        _started = true;
        if (_observeMouseClicks) _mouseClicks = new GlobalMouseClickSource(Clicks);
        SystemEvents.DisplaySettingsChanged += OnDisplayChanged;
        SynchronizeDisplays();
        _clock.Start();
        Subscribe();
    }

    public void SynchronizeDisplays()
    {
        _dispatcher.VerifyAccess();
        if (_disposed) return;
        var displays = _getDisplays();
        Simulation.Synchronize(displays);
        var old = Overlays.ToDictionary(w => w.Session.Display.Id, StringComparer.Ordinal);
        var next = new List<OverlayWindow>();
        _reconciling = true;
        try
        {
            foreach (var session in Simulation.Worlds)
            {
                if (old.TryGetValue(session.Display.Id, out var window) && ReferenceEquals(window.Session.World, session.World))
                {
                    window.UpdateDisplay(session);
                    old.Remove(session.Display.Id);
                }
                else
                {
                    window = new OverlayWindow(session) { InsectStyle = Style };
                    window.Closed += OnWindowClosed;
                    WindowCreated?.Invoke(window);
                }
                next.Add(window);
            }
            // Install the new snapshot before showing windows (Loaded can run reentrantly).
            Overlays = next.AsReadOnly();
            foreach (var obsolete in old.Values) obsolete.Close();
            foreach (var window in Overlays)
            {
                if (!IsPaused) window.Show();
                window.PlaceOnDisplay();
            }
        }
        finally { _reconciling = false; }
        LayoutChanged?.Invoke();
    }

    public void TogglePause()
    {
        _dispatcher.VerifyAccess();
        if (_disposed) return;
        IsPaused = !IsPaused;
        Clicks.Enabled = !IsPaused;
        if (IsPaused)
        {
            Unsubscribe();
            foreach (var window in Overlays) window.Hide();
        }
        else
        {
            _loop.Reset();
            Simulation.ResetInput();
            foreach (var window in Overlays) { window.Show(); window.PlaceOnDisplay(); }
            Subscribe();
        }
        StateChanged?.Invoke();
    }

    public void SetPopulation(PopulationSettings settings)
    {
        _dispatcher.VerifyAccess();
        ObjectDisposedException.ThrowIf(_disposed, this);
        Simulation.SetPopulation(settings);
        StateChanged?.Invoke();
    }

    private void Subscribe()
    {
        if (_subscribed || IsPaused || _disposed) return;
        _lastRenderingTime = null;
        CompositionTarget.Rendering += OnFrame;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        CompositionTarget.Rendering -= OnFrame;
        _subscribed = false;
    }

    private void OnFrame(object? sender, EventArgs args)
    {
        if (_disposed || IsPaused) return;
        if (args is RenderingEventArgs rendering)
        {
            if (_lastRenderingTime == rendering.RenderingTime) return;
            _lastRenderingTime = rendering.RenderingTime;
        }
        var time = _loop.Tick(_clock.Elapsed.TotalSeconds);
        if (time.DeltaTime <= 0 || !CursorService.TryGetPosition(out var cursor)) return;
        var start = Stopwatch.GetTimestamp();
        Simulation.Update(time.ElapsedSeconds, cursor, Clicks.TakeLatest());
        var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        foreach (var window in Overlays) window.PresentFrame(time, elapsed);
    }

    private void OnDisplayChanged(object? sender, EventArgs args)
    {
        if (_disposed || _dispatcher.HasShutdownStarted || Interlocked.Exchange(ref _refreshQueued, 1) != 0) return;
        _dispatcher.BeginInvoke(new Action(() =>
        {
            Interlocked.Exchange(ref _refreshQueued, 0);
            if (!_disposed) SynchronizeDisplays();
        }));
    }

    private void OnWindowClosed(object? sender, EventArgs args)
    {
        if (!_reconciling && !_disposed) ExitRequested?.Invoke();
    }

    public void Dispose()
    {
        _dispatcher.VerifyAccess();
        if (_disposed) return;
        _disposed = true;
        Clicks.Enabled = false;
        _mouseClicks?.Dispose();
        SystemEvents.DisplaySettingsChanged -= OnDisplayChanged;
        Unsubscribe();
        foreach (var window in Overlays) { window.Closed -= OnWindowClosed; window.Close(); }
        Overlays = [];
        _clock.Stop();
    }
}
