namespace DesktopLife.Engine.Time;
public sealed class GameLoop
{
    private double? _previous;
    public GameTime Tick(double seconds)
    {
        if (!double.IsFinite(seconds)) return default;
        var elapsed = _previous.HasValue ? (float)System.Math.Max(0, seconds - _previous.Value) : 0;
        _previous = seconds;
        return new(MathF.Min(elapsed, 0.05f), elapsed, seconds);
    }
    public void Reset() => _previous = null;
}
