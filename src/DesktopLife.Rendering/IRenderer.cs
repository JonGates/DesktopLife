using System.Windows.Media;
using DesktopLife.Engine.Creatures;
using DesktopLife.Engine.World;
namespace DesktopLife.Rendering;
public interface IRenderer
{
    void Render(DrawingContext dc, IReadOnlyList<ICreature> creatures, WorldBounds bounds, float time, double scaleX, double scaleY);
}
