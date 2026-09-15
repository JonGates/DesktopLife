using System.Globalization;
using System.Windows;
using System.Windows.Media;
using DesktopLife.Engine.Input;

namespace DesktopLife.App.Overlay;

internal sealed class DebugHud
{
#if DEBUG
    private readonly Typeface _typeface = new("Consolas");
    private readonly SolidColorBrush _background = new(Color.FromArgb(215, 20, 27, 34));
    private FormattedText? _text;
    private double _nextRefresh;

    public DebugHud() => _background.Freeze();

    public void Draw(DrawingContext dc, double width, double seconds, double fps, double frameMs, MouseState mouse, string state, int cockroachCount, int visibleCount, double pixelsPerDip)
    {
        if (_text is null || seconds >= _nextRefresh)
        {
            _nextRefresh = seconds + 0.25;
            _text = new FormattedText($"DesktopLife DEBUG\nFPS: {fps:F0}  Update: {frameMs:F2}ms\nMouse: {mouse.Position.X:F0}, {mouse.Position.Y:F0} px\nSpeed: {mouse.Speed:F0} px/s\nIdle: {mouse.IdleTime.TotalSeconds:F2}s\nFly: 1  {state}\nCockroach: {cockroachCount}  Total visible: {visibleCount}",
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _typeface, 12, Brushes.White, pixelsPerDip);
        }
        var origin = new Point(Math.Max(8, width - 305), 16);
        dc.DrawRoundedRectangle(_background, null, new Rect(origin.X - 12, origin.Y - 10, 301, 144), 8, 8);
        dc.DrawText(_text, origin);
    }
#endif
}
