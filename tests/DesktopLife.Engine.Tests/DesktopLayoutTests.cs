using System.Numerics;
using DesktopLife.Engine.World;
namespace DesktopLife.Engine.Tests;
public class DesktopLayoutTests
{
    private static DisplayArea Screen(string id, float x, float y, float w = 1000, float h = 800) => new(id, new(x, y, w, h));
    [Theory]
    [InlineData(1000, 0, 995, 400, 1005, 400)]
    [InlineData(-1000, 0, 5, 400, -5, 400)]
    [InlineData(0, -800, 500, 5, 500, -5)]
    [InlineData(0, 800, 500, 795, 500, 805)]
    public void SharedSeamsAllowContinuousMovement(float x, float y, float sx, float sy, float ex, float ey)
    {
        var layout = new DesktopLayout([Screen("a", 0, 0), Screen("b", x, y)]);
        var end = new Vector2(ex, ey);
        Assert.True(layout.Contains(end));
        Assert.Equal(end, layout.ConstrainMove(new(sx, sy), end));
        Assert.True(layout.NearestEdge(new((sx+ex)/2, (sy+ey)/2)).Distance > 100);
    }
    [Fact]
    public void OffsetScreensExposeOnlyUnconnectedPartOfEdge()
    {
        var layout = new DesktopLayout([Screen("a", 0, 0), Screen("b", 1000, 300)]);
        Assert.Equal(new Vector2(1005, 500), layout.ConstrainMove(new(995, 500), new(1005, 500)));
        Assert.True(layout.ConstrainMove(new(995, 100), new(1005, 100)).X < 1000);
        Assert.False(layout.Contains(new(1200, 100)));
        Assert.Equal(5, layout.NearestEdge(new(995, 100)).Distance);
    }
    [Fact]
    public void MovementCannotJumpAcrossSmallGapEvenIfEndpointIsOnOtherScreen()
    {
        var layout = new DesktopLayout([Screen("a", 0, 0), Screen("b", 1002, 0)]);
        Assert.True(layout.ConstrainMove(new(995, 400), new(1010, 400)).X < 1000);
        Assert.False(layout.Contains(new(1001, 400)));
    }
    [Fact]
    public void DiagonalCornerContactIsNotAWalkablePassage()
    {
        var layout = new DesktopLayout([Screen("a", 0, 0), Screen("b", 1000, 800)]);
        Assert.True(layout.ConstrainMove(new(995, 795), new(1005, 805)).X < 1000);
    }
    [Fact]
    public void ProjectionReturnsPointInsideActualScreenNotBoundingBoxVoid()
    {
        var layout = new DesktopLayout([Screen("a", -1000, 0), Screen("b", 0, 300)]);
        Assert.True(layout.Contains(layout.Clamp(new(500, 100))));
        Assert.Equal(new WorldBounds(-1000, 0, 2000, 1100), layout.Bounds);
        Assert.False(layout.Contains(new(500, 100)));
    }
    [Fact]
    public void InvalidGeometryAndDuplicateIdsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new DesktopLayout([Screen("a", 0, 0), Screen("a", 1000, 0)]));
        Assert.Throws<ArgumentException>(() => new DesktopLayout([Screen("a", float.NaN, 0)]));
        Assert.Throws<ArgumentException>(() => new DesktopLayout([Screen("a", 0, 0, 0)]));
    }
}
