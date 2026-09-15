using System.Numerics;
namespace DesktopLife.Engine.Input;
public sealed class MouseTracker
{
    private bool _initialized;
    public MouseState State { get; private set; }
    public void Reset()
    {
        _initialized = false;
        State = default;
    }
    public void Update(Vector2 position, float elapsedSeconds)
    {
        if (!float.IsFinite(elapsedSeconds) || elapsedSeconds <= 0 || !float.IsFinite(position.X) || !float.IsFinite(position.Y)) return;
        if (!_initialized)
        {
            State = new(position, Vector2.Zero, 0, false, TimeSpan.Zero);
            _initialized = true;
            return;
        }
        var delta = position - State.Position;
        var distance = delta.Length();
        var speed = distance / MathF.Max(elapsedSeconds, 0.0001f);
        var moving = distance >= 1 && speed > 15;
        var idle = moving ? TimeSpan.Zero : State.IdleTime + TimeSpan.FromSeconds(MathF.Min(elapsedSeconds, 3600));
        State = new(position, delta, speed, moving, idle);
    }
}
