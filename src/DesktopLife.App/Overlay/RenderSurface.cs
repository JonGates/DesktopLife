using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;
namespace DesktopLife.App.Overlay;
public sealed class RenderSurface(SimulationWorld world) : FrameworkElement
{
    public WorldBounds Viewport { get; set; } = world.Bounds;
    private readonly DebugHud _hud = new();
    public CreatureStyle InsectStyle { get => _renderer.Style; set { _renderer.Style = value; InvalidateVisual(); } }
    private readonly WpfCreatureRenderer _renderer = new WpfCreatureRenderer();
    public double UpdateMs { get; set; }
    public string StateLabel { get; set; } = "Offscreen";
    public int CockroachCount { get; set; }
    public int VisibleCount { get; set; }
    public double Seconds { get; set; }
    public double Fps { get; set; }
    protected override void OnRender(DrawingContext dc)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        _renderer.Render(dc, world.Manager.Creatures, Viewport, world.TotalTime, dpi.DpiScaleX, dpi.DpiScaleY);
#if DEBUG
        _hud.Draw(dc, ActualWidth, Seconds, Fps, UpdateMs, world.Mouse.State, StateLabel, CockroachCount, VisibleCount, dpi.PixelsPerDip);
#endif
    }
}
