namespace DesktopLife.Engine.World;

/// <summary>A logical desktop display in physical screen coordinates; device id is stable across enumeration.</summary>
public sealed record DisplayArea(string Id, WorldBounds Bounds, bool IsPrimary = false);
