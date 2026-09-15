using System.Numerics;
namespace DesktopLife.Engine.Input;
public readonly record struct MouseState(Vector2 Position, Vector2 Delta, float Speed, bool IsMoving, TimeSpan IdleTime, MouseClick? Click = null);
