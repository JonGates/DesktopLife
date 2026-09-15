using System.Numerics;
namespace DesktopLife.Engine.Input;
/// <summary>A bounded, one-shot mailbox. The latest click wins; pausing drops pending input.</summary>
public sealed class MouseClickBuffer
{
    private readonly object _gate = new();
    private MouseClick? _pending;
    private long _sequence;
    private bool _enabled = true;
    public bool Enabled
    {
        get { lock (_gate) return _enabled; }
        set { lock (_gate) { _enabled = value; _pending = null; } }
    }
    public void Record(Vector2 position)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y)) return;
        lock (_gate) { if (_enabled) _pending = new(++_sequence, position); }
    }
    public MouseClick? TakeLatest()
    {
        lock (_gate) { var result = _pending; _pending = null; return result; }
    }
}
