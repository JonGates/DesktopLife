using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using DesktopLife.Windows;
using DesktopLife.Engine.Time;
using DesktopLife.Creatures.Fly;
using DesktopLife.Creatures.Displays;

namespace DesktopLife.App.Overlay;

public partial class OverlayWindow : Window
{
    private HwndSource? _source;
    private readonly RenderSurface _surface;
    private bool _closed;
    private bool _placementQueued;
    private bool _wasVisible;
    private static readonly string[] StateLabels = ["Offscreen", "Approach", "Orbit", "Panic", "Depart"];
#if DEBUG
    private double _nextIdleDraw;
#endif
    public DisplayWorld Session { get; private set; }

    public void UpdateDisplay(DisplayWorld session)
    {
        if (!ReferenceEquals(Session.World, session.World)) throw new ArgumentException("Cannot replace a window's simulation.", nameof(session));
        Session = session;
        _surface.Viewport = session.Display.Bounds;
        _surface.InvalidateVisual();
    }

    public OverlayWindow(DisplayWorld session)
    {
        Session = session;
        InitializeComponent();
        Title = $"DesktopLife Overlay [{session.Display.Id}]";
        _surface = new RenderSurface(session.World) { Viewport = session.Display.Bounds, ClipToBounds = true };
        Content = _surface;
        SourceInitialized += (_, _) =>
        {
            var handle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(handle);
            _source.AddHook(WindowProc);
            OverlayWindowHelper.Configure(handle, PhysicalBounds);
        };
        Loaded += (_, _) => PlaceOnDisplay();
        Closed += (_, _) => { _closed = true; _source?.RemoveHook(WindowProc); };
    }

    private MonitorBounds PhysicalBounds => new((int)Session.Display.Bounds.Left, (int)Session.Display.Bounds.Top,
        (int)Session.Display.Bounds.Width, (int)Session.Display.Bounds.Height);

    public void PlaceOnDisplay()
    {
        if (_closed) return;
        var handle = new WindowInteropHelper(this).Handle;
        if (handle != 0) OverlayWindowHelper.PlaceOnMonitor(handle, PhysicalBounds);
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        // Reapply physical monitor bounds after WPF processes its suggested DPI rectangle.
        if (_placementQueued || _closed) return;
        _placementQueued = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            _placementQueued = false;
            PlaceOnDisplay();
            if (!_closed) _surface.InvalidateVisual();
        }));
    }

    public void PresentFrame(GameTime time, double updateMs)
    {
        _surface.UpdateMs = updateMs;
        _surface.Seconds = time.TotalTime;
        _surface.Fps = _surface.Fps * 0.9 + (1 / time.ElapsedSeconds) * 0.1;
        var creatures = Session.World.Manager.Creatures;
        _surface.StateLabel = creatures.FirstOrDefault(c => c is FlyCreature) is FlyCreature fly ? StateLabels[(int)fly.State] : "Disabled";
        _surface.CockroachCount = creatures.Count(c => c.Kind == DesktopLife.Engine.Creatures.CreatureKind.Cockroach);
        var visibleCount = 0;
        for (var i = 0; i < creatures.Count; i++)
            if (creatures[i].IsVisible && Session.Display.Bounds.Contains(creatures[i].Position, 40)) visibleCount++;
        _surface.VisibleCount = visibleCount;
        var anyVisible = visibleCount > 0;
        var redraw = anyVisible || _wasVisible;
#if DEBUG
        if (time.TotalTime >= _nextIdleDraw) { redraw = true; _nextIdleDraw = time.TotalTime + 0.25; }
#endif
        if (redraw) _surface.InvalidateVisual();
        _wasVisible = anyVisible;
    }

    private nint WindowProc(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == 0x0084) { handled = true; return -1; }
        if (message == 0x0021) { handled = true; return 3; }
        return 0;
    }
}
