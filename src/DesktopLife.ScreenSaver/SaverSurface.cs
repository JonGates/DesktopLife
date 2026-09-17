using System.Windows;
using System.Windows.Media;
using DesktopLife.Creatures.Displays;
using DesktopLife.Engine.World;
using DesktopLife.Rendering;
namespace DesktopLife.ScreenSaver;

public sealed class SaverSurface(DisplaySimulation simulation, WorldBounds viewport, bool light, bool preview = false, CreatureStyle style = CreatureStyle.Realistic, ImageSource? backgroundImage = null) : FrameworkElement
{
    private readonly WpfCreatureRenderer _renderer = new() { Style = style };
    private readonly Brush _background = new SolidColorBrush(light ? Color.FromRgb(231, 240, 233) : Color.FromRgb(29, 42, 49));
    private readonly Brush? _imageBackground = backgroundImage == null ? null : new ImageBrush(backgroundImage) { Stretch = Stretch.UniformToFill };
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(_background, null, new Rect(RenderSize));
        if (_imageBackground != null) dc.DrawRectangle(_imageBackground, null, new Rect(RenderSize));
        dc.PushClip(new RectangleGeometry(new Rect(RenderSize)));
        var dpi = VisualTreeHelper.GetDpi(this);
        if (preview)
        {
            var scale = Math.Min(ActualWidth / viewport.Width, ActualHeight / viewport.Height);
            dc.PushTransform(new TranslateTransform((ActualWidth - viewport.Width * scale) / 2, (ActualHeight - viewport.Height * scale) / 2));
            dc.PushTransform(new ScaleTransform(scale, scale));
            _renderer.Render(dc, simulation.World.Manager.Creatures, viewport, simulation.World.TotalTime, 1, 1);
            RainGlassRenderer.Render(dc, simulation.World.Rain, viewport, 1, 1);
            dc.Pop(); dc.Pop();
        }
        else
        {
            _renderer.Render(dc, simulation.World.Manager.Creatures, viewport, simulation.World.TotalTime, dpi.DpiScaleX, dpi.DpiScaleY);
            RainGlassRenderer.Render(dc, simulation.World.Rain, viewport, dpi.DpiScaleX, dpi.DpiScaleY);
        }
        dc.Pop();
    }
}
